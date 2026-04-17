using GamePlay.Role.RoleBoxCollider;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using UnityEngine;

namespace GamePlay.Role
{
    public class RoleContext : MonoBehaviour
    {
        [SerializeField] public RoleBaseData roleData;
        [SerializeField] public Animator animator;
        [SerializeField] public Rigidbody2D rb;
        [SerializeField] public BoxColliderManager boxColliderManager;
        
        private IRoleStrategy strategy;

        void Start()
        {
            Vector3 offset = transform.GetComponent<SpriteRenderer>().sprite.bounds.center;
            transform.localPosition = new Vector3(-offset.x, -offset.y / 2, 0);
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