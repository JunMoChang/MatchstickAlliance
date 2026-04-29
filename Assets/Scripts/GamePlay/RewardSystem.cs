using System;
using System.Collections;
using GamePlay.GameModel.Level;
using UnityEngine;

namespace GamePlay
{
    public class RewardSystem : MonoBehaviour
    {
        void Awake()
        {
            LevelManager.OnLevelEnded += HandleReward;
        }

        void OnDestroy()
        {
            LevelManager.OnLevelEnded -= HandleReward;
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