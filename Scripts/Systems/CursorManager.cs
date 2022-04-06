using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kutil {
    /// <summary>
    /// Manages the cursor lock and visible state. must be placed in scene
    /// </summary>
    public class CursorManager : MonoBehaviour {
#if ENABLE_INPUT_SYSTEM
        [SerializeField] InputActionReference unlockAction;
        [SerializeField] InputActionReference lockAction;
#endif
        [SerializeField] CursorLockMode lockMode = CursorLockMode.Locked;
        [SerializeField] bool hideWhenLocked = true;
        [SerializeField] bool lockOnAwake = true;
        [SerializeField] bool toggleOnPause = true;
        [SerializeField] bool toggleOnMKey = false;
        [ReadOnly] public bool isLocked = false;

        private void Awake() {
            if (lockOnAwake) {
                SetCursorLock(true);
            }
        }
        private void OnEnable() {
#if ENABLE_INPUT_SYSTEM
            if (unlockAction != null) {
                unlockAction.action.Enable();
                unlockAction.action.performed += c => SetCursorLock(false);
            }
            if (lockAction != null) {
                lockAction.action.Enable();
                lockAction.action.performed += c => SetCursorLock(true);
            }
#endif
            if (toggleOnPause) {
                // todo only if using keyboard and mouse
                PauseManager.Instance.pauseEvent.AddListener(UnlockCursor);
                PauseManager.Instance.unpauseEvent.AddListener(LockCursor);
            }
            // todo automatically detect focus?
        }
        private void OnDisable() {
            if (toggleOnPause) {
                PauseManager.Instance?.pauseEvent.RemoveListener(UnlockCursor);
                PauseManager.Instance?.unpauseEvent.RemoveListener(LockCursor);
            }
        }
        private void Update() {
#if ENABLE_INPUT_SYSTEM
            if (toggleOnMKey && (Keyboard.current?.mKey.wasPressedThisFrame ?? false)) {
#else
            if (toggleOnMKey && Input.GetKeyDown(KeyCode.M)) {
#endif
                SetCursorLock(!isLocked);
            }
        }
        public void SetCursorLock(bool locked) {
            isLocked = locked;
            if (locked) {
                Cursor.lockState = lockMode;
                Cursor.visible = !hideWhenLocked;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        public void UnlockCursor() {
            SetCursorLock(false);
        }
        public void LockCursor() {
            SetCursorLock(true);
        }
    }
}