using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentDrop : MonoBehaviour, MMEventListener<MMGameEvent>
{
    private const string _folder = "PersistentDrop";
    private static string File => SceneManager.GetActiveScene().name;
    [Serializable]
    private class Data
    {
        public List<Vector3> Position = new List<Vector3>();
        public List<string> Item = new List<string>();
        public List<int> Quantity = new List<int>();
        public void Set(IEnumerable<ItemPicker> drops)
        {
            Position.Clear();
            Item.Clear();
            Quantity.Clear();
            foreach (var drop in drops)
            {
                Position.Add(drop.transform.position);
                Item.Add(drop.Item.ItemID);
                Quantity.Add(drop.RemainingQuantity);
            }
        }
        public void Spawn()
        {
            for (var i = 0; i < Position.Count; i++)
                Instantiate(Resources.Load<InventoryItem>(Inventory._resourceItemPath+Item[i]).Prefab, Position[i], Quaternion.identity).GetComponent<ItemPicker>().Quantity = Quantity[i];
        }
    }
    private readonly Data _data = new Data();
    private void Save()
    {
        _data.Set(FindObjectsOfType<ItemPicker>().Where(picker => picker.name.EndsWith("(Clone)")));
        MMSaveLoadManager.Save(_data, File, _folder);
    }
    private void Load() => ((Data)MMSaveLoadManager.Load(typeof(Data), File, _folder))?.Spawn();
    private void OnEnable() => this.MMEventStartListening();
    private void OnDisable() => this.MMEventStopListening();
    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "Save") Save();
        else if (gameEvent.EventName == "Load") Load();
    }
}
