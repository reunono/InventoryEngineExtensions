using System;
using MoreMountains.InventoryEngine;
using UnityEngine;

public class RecoverHealthComponent : InventoryItem, IOverridable
{
    [SerializeField] private float Health;
    public override bool Use(string playerID)
    {
        Debug.Log("Recovered "+(int)Health+" HP");
        return true;
    }
    [Serializable]
    private class SerializedComponent : IOverride
    {
        [SerializeField] private float Health;
        public SerializedComponent(RecoverHealthComponent component) => Save(component);
        public void Save(RecoverHealthComponent component) => Health = component.Health;
        public void Load(RecoverHealthComponent component) => component.Health = Health;
        IOverridable IOverride.Apply(IOverridable overridable)
        {
            Load((RecoverHealthComponent)overridable);
            return overridable;
        }
    }
    IOverride IOverridable.NewOverride() => new SerializedComponent(this);
}
