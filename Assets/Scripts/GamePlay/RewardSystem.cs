using System;
using System.Collections;
using GamePlay.GameModel.Level;
using UnityEngine;

namespace GamePlay
{
    public class RewardSystem : MonoBehaviour
    {
        public static RewardSystem Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
            
            LevelManager.OnLevelEnded += HandleReward;
        }

        private void HandleReward(Action onFinished)
        {
            StartCoroutine(DropRewardThenCallback(onFinished));
        }

        private IEnumerator DropRewardThenCallback(Action onFinished)
        {
            yield return new WaitForSeconds(1);
            onFinished?.Invoke();
        }
    }
}