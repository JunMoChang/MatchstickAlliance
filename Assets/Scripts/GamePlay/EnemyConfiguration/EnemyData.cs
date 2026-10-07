using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public GameObject enemyPrefab;
        public string enemyName;
        public float baseHp;
        public float baseDamage;
        public float hpGrowthRate;
        public float damageGrowthRate;
        public float attackCooldown = 1.5f;
        public float moveSpeed = 2f;

    }
}
 