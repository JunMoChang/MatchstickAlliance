using UnityEngine;

namespace GamePlay.Role.RoleBoxCollider
{
    public class BoxColliderReporter : MonoBehaviour
    {
        private BoxColliderManager manager;
        
        public void SetBoxColliderManager(BoxColliderManager mgr)
        {
            manager = mgr;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            manager.TriggerEnter2D(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            manager.TriggerStay2D(other);
        }
    }
}