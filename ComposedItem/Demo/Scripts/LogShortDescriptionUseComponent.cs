using MoreMountains.InventoryEngine;
using UnityEngine;
public class LogShortDescriptionUseComponent : InventoryItem, IOverridable
{
    public override bool Use(string playerID)
    {
        Debug.Log(ShortDescription);
        return true;
    }
    private class Override : IOverride
    {
        public string Log;
        public Override(InventoryItem item) => Log = item.ShortDescription;
        public IOverridable Apply(IOverridable overridable)
        {
            ((InventoryItem)overridable).ShortDescription = Log;
            return overridable;
        }
    }
    public IOverride NewOverride() => new Override(this);
}
