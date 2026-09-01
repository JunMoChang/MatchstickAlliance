using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
    public class EnemyGib : MonoBehaviour
    {
        [System.Serializable]
        public struct LimbData
        {
            public SpriteRenderer sr;
            public float forceMin;
            public float forceMax;
            public float torqueRange;
        }

        [SerializeField] private LimbData[] limbs;
        [SerializeField] private GameObject gibContainer;
        [SerializeField] private float gibLifetime = 4f;
        [SerializeField] private float fadeStartRatio = 0.5f;

        public void Explode(Vector2 hitDirection)
        {
            gibContainer.SetActive(true);

            foreach (LimbData limb in limbs)
            {
                Detach(limb, hitDirection);
            }
        }

        private void Detach(LimbData limb, Vector2 hitDir)
        {
            Transform limbTrans = limb.sr.transform;
            limbTrans.SetParent(null);

            Rigidbody2D rb = limbTrans.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;

            CapsuleCollider2D capCol = limbTrans.gameObject.AddComponent<CapsuleCollider2D>();
            capCol.size = new Vector2(limb.sr.bounds.size.x * 0.8f, limb.sr.bounds.size.y * 0.8f);

            limbTrans.gameObject.layer = LayerMask.NameToLayer("Gibs");

            Vector2 rawDir = (hitDir + Random.insideUnitCircle * 0.4f);
            if(rawDir.y < 0) rawDir.y = Mathf.Abs(rawDir.y);
            Vector2 dir = rawDir.normalized;


            float baseForce = Random.Range(limb.forceMin, limb.forceMax);
            Vector2 finalForce = dir * baseForce;
            finalForce.y *= limb.forceMax / (limb.forceMax - limb.forceMin);
            rb.AddForce(finalForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-limb.torqueRange, limb.torqueRange), ForceMode2D.Impulse);

            limbTrans.gameObject.GetComponent<GibLimb>().Init(gibLifetime, fadeStartRatio);
        }
    }
}
