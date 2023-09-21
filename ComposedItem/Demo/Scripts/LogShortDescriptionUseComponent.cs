using MoreMountains.InventoryEngine;
using UnityEngine;
public class LogShortDescriptionUseComponent : InventoryItem
{
    public override bool Use(string playerID)
    {
        Debug.Log(ShortDescription);
        return true;
    }
}
