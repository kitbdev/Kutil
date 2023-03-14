using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kutil {
    /// <summary>
    /// slows down time for debugging purposes
    /// </summary>
    public class SlowMoDebug : MonoBehaviour {

        // [PostField]
        [AddButton(nameof(ResetTimeScale))]
        [AddButton(nameof(SlowMo))]
        public float slowSpeed = 0.2f;
        public bool toggleOnLKey = true;

        void Update() {
            if (toggleOnLKey) {
#if ENABLE_INPUT_SYSTEM                
                if (Keyboard.current.lKey.wasPressedThisFrame) {
                    ToggleSlowmo();
                }
#endif
            }
        }
        [ContextMenu("Toggle")]
        public void ToggleSlowmo() {
            if (Time.timeScale == 1f) {
                SlowMo();
            } else {
                ResetTimeScale();
            }
        }
        [ContextMenu("Reset")]
        public void ResetTimeScale() {
            Time.timeScale = 1f;
        }
        [ContextMenu("Slow")]
        public void SlowMo() {
            Time.timeScale = slowSpeed;
        }
    }
}