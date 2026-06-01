using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using UnityEngine;

namespace GamePlay.UI.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "EquipmentPool", menuName = "Inventory/EquipmentPool", order = 0)]
    public class EquipmentPool : ScriptableObject
    {
        public EquipmentData[] equipmentsData;
        private Dictionary<ItemScriptableObject.ItemName, ItemScriptableObject> equipmentsDictionary;
        private bool _initialized;
        
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

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

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