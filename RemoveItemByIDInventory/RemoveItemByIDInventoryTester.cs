using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace InventoryEngineExtensions
{
    /// <summary>
    /// This test class lets you play with the extended Inventory API.
    /// You can replace the one in the PixelRogueRoom2 demo scene with this to see it in action
    /// </summary>
    public class RemoveItemByIDInventoryTester : InventoryTester
    {
        [Header("Remove Item By ID")] 
        [Tooltip("the ID of the items to remove")]
        public string ItemIDToRemove;
        [Tooltip("the quantity of the specified items to remove")]
        public int QuantityToRemove;
        [Tooltip("the inventory from which to remove the items")]
        public RemoveItemByIDInventory RemoveItemByIDInventory;
        /// a test button
        [MMInspectorButton("RemoveItemByIDTest")] 
        public bool RemoveItemByIDTestButton;
        
        /// <summary>
        /// Test method to remove items with the specified ID
        /// </summary>
        protected void RemoveItemByIDTest()
        {
            RemoveItemByIDInventory.RemoveItemByID(ItemIDToRemove, QuantityToRemove);
        }
    }
}
