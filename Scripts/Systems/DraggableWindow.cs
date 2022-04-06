using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.EventSystems;
using System.Linq;

namespace Kutil {
    public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

        [SerializeField] List<DraggableWindowBar> dragWindowBars = new List<DraggableWindowBar>();
        [SerializeField] bool autoFindDragBars = true;
        public bool canMove = true;
        public bool canScale = true;
        [SerializeField] Vector2 minSize = Vector2.one * 0.2f;
        [SerializeField] Vector2 maxSize = Vector2.one;
        [SerializeField] float resizeBorderSize = 0.01f;
        [SerializeField] float resizeBorderEdgeOffset = 0.01f;

        protected Canvas canvas;
        protected Camera cam;
        [SerializeField, ReadOnly] bool isMouseHovering = false;
        [SerializeField, ReadOnly] bool isScaling = false;
        [SerializeField, ReadOnly] bool isMoving = false;
        [SerializeField, ReadOnly] ScaleMode curScaling = new ScaleMode();
        [SerializeField, ReadOnly] Rect originalScaleRectOffset;
        [SerializeField, ReadOnly] Rect originalScaleRect;
        [System.Serializable]
        struct ScaleMode {
            public bool scaleMinX;
            public bool scaleMinY;
            public bool scaleMaxX;
            public bool scaleMaxY;
        }

        RectTransform rectTransform => (RectTransform)transform;

        private void Awake() {
            canvas = GetComponentInParent<Canvas>();
            cam = Camera.main;
            originalScaleRectOffset = new Rect(rectTransform.offsetMin, rectTransform.offsetMax);
            originalScaleRect = rectTransform.rect;
            if (autoFindDragBars) {
                FindDragBars();
            }
            foreach (var bar in dragWindowBars) {
                bar.Init(this);
            }
        }
        void FindDragBars() {
            DraggableWindowBar[] bars = gameObject.GetComponentsInChildren<DraggableWindowBar>();
            dragWindowBars.AddRange(bars.Except(dragWindowBars));
        }
        Vector2 GetRectScreenPos() {
            // returns the bottom left corner of the rect
            // !note parent must fill entire screen 
            return rectTransform.anchoredPosition + rectTransform.anchorMin * ((RectTransform)rectTransform.parent).rect.size;
        }

