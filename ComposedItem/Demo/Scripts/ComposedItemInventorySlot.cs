using System.Linq;
using MoreMountains.InventoryEngine;
public class ComposedItemInventorySlot : InventorySlot
{
    public DurabilityBar DurabilityBar;
    public override void DrawIcon(InventoryItem item, int index)
    {
        base.DrawIcon(item, index);
        if (item is not ComposedItem composedItem || !composedItem.Components.Any(component => component is DurabilityUseComponent))
        {
            DurabilityBar.gameObject.SetActive(false);
            return;
        }
        DurabilityBar.Component = (DurabilityUseComponent)composedItem.Components.First(component => component is DurabilityUseComponent);
        DurabilityBar.gameObject.SetActive(true);
    }
}
