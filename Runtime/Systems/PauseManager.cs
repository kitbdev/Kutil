using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kutil {
    /// <summary>
    /// Manages Pausing.
    /// Must be only one PauseManager.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public class PauseManager : Singleton<PauseManager> {
        protected override bool destroyIfMultiple => true;

        [SerializeField, ReadOnly] bool isPaused = false;
        [SerializeField] bool pauseOnStart = false;
        [Tooltip("if greater than zero slowly transition to paused or unpasused, lerping timescale")]
        [SerializeField][Min(0)] float transitionTimeLerpDur = 0;
        [Tooltip("Set to 0 to freeze time when paused. Set to 1 to not use timescale. set between 0 and 1 to have slow motion instead of freezing time.")]
        [SerializeField][Min(0)] float minTimeScale = 0f;

        [Tooltip("Should AudioListener be paused? AudioSources can be set to ignore this")]
        [SerializeField] bool pauseAudio = false;

        // variables set for one frame for consuming pauses
        bool consumePause;
        bool pausedSetThisFrame;

        /// <summary>block pausing and unpausing</summary>
        [SerializeField, HideInInspector] List<GameObject> pauseLocks = new();
        public bool isPauseLocked => pauseLocks.Count > 0;
        /// <summary>block unpausing only</summary>
        public bool disableUnpause = false;// todo necessary?

        [Header("Focus")]
        [Tooltip("Should we unpause when we get focus?")]
        public bool unpauseOnFocusGain = false;
        [Tooltip("Should we auto pause when focus is lost?")]
        public bool autoPauseWhenFocusLost = false;
        [ConditionalHide(nameof(autoPauseWhenFocusLost), true)]
        [Tooltip("After autopausing on focus lost, should we unpause on focus is gained? only after an autounpause")]
        public bool autoUnpauseOnFocusAfterAutoPause = false;
        [ConditionalHide(nameof(autoPauseWhenFocusLost), true)]
        [SerializeField, ReadOnly] bool didAutoPause = false;

        [Header("Input")]
        /// <summary>listen and respond to input. default is escape key or start button. can be overriden</summary>
        [Tooltip("listen and respond to input. default is escape key or start button. can be overriden")]
        [SerializeField] bool handleInput = true;
#if ENABLE_INPUT_SYSTEM
        InputAction defaultPauseAction;
        // not */{menu}
        // cannot be changed at runtime
        [Tooltip("Input Action Reference to override the default pause action (<Keyboard>/escape,<Gamepad>/start)")]
        [ConditionalHide(nameof(handleInput), true)]
        [SerializeField] InputActionReference overrideTogglePauseAction;
        InputActionReference usingIAR => overrideTogglePauseAction ?? InputActionReference.Create(defaultPauseAction);
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
            if (handleInput) {
                if (overrideTogglePauseAction != null) {
                    overrideTogglePauseAction.action.Enable();
                    overrideTogglePauseAction.action.performed += TogglePauseInput;
                } else {
                    defaultPauseAction = new InputAction("DefaultPauseAction", InputActionType.Button);
                    // defaultPauseAction.AddBinding("*/{menu}");
                    defaultPauseAction.AddBinding("<Keyboard>/escape");
                    defaultPauseAction.AddBinding("<Gamepad>/start");
                    defaultPauseAction.Enable();
                    defaultPauseAction.performed += TogglePauseInput;
                }
            }
#endif
        }
        private void OnDisable() {
#if ENABLE_INPUT_SYSTEM
            if (handleInput) {
                if (overrideTogglePauseAction != null) {
                    overrideTogglePauseAction.action.performed -= TogglePauseInput;
                    overrideTogglePauseAction.action.Disable();
                } else {
                    defaultPauseAction.performed -= TogglePauseInput;
                    defaultPauseAction.Disable();
                }
            }
#endif
        }
#if ENABLE_INPUT_SYSTEM
        void TogglePauseInput(InputAction.CallbackContext cc) {
            if (debug) Debug.Log("pause input");
            TogglePause();
        }
#endif
#if !ENABLE_INPUT_SYSTEM
        // private void Update() {
        //     if (Input.GetKeyDown("Escape")){
        //         TogglePause();
        //     }
        // }
