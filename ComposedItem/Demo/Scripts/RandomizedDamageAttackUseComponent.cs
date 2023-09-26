using System;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizedDamageAttackUseComponent : InventoryItem, IJsonSerializable, IInitializable, IOverridable
{
    [HideInInspector] public float Damage;
    [MMVector("Min", "Max")] public Vector2 DamageRange = new Vector2(10, 50);
    public override bool Use(string playerID)
    {
        Debug.Log("Performed an attack for "+(int)Damage+" damage");
        return true;
    }
    private bool _initialized;
    void IInitializable.Initialize()
    {
        if (!_initialized) Damage = Random.Range(DamageRange.x, DamageRange.y);
    }
    void IInitializable.Initialized() => _initialized = true;
    [Serializable]
    protected class SerializedComponent : IOverride
    {
        [HideInInspector, SerializeField] private float Damage;
        [SerializeField, MMVector("Min", "Max")] private Vector2 DamageRange;
        public SerializedComponent(){}
        public SerializedComponent(RandomizedDamageAttackUseComponent component) => Save(component);
        public void Save(RandomizedDamageAttackUseComponent component)
        {
            Damage = component.Damage;
            DamageRange = component.DamageRange;
        }
        public void Load(RandomizedDamageAttackUseComponent component)
        {
            component.Damage = Damage;
            component.DamageRange = DamageRange;
        }
        IOverridable IOverride.Apply(IOverridable overridable)
        {
            Load((RandomizedDamageAttackUseComponent)overridable);
            return overridable;
        }
    }
    private static readonly SerializedComponent Serialized = new SerializedComponent();
    string IJsonSerializable.Save()
    {
        Serialized.Save(this);
        return JsonUtility.ToJson(Serialized);
    }
    void IJsonSerializable.Load(string json)
    {
        JsonUtility.FromJsonOverwrite(json, Serialized);
        Serialized.Load(this);
    }
    IOverride IOverridable.NewOverride() => new SerializedComponent(this);
}
