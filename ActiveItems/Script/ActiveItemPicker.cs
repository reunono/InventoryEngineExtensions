using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this class on items you want to set as "active" items (as in "The Binding of Isaac" for example).
/// </summary>
public class ActiveItemPicker : ItemPicker
{       

    public override void Pick(string targetInventoryName, string playerID = "Player1")
    {
        InventorySlot slot = GameObject.Find("ActiveItemInventoryDisplay").transform.GetChild(0).GetChild(0).GetComponent<InventorySlot>();

        if (_targetInventory == null)
        {
            return;
        }

        if (!Pickable())
        {
            PickFail();
            return;
        }

        DetermineMaxQuantity();
        if (!Application.isPlaying)
        {
            if (!Item.ForceSlotIndex)
            {
                _targetInventory.AddItem(Item, 1);
            }
            else
            {
                _targetInventory.AddItemAt(Item, 1, Item.TargetIndex);
            }
        }
        else
        {
            // Here we detect if the one-slot active inventory is already full or not...
            if (_targetInventory.IsFull)
            {
                // If full => we drop the slot's actual content to replace it with the picked item
                MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, "ActiveItemInventory", _targetInventory.Content[0], 0, 0, "Player1");
                MMInventoryEvent.Trigger(MMInventoryEventType.Pick, slot, "ActiveItemInventory", Item, 0, 0, "Player1");
            }
            else
            {
                // If empty => we simply add the picked item.
                MMInventoryEvent.Trigger(MMInventoryEventType.Pick, slot, Item.TargetInventoryName, Item, _pickedQuantity, 0, playerID);
            }
        }
        if (Item.Pick(playerID))
        {
            RemainingQuantity = RemainingQuantity - _pickedQuantity;
            PickSuccess();
            DisableObjectIfNeeded();
        }
    }

}