#endif


        private void Start() {
            if (pauseOnStart) {
                if (debug) Debug.Log("Pausing on start");
                Pause();
            }
        }

        /// <summary>
        /// Ignores a pause or unpause for a frame. Use when a pause action has triggered to avoid pausing
        /// </summary>
        public void ConsumePauseInput() {
            // ignores pause for a frame
            if (debug) Debug.Log($"consuming pause {PauseToStr(isPaused) + "d"}. just set:{pausedSetThisFrame}", this);
            if (consumePause) return;
            if (pausedSetThisFrame) {
                // if pause already occured (which it shouldnt due to high def exec order)
                // better to not pause at all?
                TogglePause();
            }
            // if pause hasnt already occured
            consumePause = true;
            Invoke(nameof(ResetConsumePauseInput), 0f);
        }
        // gets invoked on the next frame
        void ResetConsumePauseInput() {
            consumePause = false;
            if (debug) Debug.Log($"reset consuming pause", this);
        }

        /// <summary>
        /// Lock pausing and unpausing until unlocked. Disallows pausing or unpausing.
        /// Useful for menus or sub gameplay states
        /// </summary>
        /// <param name="locker">gameobject to register locking to, usually *this.gameObject*. 
        /// null can be also used</param>
        public void LockPause(GameObject locker) {
            // todo? do they need to be registerd by GO? why not just have a count? pass null ig
            if (locker != null && pauseLocks.Contains(locker)) {
                Debug.LogWarning($"Pause Locks already contains locker {locker.name}!", locker);
                // return; //? add anyway
            }
            if (debug) Debug.Log($"Adding pause locker {locker?.name ?? "null"}", locker);
            pauseLocks.Add(locker);
        }
        /// <summary>
        /// Unlock pausing and unpausing until unlocked. Resallows pausing or unpausing, if not lockedby others.
        /// </summary>
        /// <param name="locker">gameobject that was registered locking, usually *this.gameObject*. 
        /// null can be also used</param>
        public void UnlockPause(GameObject locker) {
            if (!pauseLocks.Contains(locker)) {
                Debug.LogError($"Cannot unlock pause with locker {locker?.name ?? "null"} {pauseLocks.ToStringFull(go => go.name, true)}", locker);
                return;
            }
            if (debug) Debug.Log($"Removing pause locker {locker?.name ?? "null"}", locker);
            pauseLocks.Remove(locker);
        }
        public void ClearAllPauseLocks() {
            pauseLocks.Clear();
        }


        [ContextMenu("Toggle Pause")]
        public void TogglePause() {
            SetPaused(!isPaused);
        }
        /// <summary>
        /// Pause the game
        /// </summary>
        [ContextMenu("Pause")]
        public void Pause() {
            SetPaused(true);
        }
        /// <summary>
        /// Unpause the game
        /// </summary>
        [ContextMenu("Unpause")]
        public void UnPause() {
            SetPaused(false);
        }
        public void SetPaused(bool pause = true) {
            if (!Application.isPlaying) {
                Debug.LogWarning($"Cannot {PauseToStr(pause)} when paused");
                return;
            }
            if (consumePause) {
                if (debug) Debug.Log($"{PauseToStr(pause)} was consumed", this);
                return;
            }
            if (isPauseLocked) {
                if (debug) Debug.Log($"cannot {PauseToStr(pause)}, pausing is locked. locks {pauseLocks.ToStringFull(null, true)}", this);
                return;
            }
            if (!pause && disableUnpause) {
                if (debug) Debug.Log("unpausing disallowed", this);
                return;
            }

            if (debug) Debug.Log($"{PauseToStr(pause, true)}", this);
            isPaused = pause;
            pausedSetThisFrame = true;
            Invoke(nameof(ResetSetPauseThisFrame), 0f);

            // set timescale
            float targetScale = isPaused ? minTimeScale : 1;
            if (transitionTimeLerpDur > 0) {
                StopCoroutine(pauseLerpCo);
                pauseLerpCo = SetTimeScaleCo(targetScale);
                StartCoroutine(pauseLerpCo);
            } else {
                Time.timeScale = targetScale;
            }

            if (pauseAudio) {
                // note can be ignored if AudioSource.ignoreListenerPause=true;
                AudioListener.pause = pause;
            }

            // call events
            if (isPaused) {
                pauseEvent.Invoke();
            } else {
                unpauseEvent.Invoke();
            }
        }
        // gets invoked on the next frame after pause set is called
        void ResetSetPauseThisFrame() {
            pausedSetThisFrame = false;
        }

        private static string PauseToStr(bool pause, bool ing = false) {
            return (pause ? "Paus" : "Unpaus") + (ing ? "ing" : "e");
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

        // called by unity when focus changes
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
                    didAutoPause = true;
                    if (debug) {
                        Debug.Log($"auto paused", this);
                    }
                }
            }
        }
        // called by unity when focus changes for mobile
        void OnApplicationPause(bool pauseStatus) {
            OnApplicationFocus(!pauseStatus);
        }
    }
}