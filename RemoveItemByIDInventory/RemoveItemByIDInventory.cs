using MoreMountains.InventoryEngine;

namespace InventoryEngineExtensions
{
    public class RemoveItemByIDInventory : Inventory
    {
        public void RemoveItemByID(string itemID, int quantityToRemove)
        {
            if (quantityToRemove < 1 || itemID == null || itemID == "") return;
            var list = InventoryContains(itemID);
            foreach (var index in list)
            {
                var itemQuantity = Content[index].Quantity;
                RemoveItem(index, quantityToRemove);
                quantityToRemove -= itemQuantity;
                if (quantityToRemove < 1) return;
            }
        }

        public override bool RemoveItem(int i, int quantity)
        {
            return i >= 0 && i < Content.Length && quantity > 0 && base.RemoveItem(i, quantity);
        }
    }
}
