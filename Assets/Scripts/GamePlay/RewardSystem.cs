using System;
using System.Collections;
using GamePlay.GameModel.Level;
using GamePlay.PlayerDataHandle;
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
            PlayerDataManager pdm = PlayerDataManager.Instance;
           
            ChapterData chapter = LevelContext.CurrentChapter;
            LevelData level = LevelContext.CurrentLevel;
            
            pdm.AddGold(chapter.GetGold(level.levelIndex));
            pdm.AddDiamond(chapter.DiamondsPerLevel);
            
            if (pdm.IsLeveFirstPass(chapter.chapter, level.levelIndex + 1))
            {
                pdm.AddDiamond(chapter.DiamondsFirstLevel);
            }

            DropManager.Instance.CommitEquipmentsToInventory();
            yield return new WaitForSeconds(0.5f);
            onFinished?.Invoke();
        }
        
        
    }
}
