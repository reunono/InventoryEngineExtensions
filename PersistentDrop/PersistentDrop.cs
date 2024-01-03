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
    private static string Scene => SceneManager.GetActiveScene().name;
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
    private const int _maxScenes = 30;
    private static readonly Data[] _data = new Data[_maxScenes];
    private static readonly string[] _scenes = new string[_maxScenes];
    private static int _count;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _count = 0;
        for (var i = 0; i < _maxScenes; i++) _data[i] = new Data();
    }
    private void OnEnable() => this.MMEventStartListening();
    private void OnDisable() => this.MMEventStopListening();
    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "Save")
        {
            SaveToMemory();
            for (var i = 0; i < _count; i++) MMSaveLoadManager.Save(_data[i], _scenes[i], _folder);
            _count = 0;
        }
        else if (gameEvent.EventName == "Load")
        {
            var scene = Scene;
            var i = Array.IndexOf(_scenes, scene, 0, _count);
            if (i == -1) ((Data)MMSaveLoadManager.Load(typeof(Data), scene, _folder))?.Spawn();
            else _data[i].Spawn();
        }
        else if (gameEvent.EventName == "SaveToMemory") SaveToMemory();
        void SaveToMemory()
        {
            var scene = Scene;
            var i = Array.IndexOf(_scenes, scene, 0, _count);
            if (i == -1)
            {
                i = _count++;
                _scenes[i] = scene;
            }
            _data[i].Set(FindObjectsOfType<ItemPicker>().Where(picker => picker.name.EndsWith("(Clone)")));
        }
    }
}
