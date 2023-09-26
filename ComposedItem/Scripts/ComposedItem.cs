using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
[CreateAssetMenu]
public class ComposedItem : InventoryItem, IJsonSerializable, IInitializable
{
    [Serializable]
    private class ComponentAuthoring
    {
        [SerializeField] public InventoryItem Component;
        public ComponentAuthoring(){}
        public ComponentAuthoring(ComponentAuthoring componentAuthoring) => Component = componentAuthoring.Component;
        public virtual InventoryItem CreateComponent() => Component.Copy();
        public virtual ComponentAuthoring DeepCopy() => new ComponentAuthoring(this);
    }
    private sealed class ComponentAuthoringWithOverride : ComponentAuthoring
    {
        [SerializeReference] private IOverride Override;
        private IOverride NewOverride() => ((IOverridable)Component).NewOverride();
        public ComponentAuthoringWithOverride(ComponentAuthoring componentAuthoring) : base(componentAuthoring) => Override = NewOverride();
        private ComponentAuthoringWithOverride(ComponentAuthoringWithOverride componentAuthoring) : base(componentAuthoring) => Override = ((IOverridable)componentAuthoring.CreateComponent()).NewOverride();
        public override InventoryItem CreateComponent() => (InventoryItem)((IOverridable)base.CreateComponent()).WithOverride(Override);
        public override ComponentAuthoring DeepCopy() => new ComponentAuthoringWithOverride(this);
    }
    [SerializeReference] private List<ComponentAuthoring> ComponentsAuthoring = new List<ComponentAuthoring>();
    [HideInInspector] public List<InventoryItem> Components;
    public override bool Pick(string playerID)
    {
        foreach (var component in Components) if (!component.Pick(playerID)) return false;
        return true;
    }
    public override bool Use(string playerID)
    {
        foreach (var component in Components) if (!component.Use(playerID)) return false;
        return true;
    }
    public override bool Equip(string playerID)
    {
        foreach (var component in Components) component.Equip(playerID);
        return true;
    }
    public override bool UnEquip(string playerID)
    {
        foreach (var component in Components) component.UnEquip(playerID);
        return true;
    }
    public override void Swap(string playerID)
    {
        foreach (var component in Components) component.Swap(playerID);
    }
    public override bool Drop(string playerID)
    {
        foreach (var component in Components) component.Drop(playerID);
        return true;
    }
    [Serializable]
    private class SerializedComposedItem
    {
        [SerializeField] private string[] Components;
        [SerializeField] private string[] ComponentsSaveData;
        public void Save(ComposedItem item)
        {
            Components = item.Components.Select(component => component.name).ToArray();
            ComponentsSaveData = item.Components.Select(component => (component as IJsonSerializable)?.Save()).ToArray();
        }
        public void Load(ComposedItem item)
        {
            item.Components = Components.Select(component => item.Components.FirstOrDefault(c => c.name == component) == null ? Resources.Load<InventoryItem>(Inventory._resourceItemPath + component).Copy() : item.Components.First(c => c.name == component).Copy()).ToList();
            foreach (var pair in item.Components.Zip(ComponentsSaveData, (component, saveData) => (component, saveData)))
                (pair.component as IJsonSerializable)?.Load(pair.saveData);
        }
    }
    private static readonly SerializedComposedItem SerializedItem = new SerializedComposedItem();
    string IJsonSerializable.Save()
    {
        SerializedItem.Save(this);
        return JsonUtility.ToJson(SerializedItem);
    }
    void IJsonSerializable.Load(string json)
    {
        JsonUtility.FromJsonOverwrite(json, SerializedItem);
        SerializedItem.Load(this);
    }
    public override InventoryItem Copy()
    {
        Components ??= ComponentsAuthoring.Select(componentAuthoring => componentAuthoring.CreateComponent()).ToList();
        var clone = (ComposedItem)base.Copy();
        clone.Components = clone.Components.Select(component => component.Copy()).ToList();
        return clone;
    }
    void IInitializable.Initialize()
    {
        Components ??= ComponentsAuthoring.Select(componentAuthoring => componentAuthoring.CreateComponent()).ToList();
        Components.ForEach(component => (component as IInitializable)?.Initialize());
    }
    public void Initialized() => Components.ForEach(component => (component as IInitializable)?.Initialized());
    public override void SpawnPrefab(string playerID)
    {
        if (!Prefab) return;
        var droppedObject = Instantiate(Prefab);
        var picker = droppedObject.GetComponent<ItemPicker>();
        if (picker)
        {
            Initialized();
            picker.Item = this;
            picker.Quantity = Quantity;
        }
        MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties, TargetInventory(playerID).TargetTransform.position);
    }
    private void OnValidate()
    {
        for (var i = 0; i < ComponentsAuthoring.Count; i++)
        {
            if (ComponentsAuthoring[i] == null) ComponentsAuthoring[i] = new ComponentAuthoring();
            else if (ComponentsAuthoring[i].Component is IOverridable && ComponentsAuthoring[i] is not ComponentAuthoringWithOverride)
                ComponentsAuthoring[i] = new ComponentAuthoringWithOverride(ComponentsAuthoring[i]);
            else if (ComponentsAuthoring[i].Component is not IOverridable && ComponentsAuthoring[i] is ComponentAuthoringWithOverride)
                ComponentsAuthoring[i] = new ComponentAuthoring(ComponentsAuthoring[i]);
            else if (ComponentsAuthoring[i].Component is ComposedItem composed)
            {
                ComponentsAuthoring.AddRange(composed.ComponentsAuthoring.Select(component => component.DeepCopy()));
                ComponentsAuthoring[i] = null;
            }
        }
        ComponentsAuthoring.RemoveAll(c => c == null);
        Components = null;
    }
    private void OnEnable() => Application.quitting += Init;
    private void OnDisable() => Application.quitting -= Init;
    private void Init() => Components = null;
}