        void HandleOnBeginDrag(PointerEventData eventData) {
            // if (CanScale()) {
            //     isScaling = true;
            // } else {
            if (!isScaling) {
                isMoving = true;
                // todo get pos
            }
        }
        void HandleOnEndDrag(PointerEventData eventData) {
            isMoving = false;
            isScaling = false;
        }
        void HandleOnDrag(PointerEventData eventData) {
            if (isScaling) {
                HandleScale(eventData);
            } else if (isMoving) {
                HandleMoveWindow(eventData);
            }
        }
        void HandleMoveWindow(PointerEventData eventData) {
            if (!canMove) return;
            Vector2 screenpos = eventData.position / canvas.scaleFactor;
            Vector2 viewpos = cam.ScreenToViewportPoint(screenpos);

            Vector2 rectspos = GetRectScreenPos();
            // Vector2 rectsize = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            // Vector2 rectspos = new Rect((Vector2)rectTransform.position - (rectsize * rectTransform.pivot), rectsize).position;
            // Vector2 rectRel = rectTransform.rect.center - (Vector2)rectTransform.InverseTransformPoint(screenpos);
            // Debug.Log($"moving! sp:{screenpos} vpos:{viewpos} rt:{((RectTransform)rectTransform.parent).rect.size} rpos:{rectspos} r:{rectRel}");
            // todo keep drag mdown pos and move window to there instead of mouse delta
            Vector2 rectminspos = rectspos;
            Vector2 rectmaxvpos = cam.ScreenToViewportPoint(rectspos + rectTransform.rect.size);
            // check bounds of screen
            if (rectminspos.x < 0 && eventData.delta.x < 0) {
                return;
            }
            if (rectminspos.y < 0 && eventData.delta.y < 0) {
                return;
            }
            if (rectmaxvpos.x > 1 && eventData.delta.x > 0) {
                return;
            }
            if (rectmaxvpos.y > 1 && eventData.delta.y > 0) {
                return;
            }
            // move window
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        [ContextMenu("Reset Scale")]
        void ResetScale() {
            rectTransform.offsetMin = originalScaleRectOffset.min;
            rectTransform.offsetMax = originalScaleRectOffset.max;
        }
        void HandleScale(PointerEventData eventData) {
            // if (!CanScale(out bool scaleMinX, out bool scaleMinY, out bool scaleMaxX, out bool scaleMaxY)) return false;
            Vector2 screendelta = eventData.delta / canvas.scaleFactor;
            // Debug.Log("Scaling by " + screendelta);
            Vector2 scaledOffsetMin = rectTransform.offsetMin;
            Vector2 scaledOffsetMax = rectTransform.offsetMax;

            if (curScaling.scaleMinX) {
                scaledOffsetMin.x += screendelta.x;
            }
            if (curScaling.scaleMinY) {
                scaledOffsetMin.y += screendelta.y;
            }
            if (curScaling.scaleMaxX) {
                scaledOffsetMax.x += screendelta.x;
            }
            if (curScaling.scaleMaxY) {
                scaledOffsetMax.y += screendelta.y;
            }
            // min and max
            // todo use non offset rect to compare
            // Rect drect = new Rect(scaledOffsetMin - originalScaleRectOffset.min, scaledOffsetMax - originalScaleRectOffset.max);
            // scaledOffsetMin.x = Mathf.Clamp(scaledOffsetMin.x, originalScaleRectOffset.xMin * minSize.x, originalScaleRectOffset.xMin * maxSize.x);
            // Debug.Log($"clamping {scaledOffsetMin.y} {originalScaleRectOffset.yMin * minSize.y} {originalScaleRectOffset.yMin * maxSize.y}");
            // scaledOffsetMin.y = Mathf.Clamp(scaledOffsetMin.y, originalScaleRectOffset.yMin * minSize.y, originalScaleRectOffset.yMin * maxSize.y);
            // scaledOffsetMax.x = Mathf.Clamp(scaledOffsetMax.x, originalScaleRectOffset.xMax * maxSize.x, originalScaleRectOffset.xMax * maxSize.x);
            // scaledOffsetMax.y = Mathf.Clamp(scaledOffsetMax.y, originalScaleRectOffset.yMax * maxSize.y, originalScaleRectOffset.yMax * maxSize.y);
            //todo
            // scaledOffsetMin.x = Mathf.Max(scaledOffsetMin.x, minSize.x * originalScaleRect.xMin);
            // scaledOffsetMin.y = Mathf.Max(scaledOffsetMin.y, minSize.y * originalScaleRect.yMin);
            // scaledOffsetMax.x = Mathf.Min(scaledOffsetMax.x, maxSize.x * originalScaleRect.xMax);
            // scaledOffsetMax.y = Mathf.Min(scaledOffsetMax.y, maxSize.y * originalScaleRect.yMax);
            // todo also max and min w/ screen size
            rectTransform.offsetMin = scaledOffsetMin;
            rectTransform.offsetMax = scaledOffsetMax;
        }
        bool CanScale() {//out bool scaleMinX, out bool scaleMinY, out bool scaleMaxX, out bool scaleMaxY) {
            if (!canScale) {
                curScaling.scaleMinX = curScaling.scaleMinY = curScaling.scaleMaxX = curScaling.scaleMaxY = false;
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            Vector2 mscreenpos = Mouse.current.position.ReadValue() / canvas.scaleFactor;
#else
            Vector2 mscreenpos = Input.mousePosition / canvas.scaleFactor;
#endif
            Vector2 rectspos = GetRectScreenPos();
            Vector2 rectRel = (mscreenpos - rectspos) / rectTransform.rect.size;
            // float scaleEdgeThickness = 0.1f;
            // Vector2 minp = scaleEdgeThickness * Vector2.one;
            // Vector2 maxp = (1f - scaleEdgeThickness) * Vector2.one;
            // Vector2 vp = cam.ScreenToViewportPoint(rectsRel);
            // Vector2 rectmaxvpos = cam.ScreenToViewportPoint(rectspos + rectTransform.rect.size);
            // Debug.Log($"Scale p{rectRel}");
            // bool isNear(float pos, float npos, float dist) {
            //     return pos > npos - dist && pos < npos + dist;
            // }
            // scaleMinX = isNear(minp.x, rectRel.x, resizeBorderSize);
            // scaleMinY = isNear(minp.y, rectRel.y, resizeBorderSize);
            // scaleMaxX = isNear(maxp.x, rectRel.x, resizeBorderSize);
            // scaleMaxY = isNear(maxp.y, rectRel.y, resizeBorderSize);
            curScaling.scaleMinX = rectRel.x > 0 + resizeBorderEdgeOffset && rectRel.x <= resizeBorderEdgeOffset + resizeBorderSize;
            curScaling.scaleMinY = rectRel.y > 0 + resizeBorderEdgeOffset && rectRel.y <= resizeBorderEdgeOffset + resizeBorderSize;
            curScaling.scaleMaxX = rectRel.x < 1 - resizeBorderEdgeOffset && rectRel.x >= 1 - resizeBorderEdgeOffset - resizeBorderSize;
            curScaling.scaleMaxY = rectRel.y < 1 - resizeBorderEdgeOffset && rectRel.y >= 1 - resizeBorderEdgeOffset - resizeBorderSize;
            // if (!(scaleMinX || scaleMinY || scaleMaxX || scaleMaxY))
            //     Debug.Log("Scale none");
            return curScaling.scaleMinX || curScaling.scaleMinY || curScaling.scaleMaxX || curScaling.scaleMaxY;
        }
        void CheckScale() {
            if (!CanScale()) return;
            if (curScaling.scaleMinX) {
                // Debug.Log("Scale minx");
                // Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
            if (curScaling.scaleMinY) {
                // Debug.Log("Scale miny");
                // rectTransform.anchorMin += new Vector2(0, amount.y);
            }
            if (curScaling.scaleMaxX) {
                // Debug.Log("Scale maxx");
                // rectTransform.anchorMax += new Vector2(amount.x, 0);
            }
            if (curScaling.scaleMaxY) {
                // Debug.Log("Scale maxy");
                // rectTransform.anchorMax += new Vector2(0, amount.y);
            }
        }

        // these methods only foward functionalaity
        public void OnDrag(PointerEventData eventData) {
            if (dragWindowBars.Count > 0) return;
            HandleOnDrag(eventData);
        }
        public void OnBeginDrag(PointerEventData eventData) {
            if (dragWindowBars.Count > 0) return;
            HandleOnBeginDrag(eventData);
        }
        public void OnEndDrag(PointerEventData eventData) {
            if (dragWindowBars.Count > 0) return;
            HandleOnEndDrag(eventData);
        }
        public void OnBarDrag(PointerEventData eventData) {
            HandleOnDrag(eventData);
        }
        public void OnBarBeginDrag(PointerEventData eventData) {
            HandleOnBeginDrag(eventData);
        }
        public void OnBarEndDrag(PointerEventData eventData) {
            HandleOnEndDrag(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            // CheckScale(eventData);
            isMouseHovering = true;
        }
        public void OnPointerExit(PointerEventData eventData) {
            // CheckScale(eventData);
            isMouseHovering = false;
        }
        private void Update() {
            if (isMouseHovering) {
                OnPoinerHover();
            }
        }
        void OnPoinerHover() {
            if (!isScaling) {
                CheckScale();
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            transform.SetAsLastSibling();
            if (!isScaling) {
                // CheckScale();
            }
            if (CanScale()) {
                isScaling = true;
            }
            // Debug.Log("p:" + cam.ScreenToViewportPoint(eventData.position));
        }
    }
}