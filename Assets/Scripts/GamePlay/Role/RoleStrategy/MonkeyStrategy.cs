using System.Collections.Generic;
using GamePlay.GameModel;
using UnityEngine;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;

namespace GamePlay.Role.RoleStrategy
{
    [System.Serializable]
    public class MonkeyStrategy : IRoleStrategy
    {
        private Rigidbody2D rb;
        private Animator animator;
        private MonkeyData template;
        private RoleContext roleContext;
        private RoleRuntimeData runtimeData;
        private CapsuleCollider2D bodyCollider;
        private RoleBoxCollider.BoxColliderManager boxColliderManager;
        
        private int combosStep;
        private bool isAttacking;
        private bool pendingAttack;
        private bool facingRight = true;
        private float currentGravityScale = 1f;
        private bool canApplyPhysicalMotion = true;
        
        private Vector2 moveInput;
        private Vector2 currentVelocity;

        private Dictionary<RoleBaseData.ActionName, RoleBaseData.MotionKeyframe[]> motionKeyframes;
        private RoleBaseData.ActionName activeActionName;
        private bool motionSequenceActive;
        
        private Dictionary<RoleBaseData.ActionName, RoleBaseData.HitBoxCommand> hitBoxCommands;
        private RoleBaseData.ActionName activeHitBoxActionName;
        private bool hitBoxSequenceActive;
        private int hitBoxKeyframeIndex;
        private bool isActionActive;
        /// <summary> 瞬时盒命中窗口是否激活 </summary>
        private bool instantWindowActive;
        /// <summary> 当前窗口的瞬时盒 keyframe（携带几何） </summary>
        private RoleBaseData.HitBoxKeyframe activeInstantWindowKeyframe;
        /// <summary> 窗口关闭时刻 </summary>
        private float instantWindowEndTime;
        /// <summary> 激活中的持续盒窗口 </summary>
        private readonly Dictionary<RoleBoxCollider.BoxColliderManager.HitBoxName, RoleBaseData.HitBoxKeyframe> activeContinuousBoxes = new();
        /// <summary> 动作期间是否允许输入控制移动 </summary>
        private bool canMoveDuringAction;
        /// <summary> 自由移动允许结束的动画进度 </summary>
        private float freeMoveEndProgress = 1f;
        /// <summary> 下一个执行的运动索引 </summary>
        private int motionKeyframeIndex;
        /// <summary> 序列启动帧 </summary>
        private int sequenceStartFrame = -1;
        /// <summary> 最近消费的 keyframe 是否为 LinearVelocity（决定插值段是否介入）</summary>
        private bool lastConsumedIsLinear;
        /// <summary> 速度插值起点速度</summary>
        private Vector2 lastLinearVelocity;
        private float lastLinearTime;
        /// <summary>动作触发瞬间的朝向快照</summary>
        private float currentMotionDir = 1f;
        
        public void Initialize(RoleContext context)
        {
            roleContext = context;
            
            rb = roleContext.rb;
            animator = roleContext.animator;
            runtimeData = roleContext.RuntimeData;
            template = (MonkeyData)roleContext.Template;
            boxColliderManager = roleContext.boxColliderManager;
            bodyCollider = roleContext.GetComponent<CapsuleCollider2D>();
            
            currentGravityScale = 1f;

            ReloadConfig();

            roleContext.SetStrategy(this);
        }

        /// <summary>
        /// 重载动作配置：重建位移/攻击盒 keyframe 字典。
        /// Play 模式调参时结构性修改（增删 keyframe/命令）
        /// </summary>
        public void ReloadConfig()
        {
            if(motionKeyframes != null) motionKeyframes.Clear();
            if(hitBoxCommands != null) hitBoxCommands.Clear();

            int len = template.motionCommands.Length;
            motionKeyframes = new Dictionary<RoleBaseData.ActionName, RoleBaseData.MotionKeyframe[]>(len);
            hitBoxCommands = new Dictionary<RoleBaseData.ActionName, RoleBaseData.HitBoxCommand>(len);

            foreach (RoleBaseData.MotionCommand command in template.motionCommands)
            {
                if (command.keyframes == null || command.keyframes.Length == 0) continue;
                motionKeyframes.Add(command.actionName, command.keyframes);
            }

            foreach (RoleBaseData.HitBoxCommand command in template.hitBoxCommands)
            {
                if (command.keyframes == null || command.keyframes.Length == 0) continue;
                hitBoxCommands.Add(command.actionName, command);
            }
        }

