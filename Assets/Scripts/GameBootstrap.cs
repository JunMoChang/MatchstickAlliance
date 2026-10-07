using AssetLoad;
using UnityEngine;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitGame()
    {
        // 只负责点火：加载结果通过 GameDataManager.IsReady / IsFailed / OnReady 获取
        GameDataManager.LoadResources();
    }
}
