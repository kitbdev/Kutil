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

        // [PostFieldDecorator]
        [AddButton(nameof(ResetTimeScale))]
        [AddButton(nameof(SlowMo))]
        public float slowSpeed = 0.2f;
        public bool toggleOnKey = true;
#if ENABLE_INPUT_SYSTEM
        [ConditionalHide(nameof(toggleOnKey))]
        public Key toggleKey = Key.L;
#endif

        void Update() {
            if (toggleOnKey) {
#if ENABLE_INPUT_SYSTEM                
                if (Keyboard.current[toggleKey].wasPressedThisFrame) {
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