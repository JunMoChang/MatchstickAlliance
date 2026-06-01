using System;
using UnityEngine;

namespace GamePlay.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "ItemStartTable", menuName = "Inventory/ItemStartTable", order = 2)]
    public class ItemStartScriptObject : ScriptableObject
    {
        public StartTable[] startTables;
        [Serializable]
        public struct StartTable
        {
            public int start;
            public float startMult;
        }
    }
}