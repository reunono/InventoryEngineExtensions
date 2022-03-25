using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Combine
{
    public class CombineInventoryDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private GraphicRaycaster _raycaster;
        private Canvas _canvas;
        private List<RaycastResult> _raycastResults;
        private InventorySlot _slot;
        private InventoryItem _item;
        private Inventory _inventory;
        private string _playerID;
        public Combine Combine;
        
        private void Awake()
        {
            _raycaster = GetComponent<GraphicRaycaster>();
            _canvas = GetComponent<Canvas>();
        }

        private void Raycast(PointerEventData eventData)
        {
            _raycastResults = new List<RaycastResult>();
            _raycaster.Raycast(eventData, _raycastResults);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _slot = null;
            Raycast(eventData);
            foreach (var result in _raycastResults)
            {
                _slot = result.gameObject.GetComponent<InventorySlot>();
                if (_slot == null) continue;
                _inventory = _slot.ParentInventoryDisplay.TargetInventory;
                _playerID = _inventory.PlayerID;
                _item = _inventory.Content[_slot.Index];
                if (InventoryItem.IsNull(_item)) _slot = null;
                return;
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_slot == null) return;
            var screenPoint = Input.mousePosition;
            _slot.IconImage.transform.SetParent(transform, false);
            if (_canvas.worldCamera != null)
            {
                screenPoint.z = _canvas.planeDistance;
                _slot.IconImage.transform.position = _canvas.worldCamera.ScreenToWorldPoint(screenPoint);
            }
            else
                _slot.IconImage.transform.position = screenPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_slot == null) return;
            _slot.IconImage.transform.SetParent(_slot.transform, false);
            _slot.IconImage.transform.localPosition = Vector3.zero;
            Raycast(eventData);
            foreach (var result in _raycastResults)
            {
                var destinationSlot = result.gameObject.GetComponent<InventorySlot>();
                if (destinationSlot == null) continue;
                if (_slot == destinationSlot && _item.Quantity == 1) return;
                var destinationInventory = destinationSlot.ParentInventoryDisplay.TargetInventory;
                var destinationItem = destinationInventory.Content[destinationSlot.Index];
                var isDestinationEmpty = InventoryItem.IsNull(destinationItem);
                if (!isDestinationEmpty &&
                    Combine.TryCombineItems(_inventory, _slot.Index, destinationInventory, destinationSlot.Index))
                    return;
                if (_inventory == destinationInventory && (_item.CanMoveObject && isDestinationEmpty || _item.CanSwapObject && !isDestinationEmpty && destinationItem.CanSwapObject))
                    _inventory.MoveItem(_slot.Index, destinationSlot.Index);
                else if (_item.IsEquippable && _item.TargetEquipmentInventoryName == destinationInventory.name)
                {
                    if (isDestinationEmpty)
                    {
                        _item.Equip(_playerID);
                        _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                        MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, destinationSlot, destinationInventory.name, _item, 0, destinationSlot.Index, _playerID);
                    }
                    else
                    {
                        destinationItem.UnEquip(_playerID);
                        _item.Equip(_playerID);
                        var tempItem = destinationItem.Copy();
                        destinationInventory.Content[destinationSlot.Index] = _item.Copy();
                        _inventory.Content[_slot.Index] = tempItem;
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                    }
                }
                else if (_slot.Unequippable() && _item.TargetInventoryName == destinationInventory.name)
                {
                    if (isDestinationEmpty)
                    {
                        _item.UnEquip(_playerID);
                        _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                    }
                    else if (destinationItem.IsEquippable && destinationItem.TargetEquipmentInventoryName == _inventory.name)
                    {
                        _item.UnEquip(_playerID);
                        destinationItem.Equip(_playerID);
                        var tempItem = _item.Copy();
                        _inventory.Content[_slot.Index] = destinationItem.Copy();
                        destinationInventory.Content[destinationSlot.Index] = tempItem;
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                    }
                }

                return;
            }
            _slot.Drop();
        }
    }
}
