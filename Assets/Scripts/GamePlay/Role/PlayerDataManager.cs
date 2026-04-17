using GamePlay.PlayerDataHandle;
using UnityEngine;

namespace GamePlay.Role
{
    public class PlayerDataManager : MonoBehaviour
    {
        private readonly SaveManager saveManager =  new ();
        public PlayerData PlayerData { get; private set; } //玩家数据
        void Awake()
        {
            PlayerData = saveManager.LoadData();
        }
        
        void OnApplicationQuit()
        {
            saveManager.Save(PlayerData);
            Debug.Log("退出保存完成");
        }
    }
}