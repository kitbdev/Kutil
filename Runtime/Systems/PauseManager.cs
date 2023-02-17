using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kutil {
    [DisallowMultipleComponent]
    public class PauseManager : Singleton<PauseManager> {
        protected override bool destroyIfMultiple => true;

        [SerializeField, ReadOnly] bool isPaused = false;
        [SerializeField] bool pauseOnStart = false;
        [Min(0)]
        [SerializeField] float transitionTimeLerpDur = 0;
        [Min(0)]
        [SerializeField] float minTimeScale = 0f;
        public bool pauseLock = false;
        public bool disableUnpause = false;
        [Space]
        public bool unpauseOnFocusGain = false;
        [Tooltip("Should we auto pause when focus is lost?")]
        public bool autoPauseWhenFocusLost = false;
        [ConditionalHide(nameof(autoPauseWhenFocusLost), true)]
        [Tooltip("After autopausing, should we unpause on focus is gained? only after an autounpause")]
        public bool autoUnpauseOnFocusAfterAutoPause = false;
        [ConditionalHide(nameof(autoPauseWhenFocusLost), true)]
        [SerializeField, ReadOnly] bool didAutoPause = false;

#if ENABLE_INPUT_SYSTEM
        [Space]
        [SerializeField] InputActionReference togglePauseButton;
#endif
        [Space]
        [SerializeField] bool debug = false;

        IEnumerator pauseLerpCo;

        [Header("Events")]
        public UnityEvent pauseEvent;
        public UnityEvent unpauseEvent;

        public bool IsPaused => isPaused;

        protected void OnEnable() {
#if ENABLE_INPUT_SYSTEM
            if (togglePauseButton) {
                togglePauseButton.action.Enable();
                togglePauseButton.action.performed += c => TogglePause();
            }
#endif
        }
        private void OnDisable() {
#if ENABLE_INPUT_SYSTEM
            if (togglePauseButton) {
                togglePauseButton.action.Dispose();
            }
#endif
        }
        private void Start() {
            if (pauseOnStart) {
                Pause();
            }
        }

        [ContextMenu("Toggle Pause")]
        public void TogglePause() {
            SetPaused(!isPaused);
        }
        public void Pause() {
            SetPaused(true);
        }
        public void UnPause() {
            SetPaused(false);
        }
        public void SetPaused(bool pause = true) {
            if (debug) {
                Debug.Log($"{(pause ? "Pausing" : "Unpausing")} locked:{pauseLock}", this);
            }
            if (pauseLock) {
                Debug.Log("pausing is locked");
                return;
            }
            if (!pause && disableUnpause) {
                Debug.Log("unpausing disallowed");
                return;
            }
            isPaused = pause;
            float targetScale = isPaused ? minTimeScale : 1;
            if (transitionTimeLerpDur > 0) {
                StopCoroutine(pauseLerpCo);
                pauseLerpCo = SetTimeScaleCo(targetScale);
                StartCoroutine(pauseLerpCo);
            } else {
                Time.timeScale = targetScale;
            }
            if (isPaused) {
                pauseEvent.Invoke();
            } else {
                unpauseEvent.Invoke();
            }
        }
        IEnumerator SetTimeScaleCo(float target) {
            float initial = Time.timeScale;
            float progress = 0;
            float interp = initial;
            float scaleSpeed = 1f / transitionTimeLerpDur;
            while (progress < 1) {
                yield return null;
                progress += Time.unscaledDeltaTime * scaleSpeed;
                interp = Mathf.Lerp(initial, target, progress);
                Time.timeScale = interp;
            }
            Time.timeScale = target;
        }
        private void OnApplicationFocus(bool hasFocus) {
            if (hasFocus) {
                if (unpauseOnFocusGain || (didAutoPause && autoUnpauseOnFocusAfterAutoPause)) {
                    UnPause();
                    didAutoPause = false;
                    if (debug) {
                        Debug.Log($"auto unpaused", this);
                    }
                }
            } else {
                if (autoPauseWhenFocusLost) {
                    Pause();
                    if (debug) {
                        Debug.Log($"auto paused", this);
                    }
                    didAutoPause = true;
                }
            }
        }
        void OnApplicationPause(bool pauseStatus) {
            OnApplicationFocus(!pauseStatus);
        }
    }
}