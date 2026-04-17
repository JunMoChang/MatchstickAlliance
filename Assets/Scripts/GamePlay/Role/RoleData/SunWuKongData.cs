using System;
using UnityEngine;

namespace GamePlay.Role.RoleData
{
    [CreateAssetMenu(fileName = "SunWuKongData", menuName = "RoleData/孙悟空")]
    public class SunWuKongData : RoleBaseData
    {
        public int maxCombos = 5;
        public readonly float[] skillCooldowns = {3,5,0,12};
        
        public float skill1TeleportDistance;
        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;
        public float skill3TeleportDistance = 2.2f;
        public float skill3TeleportForceX = 1.5f;
        public float skill3TeleportForceY = 2.6f;
        
        public float finalComboJumpForce = 8f;
        public float finalComboExtraFallSpeed = 2f;
        public float finalComboDefaultHorizontal = 2f;
        public float finalComboFastHorizontal = 4f;
        public float finalComboSlowHorizontal = 1f;
        
        public MotionCommand[] motionCommands;
    }
}