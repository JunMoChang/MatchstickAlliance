using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "EquipmentPool", menuName = "Inventory/EquipmentPool", order = 0)]
    public class EquipmentPool : ScriptableObject
    {
        public static EquipmentPool Instance { get; private set; }
        
        public EquipmentData[] equipmentsData;
        private Dictionary<ItemScriptableObject.ItemName, ItemScriptableObject> equipmentsDictionary;
        
        [Serializable]
        public struct EquipmentData
        {
            public int star;
            public EquipmentEntry[] equipments;
        }
        
        [Serializable]
        public struct EquipmentEntry
        {
            public ItemScriptableObject item;
            public float weight;
        }

        void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("EquipmentPool 存在多个实例，请检查资产配置");
                return;
            }
            
            Instance = this;
            Initialize();
        }
        
        private void Initialize()
        {
            equipmentsDictionary = new (equipmentsData.Length);
            foreach (EquipmentData itemData in equipmentsData)
            {
                foreach (EquipmentEntry equipment in itemData.equipments)
                {
                    equipmentsDictionary.TryAdd(equipment.item.itemName, equipment.item);
                }
                
            }
        }
        public ItemScriptableObject RollEquipment(int star)
        {
            EquipmentData pool = equipmentsData[star - 1];
            
            float total = 0;
            foreach (EquipmentEntry entry in pool.equipments)
            { 
                total += entry.weight;
            }
            
            if (total <= 0) return null;
            
            float chance = UnityEngine.Random.Range(0f, total);
            float access = 0;
            foreach (EquipmentEntry entry in pool.equipments)
            {
                access += entry.weight;
                if (chance <= access) return entry.item;
            }
            
            return null;
        }
        
        public ItemScriptableObject FindItemScriptableObject(ItemScriptableObject.ItemName itemName)
        {
            return equipmentsDictionary.GetValueOrDefault(itemName);
        }
    }
}