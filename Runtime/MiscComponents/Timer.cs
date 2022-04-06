using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kutil {
    // todo too similar to tick updater?
    public class Timer : MonoBehaviour {

        public bool startOnStart = false;
        /// <summary>
        /// duration in seconds
        /// </summary>
        [Min(0.001f)]
        public float duration = 1;
        public bool autoRestart = false;
        public bool useUnscaledTime = false;

        [SerializeField, ReadOnly] bool isRunning = false;
        [SerializeField, ReadOnly] float timer = 0f;

        [Header("Events")]
        public UnityEvent<float> onTimerUpdate;// for ui and stuff
        public UnityEvent onTimerComplete;

        public bool IsRunning => isRunning;
        public float Progress => isRunning ? Mathf.Clamp01(timer / duration) : 0f;

        private void Start() {
            if (startOnStart) {
                StartTimer();
            }
        }
        private void Update() {
            if (isRunning) {
                timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                onTimerUpdate?.Invoke(Progress);
                if (timer >= duration) {
                    if (autoRestart) {
                        // StartTimer();  
                        timer = 0;
                    } else {
                        StopTimer();
                    }
                    // trigger
                    onTimerComplete?.Invoke();
                }
            }
        }

        public void StartTimer() {
            isRunning = true;
            timer = 0;
            onTimerUpdate?.Invoke(Progress);
        }
        public void StopTimer() {
            isRunning = false;
            timer = 0;
            onTimerUpdate?.Invoke(Progress);
        }
        public void PauseTimer() {
            isRunning = false;
        }
        // doesnt reset timer
        public void ResumeTimer() {
            isRunning = true;
        }
    }
}