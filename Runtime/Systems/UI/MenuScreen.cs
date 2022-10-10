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
        protected enum FadeEasing {
            Linear,
            InOutSine,
            // InSine is probably the best, given how the alpha seems kinda exp 
            InSine,
            OutSine,
        }
        // protected enum TransitionMode {
        //     Fade, Slide,
        //     Dynamic//?
        // }
        // protected enum SlideMode {
        //     // LEFT,RIGHT,
        //     // UP,DOWN,
        //     // CUBERIGHT,CUBELEFT,
        //     Horizontal, Vertical,
        //     // InFront,//?
        //     // HorizontalCube
        // }
        // public struct MenuScreenNavigationSlide {
        //     public enum Mode {
        //         Vertical, Horizontal, Both, Automatic
        //     }
        //     public MenuScreen up;
        //     public MenuScreen down;
        //     public MenuScreen left;
        //     public MenuScreen right;
        // }

        [Tooltip("Should the MenuScreen Show or Hide on start")]
        [SerializeField] protected ShowAction showOnStart = ShowAction.NONE;
        [Tooltip("Should the MenuScreen be recentered? resets local position (for easier editing)")]
        [SerializeField] protected bool recenterOnAwake = true;
        [Tooltip("Should the CanvasGroup be interactable and block raycasts")]
        // ? split with block raycasts?
        // ? max alpha value?
        [SerializeField] protected bool interactable = true;

        [Space]
        [Tooltip("The Selectable to select when shown, like a button")]
        [SerializeField] protected Selectable selectOnShow;
        // [Tooltip("Should we prevent other selectables from being selected while shown?")]
        // [SerializeField] protected bool lockSelectionToChildren = false;
        [Tooltip("Show on top of other MenuScreen when shown? (moves to last sibling)")]
        [SerializeField] protected bool showOnTop = true;

        [Header("Fade")]
        [Tooltip("Use Fade In - linear using alpha")]
        [SerializeField] protected bool useFadeIn = true;
        [Tooltip("Use Fade Out - linear using alpha")]
        [SerializeField] protected bool useFadeOut = true;
        bool inspectorShowFadeOptions => useFadeIn || useFadeOut;

        [ConditionalHide(nameof(inspectorShowFadeOptions), true)]
        [SerializeField] protected FadeEasing fadeEasing = FadeEasing.Linear;
        [ConditionalHide(nameof(inspectorShowFadeOptions), true)]
        [Tooltip("Duration (seconds) to fade")]
        // [UnityEngine.Serialization.FormerlySerializedAs("fadeDuration")]
        [SerializeField] protected float fadeDuration = 0.5f;
        [ConditionalHide(nameof(inspectorShowFadeOptions), true)]
        [Tooltip("Should fading use unscalded time")]
        [SerializeField] protected bool fadeUnscaled = true;

        // todo other transition options
        // [Header("Sliding")]
        // [SerializeField] protected bool useSliding;
        // // [ConditionalHide(nameof(useSliding), true)]
        // // [Tooltip("Duration (seconds) to slide")]
        // // [SerializeField] protected float fadeDuration = 0.5f;
        // [ConditionalHide(nameof(useSliding), true)]
        // [SerializeField] protected FadeEasing slideEasing = FadeEasing.Linear;
        // // todo somehow dynamic
        // [ConditionalHide(nameof(useSliding), true)]
        // [SerializeField] protected SlideMode slideIn;
        // [ConditionalHide(nameof(useSliding), true)]
        // [SerializeField] protected SlideMode slideOut;

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
        [SerializeField, ReadOnly] private bool _isShown = false;
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
            if (!interactable) {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (recenterOnAwake) {
                RecenterPosition();
            }
            if (autoFindMenuScreenGroup && menuScreenGroup == null) {
                FindMenuScreenGroup();
            }
        }
        private void Start() {
            if (showOnStart == ShowAction.SHOW) {
                // set manually to force change
                isShown = false;
                // show without fading, but with events
                SetShown(true, true, false, true);
            } else if (showOnStart == ShowAction.HIDE) {
                isShown = true;
                SetShown(false, true, false, true);
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
            // Debug.Log("found menu group " + menuScreenGroup.name);
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
#if UNITY_EDITOR
            // in editor
            canvasGroup = GetComponent<CanvasGroup>();
            if (!Application.isPlaying) {
                UnityEditor.Undo.RecordObject(this, "Show MenuScreen");
                UnityEditor.Undo.RecordObject(canvasGroup, "Show MenuScreen");
                SetShown(true, false, false, false);
            } else {
                // Debug.Log("show fade");
                SetShown(true);
            }
#endif
        }
        [ContextMenu("Hide")]
        void HideEditorMode() {
#if UNITY_EDITOR
            canvasGroup = GetComponent<CanvasGroup>();
            if (!Application.isPlaying) {
                UnityEditor.Undo.RecordObject(this, "Show MenuScreen");
                UnityEditor.Undo.RecordObject(canvasGroup, "Show MenuScreen");
                SetShown(false, false, false, false);
            } else {
                SetShown(false);
            }
#endif
        }

        public void Show() {
            SetShown(true);
        }
        public void Hide() {
            SetShown(false);
        }
        /// <summary>
        /// Show immediately, without fading in
        /// </summary>
        public void ShowFast() {
            SetShown(true, true, false, true);
        }
        /// <summary>
        /// Hide immediately, without fading out
        /// </summary>
        public void HideFast() {
            SetShown(false, true, false, true);
        }
        public void ToggleShown() {
            SetShown(!isShown);
        }
        /// <summary>
        /// Show or Hide the MenuScreen
        /// </summary>
        /// <param name="shown">Show or Hide</param>
        public void SetShown(bool shown) {
            SetShown(shown, true, true, true);
        }
        /// <summary>
        /// Show or Hide the MenuScreen.
        /// with extra options
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
            if (interactable || !shown) {
                canvasGroup.blocksRaycasts = shown;
                canvasGroup.interactable = shown;
            }
        }

        protected void AfterSet(bool wasShown, bool notifyGroup, bool sendEvents = true) {
            if (isShown) {
                if (selectOnShow != null) selectOnShow.Select();
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
            if (isShown && interactable) {
                // ? do this before
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            while (progress < 1) {
                yield return null;
                timer += fadeUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                progress = Mathf.InverseLerp(0, fadeDuration, timer);
                // todo more easings?
                float val = progress;
                if (fadeEasing == FadeEasing.InOutSine) {
                    val = easeInOutSine(progress);
                } else if (fadeEasing == FadeEasing.InSine) {
                    val = easeInSine(progress);
                } else if (fadeEasing == FadeEasing.OutSine) {
                    val = easeOutSine(progress);
                }
                if (shown) {
                    canvasGroup.alpha = val;
                } else {
                    canvasGroup.alpha = 1f - val;
                }
            }
            SetDirect(shown);
            AfterSet(wasShown, invokeEvents, sendEvents);
            fadeCoroutine = null;
        }
        // https://easings.net/
        float easeInOutSine(float x) {
            return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
        }
        float easeInSine(float x) {
            return 1 - Mathf.Cos((x * Mathf.PI) / 2);
        }
        float easeOutSine(float x) {
            return Mathf.Sin((x * Mathf.PI) / 2);
        }
    }
}