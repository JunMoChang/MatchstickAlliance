using UnityEngine;

namespace GamePlay.UI.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        [SerializeField] private float pickupDelay = 0.5f;
        [SerializeField] private float launchSpeedX = 1.5f;
        [SerializeField] private float launchSpeedY = 4f;
        [SerializeField] private float gravity = 12f;
        private float groundY = -3.1f;
        
        private static int RoleLayer;
            
        private Vector2 velocity;
        private bool canPickup;
        private bool grounded;
        
        void Start()
        {
            RoleLayer = LayerMask.NameToLayer("Role");
            
            velocity = new Vector2(Random.Range(-launchSpeedX, launchSpeedX), launchSpeedY);
            Invoke(nameof(EnablePickup), pickupDelay);
        }

        void Update()
        {
            if (grounded) return;
            
            velocity.y -= gravity * Time.deltaTime;
            Vector3 pos = transform.position;
            pos.x += velocity.x * Time.deltaTime;
            pos.y += velocity.y * Time.deltaTime;
            
            if (pos.y <= groundY)
            {
                pos.y = groundY;
                grounded = true;
            }
            
            transform.position = pos;
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (canPickup && other.gameObject.layer == RoleLayer) Collect();    
        }
        
        private void EnablePickup() => canPickup = true;
        
        private void Collect()
        {
            Destroy(gameObject);
        }
        
    }
}