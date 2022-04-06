using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kutil {
    /// <summary>
    /// additional drag area for window
    /// </summary>
    public class DraggableWindowBar : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

        DraggableWindow draggableWindow;

        public void Init(DraggableWindow draggableWindow) {
            this.draggableWindow = draggableWindow;
        }

        public void OnDrag(PointerEventData eventData) {
            draggableWindow?.OnBarDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            draggableWindow?.OnBarBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData) {
            draggableWindow?.OnBarEndDrag(eventData);
        }

    }
}