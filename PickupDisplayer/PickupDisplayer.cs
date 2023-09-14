using System.Collections;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// A class used to display the picked up items on screen. 
/// The PickupDisplayItems will be parented to it, so it's better if it has a LayoutGroup (Vertical or Horizontal) too.
/// </summary>
public class PickupDisplayer : MonoBehaviour, MMEventListener<MMInventoryEvent>
{
    [Tooltip("the prefab to use to display achievements")]
    public PickupDisplayItem PickupDisplayPrefab;
    [Tooltip("the duration the pickup display item will remain on screen")]
    public float PickupDisplayDuration = 5;
    [Tooltip("the fade in/out duration")]
    public float PickupFadeDuration = .2f;
    private WaitForSeconds _pickupDisplayWfs;
    private readonly Dictionary<string, PickupDisplayItem> _displays = new Dictionary<string, PickupDisplayItem>();
    
    public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
        if (inventoryEvent.InventoryEventType != MMInventoryEventType.Pick) return;
        var item = inventoryEvent.EventItem;
        var quantity = inventoryEvent.Quantity;
        if (_displays.TryGetValue(item.ItemID, out var display)) display.AddQuantity(quantity);
        else
        {
            _displays[item.ItemID] = Instantiate(PickupDisplayPrefab, transform);
            _displays[item.ItemID].Display(item, quantity);
            var canvasGroup = _displays[item.ItemID].GetComponent<CanvasGroup>();
            if (canvasGroup)
            {
                canvasGroup.alpha = 0;
                StartCoroutine(MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 1));
            }
            StartCoroutine(FadeOutAndDestroy());

            IEnumerator FadeOutAndDestroy()
            {
                yield return _pickupDisplayWfs;
                if (canvasGroup) yield return MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 0);
                Destroy(_displays[item.ItemID].gameObject);
                _displays.Remove(item.ItemID);
            }
        }
    }
    private void OnEnable()
    {
        this.MMEventStartListening();
        OnValidate();
    }
    private void OnDisable() => this.MMEventStopListening();
    private void OnValidate() => _pickupDisplayWfs = new WaitForSeconds(PickupDisplayDuration);
}
