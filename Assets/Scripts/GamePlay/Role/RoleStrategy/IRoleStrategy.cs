using GamePlay.Role.RoleData;
using UnityEngine;

namespace GamePlay.Role.RoleStrategy
{
    public interface IRoleStrategy
    {
        void Initialize(RoleContext context);
        /// <summary>
        /// 逻辑 Tick
        /// </summary>
        /// <param name="deltaTime">PVE:Time.deltaTime  PVP:Runer.DeltaTime</param>
        void Tick(float deltaTime);
        /// <summary>
        /// 物理 Tick
        /// </summary>
        /// <param name="deltaTime">PVE:Time.fixedDeltaTime  PVP:Runer.DeltaTime</param>
        void FixedTick(float deltaTime);
        /// <summary>
        /// 移动。返回当前朝向方向
        /// </summary>
        Vector2 Move(Vector2 direction);
        void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context);
        /// <summary>
        /// PvP 模式触发攻击
        /// </summary>
        void PerformAttack();
        void UseSkill(int index);
        /// <summary>
        /// 动作结束（动画事件 EndAction 触发时由 RoleContext 转发调用）
        /// </summary>
        void EndAction();
        /// <summary>
        /// 重载动作配置（Play 模式调参时结构性修改后调用，重建 keyframe 字典）
        /// </summary>
        void ReloadConfig() { }
        /// <summary>
        /// 受击反馈
        /// </summary>
        void OnHit();
        /// <summary>
        /// 当前是否处于不可被受击打断的状态（普攻/技能动作进行中）
        /// </summary>
        bool IsUninterruptible => false;
        void Death();
        /// <summary>
        /// PvP 回合重置：恢复 Death() 禁用的物理/碰撞状态并复位动作状态
        /// </summary>
        void OnRespawn() { }

        void SetPhysicsAuthority(bool hasAuthority) { }
    }
}