using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kutil {
    [SelectionBase]
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuScreen : MonoBehaviour {

        protected enum ShowAction {
            NONE, SHOW, HIDE
        }

        [Tooltip("Should the MenuScreen Show or Hide on start?")]
        [SerializeField] protected ShowAction showOnStart = ShowAction.NONE;
        [Tooltip("Should the MenuScreen be recentered? resets local position (for easier editing)")]
        [SerializeField] protected bool recenterOnAwake = true;

        [Space]
        [Tooltip("The Selectable to select when shown, like a button")]
        [SerializeField] protected Selectable selectOnShow;
        [Tooltip("Show on top of other MenuScreen when shown? (moves to last sibling)")]
        [SerializeField] protected bool showOnTop = true;

        [Header("Fade")]
        [Tooltip("Use Fade In - linear using alpha")]
        [SerializeField] protected bool useFadeIn = true;
        [Tooltip("Use Fade Out - linear using alpha")]
        [SerializeField] protected bool useFadeOut = true;
        bool inspectorShowFadeOptions => useFadeIn || useFadeOut;

        [ConditionalHide(nameof(inspectorShowFadeOptions), true)]
        [Tooltip("Duration (seconds) to fade")]
        [SerializeField] protected float fadeDuration = 0.1f;
        [ConditionalHide(nameof(inspectorShowFadeOptions), true)]
        [Tooltip("Should fading use unscalded time")]
        [SerializeField] protected bool fadeUnscaled = true;

        [Space]
        [Tooltip("The MenuScreenGroup to use")]
        [ContextMenuItem("Find in parents", nameof(FindMenuScreenGroup))]
        [ContextMenuItem("Remove", nameof(ClearMenuScreenGroup))]
        [SerializeField] protected MenuScreenGroup _menuScreenGroup = null;

        [Tooltip("Find first MenuScreenGroup in parents to register with")]
        bool inspectorShowAutoFindMenuScreenGroup => _menuScreenGroup == null;
        [ConditionalHide(nameof(inspectorShowAutoFindMenuScreenGroup), true)]
        [SerializeField] protected bool autoFindMenuScreenGroup = false;


        [Header("Info")]
        [ReadOnly] private bool _isShown = false;
        public bool isShown { get => _isShown; protected set => _isShown = value; }

        [Header("Events")]
        // for stuff like animation?
        public UnityEvent OnShownEvent;
        public UnityEvent OnHiddenEvent;

        protected CanvasGroup canvasGroup;
        protected Coroutine fadeCoroutine;

        public MenuScreenGroup menuScreenGroup {
            get => _menuScreenGroup;
            set {
                // stay registered
                if (menuScreenGroup != null && menuScreenGroup.allMenuScreens.Contains(this)) {
                    // in case changed before enable
                    menuScreenGroup.UnRegisterMenuScreen(this);
                }
                _menuScreenGroup = value;
                if (menuScreenGroup != null) {
                    menuScreenGroup.RegisterMenuScreen(this);
                }
            }
        }

        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            if (recenterOnAwake) {
                RecenterPosition();
            }
            if (autoFindMenuScreenGroup && _menuScreenGroup != null) {
                FindMenuScreenGroup();
            }
        }
        private void Start() {
            if (showOnStart == ShowAction.SHOW) {
                // changed so will invoke event
                isShown = false;
                SetShown(true, true, false);
            } else if (showOnStart == ShowAction.HIDE) {
                isShown = true;
                SetShown(false, true, false);
            }
        }
        private void OnEnable() {
            if (menuScreenGroup != null && !menuScreenGroup.allMenuScreens.Contains(this)) {
                menuScreenGroup.RegisterMenuScreen(this);
            }
        }
        private void OnDisable() {
            if (menuScreenGroup != null) {
                menuScreenGroup.UnRegisterMenuScreen(this);
            }
        }
        [ContextMenu("Find MenuScreenGroup")]
        public void FindMenuScreenGroup() {
#if UNITY_EDITOR
            // todo check if prefabs are cool with this
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(this, "Find MenuScreenGroup");
#endif
            menuScreenGroup = gameObject.GetComponentInParent<MenuScreenGroup>();
        }
        private void ClearMenuScreenGroup() {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(this, "Reset MenuScreenGroup");
#endif
            _menuScreenGroup = null;
        }

        [ContextMenu("Recenter pos")]
        public void RecenterPosition() {
            // Debug.Log("Recentering!");
            var rt = transform as RectTransform;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(rt, "Recenter");
#endif
            rt.localPosition = Vector3.zero;
        }

        [ContextMenu("Show")]
        void ShowEditorMode() {
            // in editor
            canvasGroup = GetComponent<CanvasGroup>();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(this, "Show MenuScreen");
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(canvasGroup, "Show MenuScreen");
#endif
            SetShown(true, false, false, false);
        }
        [ContextMenu("Hide")]
        void HideEditorMode() {
            canvasGroup = GetComponent<CanvasGroup>();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(this, "Show MenuScreen");
            if (!Application.isPlaying) UnityEditor.Undo.RecordObject(canvasGroup, "Show MenuScreen");
#endif
            SetShown(false, false, false, false);
        }

        public void Show() {
            SetShown(true);
        }
        public void Hide() {
            SetShown(false);
        }
        public void ToggleShown() {
            SetShown(!isShown);
        }
        public void SetShown(bool shown) {
            SetShown(shown, true, true, true);
        }
        /// <summary>
        /// Show or Hide the MenuScreen
        /// </summary>
        /// <param name="shown">Show or Hide</param>
        /// <param name="notifyGroup">should notify our group if we changed? (single screen at a time and history)</param>
        /// <param name="allowFade">can we fade in or out</param>
        /// <param name="sendEvents">should we invoke on shown or on hidden events</param>
        public void SetShown(bool shown, bool notifyGroup, bool allowFade = true, bool sendEvents = true) {
            bool wasShown = isShown;
            isShown = shown;
            if (allowFade && fadeDuration > 0f && wasShown != shown) {
                if ((useFadeIn && !wasShown && isShown) || (useFadeOut && wasShown && !isShown)) {
                    if (fadeCoroutine != null) {
                        StopCoroutine(fadeCoroutine);
                    }
                    fadeCoroutine = StartCoroutine(Fade(shown, wasShown, notifyGroup, sendEvents));
                    return;
                }
            }
            SetDirect(shown);
            AfterSet(wasShown, notifyGroup, sendEvents);
        }

        protected void SetDirect(bool shown) {
            canvasGroup.alpha = shown ? 1f : 0f;
            canvasGroup.blocksRaycasts = shown;
            canvasGroup.interactable = shown;
        }

        protected void AfterSet(bool wasShown, bool notifyGroup, bool sendEvents = true) {
            if (isShown) {
                selectOnShow?.Select();
                if (showOnTop) {
                    // dont move in edit mode
                    if (Application.isPlaying) {
                        // make sure we are on top of others
                        transform.SetAsLastSibling();
                    }
                }
                // only invoke events if state changed
                if (!wasShown && sendEvents) {
                    OnShownEvent?.Invoke();
                }
            } else {
                if (wasShown && sendEvents) {
                    OnHiddenEvent?.Invoke();
                }
            }
            if (notifyGroup) {
                NotifyGroup();
            }
        }
        protected void NotifyGroup() {
            if (menuScreenGroup != null) {
                menuScreenGroup.NotifyMenuScreenState(isShown, this);
            }
        }

        protected IEnumerator Fade(bool shown, bool wasShown, bool invokeEvents, bool sendEvents) {
            float timer = 0;
            float progress = 0;
            canvasGroup.alpha = wasShown ? 1f : 0f;
            if (isShown) {
                // ? do before or after
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            while (progress < 1) {
                yield return null;
                timer += fadeUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                progress = Mathf.InverseLerp(0, fadeDuration, timer);
                if (shown) {
                    canvasGroup.alpha = progress;
                } else {
                    canvasGroup.alpha = 1f - progress;
                }
            }
            SetDirect(shown);
            AfterSet(wasShown, invokeEvents, sendEvents);
            fadeCoroutine = null;
        }
    }
}