        public void Tick(float deltaTime)
        {
            CombosWindows();
            TickMotionSequence();
            TickAttackBoxSequence();
            foreach (RoleRuntimeData.RuntimeSkillData skillData in runtimeData.skillRuntimeData)
            {
                skillData.Tick(deltaTime);
            }
        }

        public void FixedTick(float deltaTime)
        {
            currentVelocity.y += Physics2D.gravity.y * currentGravityScale * deltaTime;

            if (!isActionActive || CanMoveDuringAction())
            {
                currentVelocity.x = moveInput.x * runtimeData.speed;
            }
            /*else if (!(motionSequenceActive && lastConsumedIsLinear))
            {
                // 锁定期（如收招）清空输入残留速度：否则自由移动阶段的最后速度
                // 会一直滑行到技能结束。运动序列驱动期间不清零，避免杀掉其写入的水平速度
                currentVelocity.x = 0f;
            }*/

            Vector2 newPosition = rb.position + currentVelocity * deltaTime;
            newPosition = ApplyGroundConstraint(newPosition);
            rb.MovePosition(newPosition);
        }
        
        public Vector2 Move(Vector2 direction)
        {
            moveInput = direction;
            
            if (!isActionActive || CanMoveDuringAction())
            {
                ApplyFacing(direction.x);
            }
           
            animator.SetFloat(AnimationParameters.Speed, Mathf.Abs(direction.x));
            
            return facingRight ? Vector2.right : Vector2.left;
        }

        private void ApplyFacing(float dirX)
        {
            if (dirX > 0 && !facingRight)
            {
                facingRight = true;
                roleContext.Flip(facingRight);
            }
            else if (dirX < 0 && facingRight)
            {
                facingRight = false;
                roleContext.Flip(facingRight);
            }
        }
        
        /// <summary>
        /// 地面约束
        /// </summary>
        /// <param name="currentPosition">当前位置</param>
        /// <returns>地面位置</returns>
        private Vector2 ApplyGroundConstraint(Vector2 currentPosition)
        {
            Bounds bounds = bodyCollider.bounds;
            
            Vector2 rayOrigin = new Vector2(bounds.center.x, bounds.min.y);
            float rayDistance = roleContext.GroundCheckDistance;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, roleContext.GroundLayer);
            if (hit.collider != null)
            {
                float groundY = hit.point.y - (bounds.min.y - rb.position.y);
                if (currentPosition.y < groundY)
                {
                    currentPosition.y = groundY;
                    currentVelocity.y = Mathf.Max(0f, currentVelocity.y);
                }
            }
            return currentPosition;
        }
        /// <summary>
        /// 瞬间落到地面
        /// </summary>
        private void SnapToGround()
        {
            if (!canApplyPhysicalMotion) return;

            Bounds bounds = bodyCollider.bounds;
            Vector2 rayOrigin = new Vector2(bounds.center.x, bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, roleContext.GroundLayer);
            if (hit.collider == null) return;

            float groundY = hit.point.y - (bounds.min.y - rb.position.y);
            rb.position = new Vector2(rb.position.x, groundY);
            currentVelocity.y = 0f;
        }
        
