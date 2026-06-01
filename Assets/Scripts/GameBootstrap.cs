using System.Collections;
using AssetLoad;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;

    private static bool _sExists;

    private void Awake()
    {
        if (_sExists)
        {
            Destroy(gameObject);
            return;
        }
        _sExists = true;
        DontDestroyOnLoad(gameObject);

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);

        GameDataManager.InitAsync();
        StartCoroutine(WaitForDataReady());
    }

    private IEnumerator WaitForDataReady()
    {
        yield return new WaitUntil(() => GameDataManager.IsReady);

        if (GameDataManager.IsFailed)
        {
            Debug.LogError("GameDataManager: 关键数据加载失败！");
            yield break;
        }

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);
    }
}