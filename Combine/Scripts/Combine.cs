using System;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Combine
{
    [Serializable]
    public struct CombineRecipe
    {
        public InventoryItem Item1;
        public InventoryItem Item2;
        public InventoryItem Result;
    }

    [CreateAssetMenu]
    public class Combine : ScriptableObject
    {
        public CombineRecipe[] CombineRecipes;
        private Dictionary<(string, string), InventoryItem> _combinationResult;

        private void Awake()
        {
            _combinationResult = new Dictionary<(string, string), InventoryItem>();
            if (CombineRecipes == null) return;
            foreach (var recipe in CombineRecipes)
            {
                if (recipe.Item1 == null || recipe.Item2 == null || recipe.Result == null) return;
                _combinationResult[(recipe.Item1.ItemID, recipe.Item2.ItemID)] = recipe.Result;
            }
        }

        private void OnValidate()
        {
            Awake();
        }

        public bool TryCombineItems(Inventory item1Inventory, int item1Index, Inventory item2Inventory, int item2Index)
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
    }
}