        public void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.started) PerformAttack();
        }
        public void PerformAttack()
        {
            if (isActionActive && isAttacking)
            {
                pendingAttack = true;
            }
            else if (!isActionActive)
            {
                StartCombo();
            }
        }
        private void StartCombo()
        {
            isActionActive = true;
            isAttacking = true;
            combosStep = 0;
            currentVelocity.x = 0f;
            motionSequenceActive = false;
            hitBoxSequenceActive = false;
            canMoveDuringAction = false;
            currentMotionDir = facingRight ? 1f : -1f;
            
            animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
            animator.SetTrigger(AnimationParameters.NormalAttack);
            
            StartActionSequences(GetActionName(0));
        }
        private void CombosWindows()
        {
            if(!isAttacking) return;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!isAttacking || !stateInfo.IsTag("NormalAttack") || animator.IsInTransition(0)) return;
                
            float progress = stateInfo.normalizedTime; 
            switch (progress)
            {
                case > 0.75f and <= 1f when pendingAttack:
                {
                    pendingAttack = false;
                    if (combosStep < template.maxCombos - 1)
                    {
                        isActionActive = true;
                        combosStep++;
                        currentVelocity.x = 0f;
                        animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
                        StartActionSequences(GetActionName(combosStep));
                    }
                    break;
                }
                case >= 1f:
                {
                    animator.SetInteger(AnimationParameters.NormalCombos, -1);
                    combosStep = 0;
                    currentVelocity = Vector2.zero;
                    EndAction();
                    break;
                }
            }
        }
   
        public void UseSkill(int index)
        {
            if (!runtimeData.skillRuntimeData[index].IsReady) return;
            
            if (isActionActive && !isAttacking) return;
            runtimeData.skillRuntimeData[index].TriggerCooldown();
            
            if (isAttacking)
            {
                ResetActionState();
                combosStep = 0;
                animator.SetInteger(AnimationParameters.NormalCombos, -1);
                SnapToGround();
            }

            isActionActive = true;
            currentVelocity.x = 0f;

            canMoveDuringAction = runtimeData.skillRuntimeData[index].canMoveWhileCasting;
            freeMoveEndProgress = runtimeData.skillRuntimeData[index].freeMoveEndProgress;

            motionSequenceActive = false;
            hitBoxSequenceActive = false;

            currentMotionDir = facingRight ? 1f : -1f;

            animator.SetTrigger(AnimationParameters.SkillTriggers[index]);

            StartActionSequences(GetActionName(index, true));
        }
        
        public void OnHit()
        {
            ResetActionState();
            combosStep = 0;
            currentVelocity = Vector2.zero;

            animator.SetInteger(AnimationParameters.NormalCombos, -1);
            animator.SetTrigger(AnimationParameters.Attacked_Trigger);
        }

        /// <summary>
        /// 普攻/技能动作进行中不可被受击打断
        /// </summary>
        public bool IsUninterruptible => isActionActive;

        public void Death()
        {
            ResetActionState();
            
            isActionActive = true;
            currentVelocity = Vector2.zero;

            bodyCollider.enabled = false;
            rb.simulated = false;

            animator.SetBool(AnimationParameters.Death_Bool, true);
        }

        public void OnRespawn()
        {
            rb.simulated = true;
            if (bodyCollider != null) bodyCollider.enabled = true;
            
            ResetActionState();
            
            combosStep = 0;
            currentVelocity = Vector2.zero;
            currentGravityScale = 1f;
            moveInput = Vector2.zero;
            facingRight = true;
            roleContext.Flip(true);

            animator.SetBool(AnimationParameters.Death_Bool, false);
            animator.SetInteger(AnimationParameters.NormalCombos, -1);
            animator.SetFloat(AnimationParameters.Speed, 0f);
        }

        public void SetPhysicsAuthority(bool hasAuthority)
        {
            canApplyPhysicalMotion = hasAuthority;
        }
        /// <summary>
        /// 重置并启动位移/碰撞盒序列
        /// </summary>
        private void StartActionSequences(RoleBaseData.ActionName actionName)
        {
            sequenceStartFrame = Time.frameCount;
            
            ApplyFacing(moveInput.x);
            currentMotionDir = facingRight ? 1f : -1f;
            
            motionSequenceActive = false;
            hitBoxSequenceActive = false;
            lastConsumedIsLinear = false;
            
            DisableAllHitBoxes();

            if (motionKeyframes.TryGetValue(actionName, out RoleBaseData.MotionKeyframe[] motionFrames))
            {
                motionSequenceActive = true;
                activeActionName = actionName;
                motionKeyframeIndex = 0;

                if (motionFrames.Length > 0 && motionFrames[0].normalizedTime <= 0.005f)
                {
                    ApplyMotion(motionFrames[0].motionData);
                    
                    if (motionFrames[0].motionData.motionType == RoleBaseData.MotionData.MotionType.LinearVelocity)
                    {
                        lastLinearVelocity = motionFrames[0].motionData.velocity;
                        lastLinearTime = motionFrames[0].normalizedTime;
                        lastConsumedIsLinear = true;
                    }
                    motionKeyframeIndex = 1;
                }
            }
            if (hitBoxCommands.TryGetValue(actionName, out RoleBaseData.HitBoxCommand hitCmd))
            {
                RoleBaseData.HitBoxKeyframe[] hitFrames = hitCmd.keyframes;
                hitBoxSequenceActive = true;
                activeHitBoxActionName = actionName;
                hitBoxKeyframeIndex = 0;

                if (hitFrames.Length > 0 && hitFrames[0].normalizedTime <= 0.005f)
                {
                    ApplyAttackBox(hitFrames[0]);
                    hitBoxKeyframeIndex = 1;
                }
            }
        }

        private void TickMotionSequence()
        {
            if(!motionSequenceActive) return;
            // 启动帧跳过
            // 该帧时钟仍属旧状态（连击步进时旧状态是动作 tag，守卫不会拦截）
            // 会把新序列的 keyframe 按旧时钟提前消费
            if (Time.frameCount == sequenceStartFrame) return;
            float? progress = GetActionProgress();
            if (progress == null) return;

            RoleBaseData.MotionKeyframe[] frames = motionKeyframes[activeActionName];

            while (motionKeyframeIndex < frames.Length && progress >= frames[motionKeyframeIndex].normalizedTime)
            {
                RoleBaseData.MotionKeyframe keyframe = frames[motionKeyframeIndex];
                if (keyframe.motionData.motionType == RoleBaseData.MotionData.MotionType.LinearVelocity)
                {
                    lastLinearVelocity = keyframe.motionData.velocity;
                    lastLinearTime = keyframe.normalizedTime;
                    lastConsumedIsLinear = true;
                }
                else
                {
                    ApplyMotion(keyframe.motionData);
                    lastConsumedIsLinear = false;
                }
                motionKeyframeIndex++;
            }

            // 速度插值（仅当最近消费的 keyframe 是 LinearVelocity 时介入）
            // - 下一个未消费 keyframe 也是 LinearVelocity：相邻控制点之间按进度 Lerp
            // - 否则：保持最近控制点的值（LinearVelocity 消费时不再瞬时写入，靠这里持续保持）
            // 其余类型（Clear/AddForce/MovePosition/GravityScale）消费后本段不写速度，
            // 插值不跨越它们执行，避免覆盖其执行结果（如 Clear 后速度被重新写回）
            if (lastConsumedIsLinear)
            {
                bool nextIsLinear = motionKeyframeIndex < frames.Length &&
                    frames[motionKeyframeIndex].motionData.motionType == RoleBaseData.MotionData.MotionType.LinearVelocity;

                if (nextIsLinear)
                {
                    RoleBaseData.MotionKeyframe next = frames[motionKeyframeIndex];
                    float span = next.normalizedTime - lastLinearTime;
                    if (span > 0.0001f)
                    {
                        float t = Mathf.Clamp01((progress.Value - lastLinearTime) / span);
                        Vector2 v = Vector2.Lerp(lastLinearVelocity, next.motionData.velocity, t);
                        currentVelocity = new Vector2(v.x * currentMotionDir, v.y);
                    }
                }
                else
                {
                    currentVelocity = new Vector2(lastLinearVelocity.x * currentMotionDir, lastLinearVelocity.y);
                }
            }

            if (motionKeyframeIndex >= frames.Length) motionSequenceActive = false;
        }
        private void ApplyMotion(RoleBaseData.MotionData motion)
        {
            switch (motion.motionType)
            {
                case RoleBaseData.MotionData.MotionType.LinearVelocity:
                    currentVelocity = new Vector2(motion.velocity.x * currentMotionDir, motion.velocity.y);
                    break;
                case RoleBaseData.MotionData.MotionType.MovePosition:
                    if (canApplyPhysicalMotion)
                    {
                        rb.position += new Vector2(currentMotionDir * motion.offset.x, motion.offset.y);
                    }
                    break;
                case RoleBaseData.MotionData.MotionType.AddForce:
                    currentVelocity += new Vector2(currentMotionDir * motion.force.x, motion.force.y);
                    break;
                case RoleBaseData.MotionData.MotionType.GravityScale:
                    currentGravityScale = motion.gravityScale;
                    break;
                case RoleBaseData.MotionData.MotionType.ClearVelocity:
                    currentVelocity = Vector2.zero;
                    break;
            }
        }

        private void TickAttackBoxSequence()
        {
            TickInstantHitWindow();
            TickContinuousWindows();

            if(!hitBoxSequenceActive) return;
            if (Time.frameCount == sequenceStartFrame) return;
            
            float? progress = GetActionProgress();
            if (progress == null) return;

            RoleBaseData.HitBoxKeyframe[] frames = hitBoxCommands[activeHitBoxActionName].keyframes;

            while (hitBoxKeyframeIndex < frames.Length && progress >= frames[hitBoxKeyframeIndex].normalizedTime)
            {
                ApplyAttackBox(frames[hitBoxKeyframeIndex]);
                hitBoxKeyframeIndex++;
            }

            if (hitBoxKeyframeIndex >= frames.Length) hitBoxSequenceActive = false;
        }
        private void ApplyAttackBox(RoleBaseData.HitBoxKeyframe keyframe)
        {
            switch (keyframe.action)
            {
                case RoleBaseData.HitBoxAction.EnableBox:
                    EnableContinuousBox(keyframe);
                    break;
                case RoleBaseData.HitBoxAction.DisableBox:
                    DisableContinuousBox(keyframe.boxName);
                    break;
                case RoleBaseData.HitBoxAction.InstantHit:
                    OpenInstantHitWindow(keyframe);
                    break;
            }
        }

        /// <summary>
        /// 开启持续检测窗口（keyframe 携带几何，Enable/Disable 用 boxName 配对）
        /// </summary>
        private void EnableContinuousBox(RoleBaseData.HitBoxKeyframe keyframe)
        {
            boxColliderManager.ClearContinuousState();
            activeContinuousBoxes[keyframe.boxName] = keyframe;
        }
        private void DisableContinuousBox(RoleBoxCollider.BoxColliderManager.HitBoxName boxName)
        {
            activeContinuousBoxes.Remove(boxName);
        }
        /// <summary>
        /// 持续检测窗口Tick
        /// </summary>
        private void TickContinuousWindows()
        {
            if (activeContinuousBoxes.Count == 0) return;

            foreach (RoleBaseData.HitBoxKeyframe keyframe in activeContinuousBoxes.Values)
            {
                boxColliderManager.ContinuousHitWindow(keyframe);
            }
        }

        /// <summary>
        /// 打开瞬时检测窗口（keyframe 携带几何与窗口时长）
        /// </summary>
        private void OpenInstantHitWindow(RoleBaseData.HitBoxKeyframe keyframe)
        {
            float windowSeconds = keyframe.windowSeconds;

            boxColliderManager.ClearInstantWindow();
            instantWindowActive = true;
            activeInstantWindowKeyframe = keyframe;
            instantWindowEndTime = Time.time + windowSeconds;

            boxColliderManager.InstantHitWindow(keyframe);
        }
        /// <summary>
        /// 瞬时检测窗口Tick
        /// </summary>
        private void TickInstantHitWindow()
        {
            if (!instantWindowActive) return;

            if (Time.time >= instantWindowEndTime || GetActionProgress() == null)
            {
                CloseInstantWindow();
                return;
            }

            boxColliderManager.InstantHitWindow(activeInstantWindowKeyframe);
        }
        
        /// <summary>
        /// 取当前动作的动画进度
        /// </summary>
        private float? GetActionProgress()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool currentIsAction = stateInfo.IsTag("Skill") || stateInfo.IsTag("NormalAttack");

            if (animator.IsInTransition(0))
            {
                AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
                bool nextIsAction = nextInfo.IsTag("Skill") || nextInfo.IsTag("NormalAttack");
                
                if (currentIsAction && !nextIsAction) return stateInfo.normalizedTime;
                // 进入过渡（含动作→动作的连击步进）：目标动作从过渡开始计时，用 next 的时钟
                // 前提：本 controller 的动作→动作过渡只有连击前进步，序列必然属于 next
                // （旧段序列在步进前已消费完）；若未来加入动作间其他互跳过渡需重新审视
                if (nextIsAction) return nextInfo.normalizedTime;
                return null;
            }

            return currentIsAction ? stateInfo.normalizedTime : null;
        }
        
        /// <summary>
        /// 动作结束复位，由 Aseprite设置帧事件
        /// 技能动画末尾帧配置 event:EndAction
        /// </summary>
        public void EndAction()
        {
            ResetActionState();
            
            ApplyFacing(moveInput.x);
        }
        
        private bool CanMoveDuringAction()
        {
            if (!canMoveDuringAction) return false;
            float? progress = GetActionProgress();
            canMoveDuringAction = progress == null || progress < freeMoveEndProgress;
            if(!canMoveDuringAction) currentVelocity = Vector2.zero;
            return canMoveDuringAction;
        }
        
        /// <summary>
        /// 动作状态核心复位
        /// </summary>
        private void ResetActionState()
        {
            motionSequenceActive = false;
            hitBoxSequenceActive = false;

            DisableAllHitBoxes();
            
            isActionActive = false;
            canMoveDuringAction = false;
            freeMoveEndProgress = 1f;
            isAttacking = false;
            pendingAttack = false;
        }
        private void CloseInstantWindow()
        {
            instantWindowActive = false;
            boxColliderManager.ClearInstantWindow();
        }
        private void DisableAllHitBoxes()
        {
            CloseInstantWindow();
            activeContinuousBoxes.Clear();
        }
        
        /// <summary>
        /// 按连击段/技能索引取动作名
        /// </summary>
        private RoleBaseData.ActionName GetActionName(int index, bool isSkill = false)
        {
            return (RoleBaseData.ActionName)((int)(isSkill ? RoleBaseData.ActionName.Skill_1 : RoleBaseData.ActionName.Normal_1) + index);
        }
    }
}