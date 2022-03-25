using System;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace Combine
{
    public struct TryCombineEvent
    {
        public Inventory Item1Inventory;
        public int Item1Index;
        public Inventory Item2Inventory;
        public int Item2Index;
        private static TryCombineEvent e;

        public static void Trigger(Inventory item1Inventory, int item1Index, Inventory item2Inventory, int item2Index)
        {
            e.Item1Inventory = item1Inventory;
            e.Item1Index = item1Index;
            e.Item2Inventory = item2Inventory;
            e.Item2Index = item2Index;
            MMEventManager.TriggerEvent(e);
        }
    }
    
    public struct CombinedEvent
    {
        private static CombinedEvent e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(e);
        }
    }

    [Serializable]
    public struct CombineRecipe
    {
        public InventoryItem Item1;
        public InventoryItem Item2;
        public InventoryItem Result;
    }

    [CreateAssetMenu]
    public class Combine : ScriptableObject, MMEventListener<TryCombineEvent>
    {
        public CombineRecipe[] CombineRecipes;
        private Dictionary<(string, string), InventoryItem> _combinationResult;

        private void OnValidate()
        {
            _combinationResult = new Dictionary<(string, string), InventoryItem>();
            foreach (var recipe in CombineRecipes)
            {
                if (recipe.Item1 == null || recipe.Item2 == null || recipe.Result == null) return;
                _combinationResult[(recipe.Item1.ItemID, recipe.Item2.ItemID)] = recipe.Result;
            }
        }

        private bool TryCombineItems(Inventory item1Inventory, int item1Index, Inventory item2Inventory, int item2Index)
        {
            var item1ID = item1Inventory.Content[item1Index].ItemID;
            var item2ID = item2Inventory.Content[item2Index].ItemID;
            if (!_combinationResult.TryGetValue((item1ID, item2ID), out var result) &&
                !_combinationResult.TryGetValue((item2ID, item1ID), out result)) return false;
            item1Inventory.RemoveItem(item1Index, 1);
            item2Inventory.RemoveItem(item2Index, 1);
            item1Inventory.AddItem(result, 1);
            if (item1Inventory.InventoryType == Inventory.InventoryTypes.Equipment) result.Equip("Player1");
            return true;
        }

        public void OnMMEvent(TryCombineEvent tryCombineEvent)
        {
            if (TryCombineItems(tryCombineEvent.Item1Inventory, tryCombineEvent.Item1Index, tryCombineEvent.Item2Inventory, tryCombineEvent.Item2Index))
                CombinedEvent.Trigger();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }
        
        private void OnDisable()
        {
            this.MMEventStopListening();
        }
    }
}
