using System;
using System.Linq;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Craft
{
    [Serializable]
    public abstract class DisplayNiceNameInInspectorList : ISerializationCallbackReceiver
    {
        [HideInInspector]
        [SerializeField]
        protected string _name;

        protected abstract void SetName();

        public void OnBeforeSerialize()
        {
            SetName();
        }
 
        public void OnAfterDeserialize()
        {
            SetName();
        }
    }
    
    [Serializable]
    public class Ingredient : DisplayNiceNameInInspectorList
    {
        public InventoryItem Item;
        public int Quantity;

        protected override void SetName()
        {
            _name = Quantity + " x " + (Item == null ? "null" : Item.ItemName);
        }

        public override string ToString()
        {
            return Quantity + " " + Item.ItemName + (Quantity > 1 ? "s" : "");
        }
    }

    [Serializable]
    public class Recipe : DisplayNiceNameInInspectorList
    {
        public Ingredient[] Ingredients;
        public InventoryItem Result;
        public string IngredientsText => string.Join(", ", Ingredients.Select(ingredient => ingredient.ToString()));
     
        protected override void SetName()
        {
            _name = Result == null ? "null" : Result.ItemName;
        }
    }

    public static class Crafting
    {
        public static bool CanCraft(this Inventory inventory, Recipe recipe)
        {
            return !recipe.Ingredients.Any(ingredient => inventory.InventoryContains(ingredient.Item.ItemID).Sum(index => inventory.Content[index].Quantity) < ingredient.Quantity);
        }
        
        public static void Craft(this Inventory inventory, Recipe recipe)
        {
            if (!inventory.CanCraft(recipe)) return;
            foreach (var ingredient in recipe.Ingredients)
                inventory.RemoveItemByID(ingredient.Item.ItemID, ingredient.Quantity);
            inventory.AddItem(recipe.Result, 1);
        }
    }
    
    [CreateAssetMenu]
    public class Craft : ScriptableObject
    {
        public Recipe[] Recipes;
    }
}
