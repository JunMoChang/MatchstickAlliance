using GamePlay.GameModel;
using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
    public class EnemyContext : MonoBehaviour, IDamageable
    {
        public float Hp { get; private set; }
        public float Damage { get; private set; }

        public void Init(float hp, float damage)
        {
            Hp = hp;
            Damage = damage;
        }
        
        public void TakeDamage(float damage)
        {
            Hp -= damage;
            if (Hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}