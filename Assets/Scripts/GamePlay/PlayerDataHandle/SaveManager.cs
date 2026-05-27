using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace GamePlay.PlayerDataHandle
{
    public class SaveManager
    {
        private class IgnoreVector2NormalizedResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
        
                if (property.DeclaringType == typeof(Vector2) && property.PropertyName == "normalized")
                {
                    property.ShouldSerialize = instance => false;
                }
        
                return property;
            }
        }
        private string SavePath => Application.persistentDataPath + "/saveData.json";
        private readonly JsonSerializerSettings setting = new ()
        {
            ContractResolver = new IgnoreVector2NormalizedResolver(),
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = {new Newtonsoft.Json.Converters.StringEnumConverter()}
        };
        
        public void Save(PlayerData data) 
        {
            data.saveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, setting);
            File.WriteAllText(SavePath, json);
        }
        
        public PlayerData LoadData() 
        {
            if (!File.Exists(SavePath)) {
                return new PlayerData();
            }

            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<PlayerData>(json, setting);
        }
        
        
    }
}