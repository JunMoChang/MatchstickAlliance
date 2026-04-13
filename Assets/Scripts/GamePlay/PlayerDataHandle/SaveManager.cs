using System.IO;
using GamePlay.CharacterControllers;
using Newtonsoft.Json;
using UnityEngine;

namespace GamePlay.PlayerDataHandle
{
    public class SaveManager
    {
        private string SavePath => Application.persistentDataPath + "/saveData.json";
        private readonly JsonSerializerSettings setting = new () { TypeNameHandling = TypeNameHandling.Auto };
        
        public void Save(PlayerData data) 
        {
            data.saveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, setting);
            File.WriteAllText(SavePath, json);

            Debug.Log($"已保存到: {SavePath}");
        }
        
        public PlayerData LoadData() 
        {
            if (!File.Exists(SavePath)) {
                Debug.Log("未找到存档");
                return new PlayerData();
            }

            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<PlayerData>(json, setting);
        }
        
        
    }
}