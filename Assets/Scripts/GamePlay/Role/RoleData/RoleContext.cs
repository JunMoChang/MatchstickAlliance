using System;
using GamePlay.Role.RoleBoxCollider;
using GamePlay.Role.RoleStrategy;
using UnityEngine;

namespace GamePlay.Role.RoleData
{
    public class RoleContext : MonoBehaviour
    {
        public RoleBaseData Template { get; private set; }
        public RoleSaveData SaveData { get; private set; }
        public RoleRuntimeData RuntimeData { get; private set; }
        [SerializeField] public Animator animator;
        [SerializeField] public Rigidbody2D rb;
        [SerializeField] public BoxColliderManager boxColliderManager;
        
        private IRoleStrategy strategy;
        
        void Start()
        {
            Vector3 offset = transform.GetComponent<SpriteRenderer>().sprite.bounds.center;
            transform.localPosition = new Vector3(-offset.x, -offset.y / 2, 0);
        }
 
        public void Init(RoleBaseData _template, RoleSaveData _saveData)
        {
            Template = _template;
            SaveData = _saveData;
            RuntimeData = new RoleRuntimeData();
            RuntimeData.Init(_saveData);
            boxColliderManager.InitRuntime(RuntimeData);
        }
        public void SetStrategy(IRoleStrategy s)
        {
            strategy = s;
        }
        
        public void OnMotionEvent(RoleBaseData.MotionName motionName)
        {
            strategy?.OnMotionEvent(motionName);
        }
        
        public void Flip(bool facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            rb.transform.localScale = scale;
        }
    }
}