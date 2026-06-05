using AssetLoad;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
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
        
        GameDataManager.InitAsync();
    }
}