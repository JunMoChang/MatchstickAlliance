using GamePlay.CharacterControllers.RoleData;
using PlayerDataHandle;
using UnityEngine;

namespace GamePlay.CharacterControllers
{
    public class PlayerController : MonoBehaviour
    {
        private readonly SaveManager saveManager =  new ();

        private PlayerData playerData; //玩家数据
        private Transform visualTransform;
        
        private BaseRole[] currentRole = new BaseRole[2];
        private void Awake()
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
