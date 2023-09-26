using System;
using MoreMountains.InventoryEngine;
using UnityEngine;

public class RecoverManaComponent : InventoryItem, IOverridable
{
    [SerializeField] private float Mana;
    public override bool Use(string playerID)
    {
        Debug.Log("Recovered "+(int)Mana+" MP");
        return true;
    }
    [Serializable]
    private class SerializedComponent : IOverride
    {
        [SerializeField] private float Mana;
        public SerializedComponent(RecoverManaComponent component) => Save(component);
        public void Save(RecoverManaComponent component) => Mana = component.Mana;
        public void Load(RecoverManaComponent component) => component.Mana = Mana;
        IOverridable IOverride.Apply(IOverridable overridable)
        {
            Load((RecoverManaComponent)overridable);
            return overridable;
        }
    }
    IOverride IOverridable.NewOverride() => new SerializedComponent(this);
}
