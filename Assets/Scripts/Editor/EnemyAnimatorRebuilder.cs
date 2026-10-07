using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GamePlay.EditorTools
{
    /// <summary>
    /// 重建敌人 Animator 控制器（Sprite-0002.controller）的动画状态机。
    /// 背景：该控制器此前只有参数（Speed/Attack/Hit/Dead），状态机层外链到
    /// aseprite 导入器自动生成的"顺序预览"状态机，不响应任何参数转换，
    /// 导致 EnemyContext 触发 Attack/Hit 后无动画播放、IsName 判断永不成立、
    /// 敌人逻辑状态卡死在 Attack/HitReaction。
    /// 修复：重建独立状态机（Idle/Move/Attack/HitReaction），状态名与
    /// EnemyContext.cs 的 IsName 约定严格一致，并把锤子怪预制体的 Animator
    /// 引用切换到新控制器。
    /// </summary>
    public static class EnemyAnimatorRebuilder
    {
        private const string EnemyAnimRoot = "Assets/Characters/Enemies/Animations";
        private const string ControllerPath = EnemyAnimRoot + "/Sprite-0002.controller";
        private const string EnemyPrefabPath = "Assets/Characters/Enemies/Prefabs/锤子怪.prefab";

        [MenuItem("Tools/敌人动画/重建敌人控制器状态机（锤子怪）")]
        public static void Rebuild()
        {
            // 1. 加载手工动画 clip（引用 aseprite 导入帧，已确认有效）
            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EnemyAnimRoot + "/Idle.anim");
            AnimationClip moveClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EnemyAnimRoot + "/Move.anim");
            AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EnemyAnimRoot + "/Attack.anim");
            AnimationClip hitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EnemyAnimRoot + "/HIt.anim");

            if (idleClip == null || moveClip == null || attackClip == null || hitClip == null)
            {
                Debug.LogError("[EnemyAnimRebuild] 动画 clip 加载失败，请确认 Enemies/Animations 下存在 Idle/Move/Attack/HIt.anim");
                return;
            }

            // 2. 删除旧控制器（重建会生成新 guid，预制体引用随后重设；
            //    全工程仅锤子怪.prefab 引用它，见 guid 4a1040fe 检索）
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null
                && !AssetDatabase.DeleteAsset(ControllerPath))
            {
                Debug.LogError("[EnemyAnimRebuild] 删除旧控制器失败：" + ControllerPath);
                return;
            }

            // 3. 同路径重建控制器
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            if (controller == null)
            {
                Debug.LogError("[EnemyAnimRebuild] 创建控制器失败：" + ControllerPath);
                return;
            }

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Trigger);

            // 4. 建立状态：状态名与 EnemyContext.cs 的 IsName 判断一致（Attack / HitReaction）
            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState idleState = sm.AddState("Idle");
            idleState.motion = idleClip;

            AnimatorState moveState = sm.AddState("Move");
            moveState.motion = moveClip;

            AnimatorState attackState = sm.AddState("Attack");
            attackState.motion = attackClip;

            AnimatorState hitState = sm.AddState("HitReaction");
            hitState.motion = hitClip;

            sm.defaultState = idleState;

            // 5. 转换：
            //    - Speed 控制 Idle/Move（EnemyContext Chase 时 SetFloat(Speed,1)，
            //      其余状态置 -1；初始 0 保持 Idle）
            //    - Attack / Hit trigger 可从任意状态触发（受击可打断追击与攻击）
            AnimatorStateTransition toMove = idleState.AddTransition(moveState);
            toMove.hasExitTime = false;
            toMove.duration = 0f;
            toMove.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Speed");

            AnimatorStateTransition toIdle = moveState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.5f, "Speed");

            AnimatorStateTransition anyToAttack = sm.AddAnyStateTransition(attackState);
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0f;
            anyToAttack.canTransitionToSelf = false;
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            AnimatorStateTransition anyToHit = sm.AddAnyStateTransition(hitState);
            anyToHit.hasExitTime = false;
            anyToHit.duration = 0f;
            anyToHit.canTransitionToSelf = false;
            anyToHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

            // Attack / HitReaction 均不循环：clip 播完后自动退回 Idle（停靠状态），
            // 使下次 Attack/Hit trigger 能真正从 Idle 重新进入（canTransitionToSelf=false
            // 会拦截身处同状态时的重触发，若无退出转换则动画将永久定格在末帧）
            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1f;
            attackToIdle.duration = 0f;

            AnimatorStateTransition hitToIdle = hitState.AddTransition(idleState);
            hitToIdle.hasExitTime = true;
            hitToIdle.exitTime = 1f;
            hitToIdle.duration = 0f;

            // 6. 切换锤子怪预制体的 Animator 引用并保存
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[EnemyAnimRebuild] 找不到敌人预制体：" + EnemyPrefabPath);
                return;
            }
            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("[EnemyAnimRebuild] 敌人预制体根节点缺少 Animator");
                return;
            }
            animator.runtimeAnimatorController = controller;
            PrefabUtility.SavePrefabAsset(prefabRoot);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[EnemyAnimRebuild] 完成：已重建 " + ControllerPath + "（Idle/Move/Attack/HitReaction）并更新锤子怪.prefab 引用");
        }
    }
}
