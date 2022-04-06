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

        enum ShowAction {
            NONE, SHOW, HIDE
        }

        [SerializeField] Selectable selectOnShow;
        [SerializeField] private MenuScreenGroup _menuScreenGroup;
        [SerializeField] bool recenterOnAwake = true;
        [SerializeField] bool showOnTop = true;
        [SerializeField] ShowAction showOnStart = ShowAction.NONE;
        [SerializeField] float fadeDuration = 0f;
        [SerializeField] bool fadeUnscaled = true;

        [ReadOnly] public bool isShown = false;

        [Header("Events")]
        // for stuff like animation?
        public UnityEvent OnShownEvent;
        public UnityEvent OnHiddenEvent;

        CanvasGroup canvasGroup;

        public MenuScreenGroup menuScreenGroup {
            get => _menuScreenGroup;
            set {
                // in case changed before enable
                if (menuScreenGroup != null && menuScreenGroup.allMenuScreens.Contains(this)) {
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

        [ContextMenu("Recenter pos")]
        public void RecenterPosition() {
            var rt = transform as RectTransform;
            rt.localPosition = Vector3.zero;
        }
        [ContextMenu("Show")]
        void ShowEditor() {
            canvasGroup = GetComponent<CanvasGroup>();
            SetShown(true, false, false);
        }
        [ContextMenu("Hide")]
        void HideEditor() {
            canvasGroup = GetComponent<CanvasGroup>();
            SetShown(false, false, false);
        }
        public void Show() {
            SetShown(true);
        }
        public void Hide() {
            SetShown(false);
        }
        public void SetShown(bool shown) {
            SetShown(shown, true, true);
        }

        Coroutine coroutine;
        public void SetShown(bool shown, bool invokeEvents, bool allowFade = true) {
            bool wasShown = isShown;
            isShown = shown;
            if (allowFade && fadeDuration > 0f && wasShown != shown) {
                if (coroutine != null) {
                    StopCoroutine(coroutine);
                }
                coroutine = StartCoroutine(Fade(shown, wasShown, invokeEvents));
                return;
            }
            SetDirect(shown);
            AfterSet(wasShown, invokeEvents);
        }

        private void SetDirect(bool shown) {
            canvasGroup.alpha = shown ? 1f : 0f;
            canvasGroup.blocksRaycasts = shown;
            canvasGroup.interactable = shown;
        }

        private void AfterSet(bool wasShown, bool invokeEvents) {
            if (isShown) {
                selectOnShow?.Select();
                if (showOnTop) {
                    // make sure we are on top of others
                    transform.SetAsLastSibling();
                }
                // only invoke events if state changed
                if (!wasShown) {
                    OnShownEvent?.Invoke();
                }
            } else {
                if (wasShown) {
                    OnHiddenEvent?.Invoke();
                }
            }
            if (isShown && invokeEvents) {
                if (menuScreenGroup != null) {
                    menuScreenGroup.NotifyMenuScreenOn(this);
                }
            }
        }

        IEnumerator Fade(bool shown, bool wasShown, bool invokeEvents) {
            float timer = 0;
            float progress = 0;
            canvasGroup.alpha = wasShown ? 1f : 0f;
            if (isShown) {
                // ? before
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
            AfterSet(wasShown, invokeEvents);
            coroutine = null;
        }
    }
}