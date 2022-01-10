using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventoryDragAndDropExtension
{
    public class InventoryDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private GraphicRaycaster _raycaster;
        private Canvas _canvas;
        private List<RaycastResult> _raycastResults;
        private InventorySlot _slot;
        
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
                _slot.Move();
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
                destinationSlot.Move();
                return;
            }
            _slot.Drop();
        }
    }
}
