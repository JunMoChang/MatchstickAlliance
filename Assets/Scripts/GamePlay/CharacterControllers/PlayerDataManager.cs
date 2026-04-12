using PlayerDataHandle;
using UnityEngine;

namespace GamePlay.CharacterControllers
{
    public class PlayerDataManager :  MonoBehaviour
    {
        private readonly SaveManager saveManager =  new ();
        private PlayerData playerData; //玩家数据
        void Awake()
        {
            playerData = saveManager.LoadData();
        }
        
        void OnApplicationQuit() 
        {
            saveManager.Save(playerData);
            Debug.Log("退出保存完成");
        }
    }
}