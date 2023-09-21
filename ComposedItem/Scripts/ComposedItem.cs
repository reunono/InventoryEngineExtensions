using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
[CreateAssetMenu]
public class ComposedItem : InventoryItem, IJsonSerializable, IInitializable
{
    public List<InventoryItem> Components;
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
            item.Components = Components.Select(component => Resources.Load<InventoryItem>(Inventory._resourceItemPath + component).Copy()).ToList();
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
        var clone = (ComposedItem)base.Copy();
        clone.Components = clone.Components.Select(component => component.Copy()).ToList();
        return clone;
    }
    void IInitializable.Initialize() => Components.ForEach(component => (component as IInitializable)?.Initialize());
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
}
