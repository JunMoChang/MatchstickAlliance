using UnityEngine;

namespace GamePlay.Role.RoleBoxCollider
{
    public class BoxColliderReporter : MonoBehaviour
    {
        private BoxColliderManager manager;
        private BoxColliderManager.BoxColliderName boxName;

        public void Init(BoxColliderManager mgr, BoxColliderManager.BoxColliderName name)
        {
            manager = mgr;
            boxName = name;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            manager.TriggerEnter2D(boxName, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            manager.TriggerStay2D(boxName, other);
        }
    }
}