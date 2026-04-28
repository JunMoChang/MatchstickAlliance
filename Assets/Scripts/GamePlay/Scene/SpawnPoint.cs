using UnityEngine;

namespace GamePlay.Scene
{
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}