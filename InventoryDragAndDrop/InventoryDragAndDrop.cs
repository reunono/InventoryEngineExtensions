using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventoryDragAndDrop
{
    public class InventoryDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GraphicRaycaster _raycaster;
    private Canvas _canvas;
    private List<RaycastResult> _raycastResults;
    private InventorySlot _slot;
    private InventoryItem _item;
    private Inventory _inventory;
    private string _playerID;
    private InventorySlot _currentlyHoveredSlot;

    [SerializeField] private bool dragOutsideDropsItem;
    
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
        foreach (RaycastResult result in _raycastResults)
        {
            _slot = result.gameObject.GetComponent<InventorySlot>();
            if (_slot == null) continue;

            if (!_slot.SlotEnabled)
            {
                continue;
            }
            
            if (InventoryItem.IsNull(_slot.CurrentItem))
            {
                _slot = null;
                continue;
            }
            
            if (!_slot.CurrentItem.CanMoveObject)
            {
                _slot = null;
                continue;
            }
            
            _inventory = _slot.ParentInventoryDisplay.TargetInventory;
            _playerID = _inventory.PlayerID;
            _item = _inventory.Content[_slot.Index];
            return;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_slot == null) return;
        Vector3 screenPoint = Input.mousePosition;
        _slot.IconImage.transform.SetParent(transform, false);
        if (_canvas.worldCamera != null)
        {
            screenPoint.z = _canvas.planeDistance;
            _slot.IconImage.transform.position = _canvas.worldCamera.ScreenToWorldPoint(screenPoint);
        }
        else
        {
            _slot.IconImage.transform.position = screenPoint;
        }
        
        //Highlighting slots the item is getting dragged over
        Raycast(eventData);
        InventorySlot newHoveredSlot = null;
        
        foreach (RaycastResult result in _raycastResults)
        {
            newHoveredSlot = result.gameObject.GetComponent<InventorySlot>();
            if (newHoveredSlot != null) break; 
        }
        
        if (newHoveredSlot != _currentlyHoveredSlot)
        {
            //remove highlight from old slot
            if (_currentlyHoveredSlot != null)
            {
                _currentlyHoveredSlot.OnPointerExit(eventData);
            }

            //highlight new slot
            if (newHoveredSlot != null)
            {
                newHoveredSlot.OnPointerEnter(eventData);
            }

            _currentlyHoveredSlot = newHoveredSlot;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentlyHoveredSlot)
        {
            _currentlyHoveredSlot.OnPointerExit(eventData);
            _currentlyHoveredSlot = null;
        }
        
        if (_slot == null) return;
        _slot.IconImage.transform.SetParent(_slot.transform, false);
        _slot.IconImage.transform.localPosition = Vector3.zero;
        
        Raycast(eventData);
        
        //Validation (especially for move) to work like original Move method using buttons
        foreach (RaycastResult result in _raycastResults)
        {
            InventorySlot destinationSlot = result.gameObject.GetComponent<InventorySlot>();
            if (destinationSlot == null) continue;

            InventoryDisplay destinationDisplay = destinationSlot.ParentInventoryDisplay;
            Inventory destinationInventory = destinationDisplay.TargetInventory;
            
            if (!destinationSlot.SlotEnabled) return;
            
            InventoryItem destinationItem = destinationInventory.Content[destinationSlot.Index];
            bool isDestinationEmpty = InventoryItem.IsNull(destinationItem);

            bool moveSuccessful = false;

            //Moving within the same inventory
            if (_inventory == destinationInventory)
            {
                if ((_item.CanMoveObject && isDestinationEmpty) ||
                    (_item.CanSwapObject && !isDestinationEmpty && destinationItem.CanSwapObject))
                {
                    moveSuccessful = _inventory.MoveItem(_slot.Index, destinationSlot.Index);
                }
            }
            //Moving from Inventory to Equipment Inventory (equip item)
            else if (_item.IsEquippable && _item.TargetEquipmentInventoryName == destinationInventory.name)
            {
                if (isDestinationEmpty)
                {
                    _item.Equip(_playerID);
                    moveSuccessful = _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                    if (moveSuccessful)
                    {
                        MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, destinationSlot, destinationInventory.name, _item, 0, destinationSlot.Index, _playerID);
                    }
                }
                else if (destinationItem.CanSwapObject)
                {
                    destinationItem.UnEquip(_playerID);
                    _item.Equip(_playerID);
            
                    InventoryItem tempItem = destinationItem.Copy();
                    destinationInventory.Content[destinationSlot.Index] = _item.Copy();
                    _inventory.Content[_slot.Index] = tempItem;
            
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                    moveSuccessful = true;
                }
            }
            //Moving from Equipment Inventory to main Inventory (unequip item)
            else if (_slot.Unequippable() && _item.TargetInventoryName == destinationInventory.name)
            {
                if (isDestinationEmpty)
                {
                    _item.UnEquip(_playerID);
                    moveSuccessful = _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                }
                else if (destinationItem.IsEquippable && destinationItem.TargetEquipmentInventoryName == _inventory.name)
                {
                    _item.UnEquip(_playerID);
                    destinationItem.Equip(_playerID);
            
                    InventoryItem tempItem = _item.Copy();
                    _inventory.Content[_slot.Index] = destinationItem.Copy();
                    destinationInventory.Content[destinationSlot.Index] = tempItem;
            
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                    moveSuccessful = true;
                }
            }
            //Inventory switch (for example Main Inventory to chest - Main Inventory to Main Inventory)
            else if (_inventory != destinationInventory)
            {
                if (destinationDisplay.AllowMovingObjectsToThisInventory)
                {
                    if (isDestinationEmpty)
                    {
                        moveSuccessful = _inventory.MoveItemToInventory(_slot.Index, destinationInventory, destinationSlot.Index);
                    }
                    else if (_item.CanSwapObject && destinationItem.CanSwapObject)
                    {
                        InventoryItem tempItem = destinationItem.Copy();
                        destinationInventory.Content[destinationSlot.Index] = _item.Copy();
                        _inventory.Content[_slot.Index] = tempItem;
                        moveSuccessful = true;
                    }
                    
                    if (moveSuccessful)
                    {
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, _inventory.name, null, 0, 0, _playerID);
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, destinationInventory.name, null, 0, 0, _playerID);
                    }
                }
            }

            if (moveSuccessful)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Move, destinationSlot, destinationInventory.name, destinationInventory.Content[destinationSlot.Index], 0, destinationSlot.Index, _playerID);
                destinationSlot.Select();
            }
            else
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, destinationSlot, destinationInventory.name, null, 0, destinationSlot.Index, _playerID);
            }
            
            return;
        }

        if (dragOutsideDropsItem)
        {
            _slot.Drop();
        }
    }
}
}
