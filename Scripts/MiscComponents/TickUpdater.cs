using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Events;

namespace Kutil {
    /// <summary>
    /// Triggers an action after a specified duration or number of frames. millisecond accuracy is not guaranteed.
    /// Duration mode is framerate independent (matches real seconds)
    /// Frame mode is framerate dependent (runs more with higher fps)
    /// </summary>
    public class TickUpdater : MonoBehaviour {

        [System.Serializable]
        public struct TickUpdateArgs {
            public float elapsedDuration;
            public int tick;

            public override string ToString() {
                return $"tick {tick} elapsed:{elapsedDuration}";
            }
        }
        // todo more options like bounds in dur or frames for low and high
        // eg at least 1 every second, otherwise by tick rate

        public bool startOnStart = true;
        // public bool dontDestroyOnLoad = false;
        [Tooltip("Switch timer to trigger every [timerNumFrames] updates instead of every [timerDurS] seconds")]
        /// <summary>Switch timer to trigger every [timerNumFrames] updates instead of every [timerDurS] seconds</summary>
        public bool useFrameCount = false;
        public bool useUnscaledTimeScale = false;
        /// <summary>Apply a custom time scale. default is no custom scale, 1f</summary>
        [ContextMenuItem("Reset", nameof(ResetCustomTimeScale))]
        public float customTimeScale = 1f;
        [ConditionalHide(nameof(useFrameCount), true)]
        [Min(1)]
        public int timerNumFrames = 100;
        [Tooltip("For staggering multiple timers")]
        [ConditionalHide(nameof(useFrameCount), true)]
        public int timerStartFrameOffset = 0;
        /// <summary>timer duration in seconds. only if not using framecount</summary>
        [ConditionalHide(nameof(useFrameCount), false)]
        [Min(0.0001f)]
        public float timerDurSec = 1f;
        [Tooltip("For staggering multiple timers")]
        [ConditionalHide(nameof(useFrameCount), false)]
        public int timerStartDurOffset = 0;
        [ConditionalHide(nameof(useFrameCount), false)]
        [SerializeField]
        //[Min(0.0001f)]
        [ReadOnly]
        private float ticksPerSecond = 1f;

        public int timerDurMS {
            get => SecToMS(timerDurSec);
            set => timerDurSec = MSToSec(value);
        }
        public float ticksPerSec {
            get => 1f / timerDurSec;
            set => timerDurSec = 1f / value;
        }
        public bool isPaused { get; protected set; }
        public int tickCounter { get; protected set; }

        [Space]
        [SerializeField, ReadOnly]
        protected int frameCounter = 0;
        [SerializeField, ReadOnly]
        protected float elapsedDur = 0f;

        // protected float lastTriggerTime = 0f;
        // protected float nextTriggerTime => lastTriggerTime + timerDurSec;

        [Space]
        public UnityEvent onTickUpdateUnityEvent;
        public event Action<TickUpdateArgs> onTickUpdateEvent;
        public event Action onPausedEvent;
        public event Action onResumedEvent;

        // private void Awake() {
        //     if (dontDestroyOnLoad) {
        //         DontDestroyOnLoad(gameObject);
        //     }
        // }
        private void OnValidate() {
            if (ticksPerSec != ticksPerSecond) {
                // ticksPerSec = ticksPerSecond;
                ticksPerSecond = ticksPerSec;
            }
        }
        private void Start() {
            ResetTimer();
            if (startOnStart) {
                isPaused = false;
            } else {
                isPaused = true;
            }
        }

        private void Update() {
            if (isPaused || (!useUnscaledTimeScale && Time.timeScale == 0)) {
                // paused
                return;
            }
            // todo put on other threads?
            frameCounter++;
            float elapsedFrameDur = useUnscaledTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsedDur += elapsedFrameDur * customTimeScale;
            // float sTime = useUnscaledTimeScale ? Time.unscaledTime : Time.time;
            bool trigger = useFrameCount ?
                frameCounter >= timerNumFrames
                : elapsedDur >= timerDurSec;
            // : sTime >= nextTriggerTime;
            if (trigger) {
                tickCounter++;
                TickUpdateArgs args = new TickUpdateArgs() {
                    elapsedDuration = elapsedDur,
                    tick = tickCounter
                };
                onTickUpdateEvent?.Invoke(args);
                onTickUpdateUnityEvent?.Invoke();
                ResetTrigger();
            }
        }
        public void ResetCustomTimeScale(){
            customTimeScale = 1f;
        }

        void ResetTrigger() {
            frameCounter = 0;
            elapsedDur = 0f;
            // lastTriggerTime = useUnscaledTimeScale ? Time.unscaledTime : Time.time;
        }
        public void ResetTimer() {
            ResetTrigger();
            frameCounter = timerStartFrameOffset;
            elapsedDur = timerStartDurOffset;
            tickCounter = 0;
        }

        public void PauseTimer() {
            SetPausedTimer(true);
        }
        public void ResumeTimer() {
            SetPausedTimer(false);
        }
        public void SetPausedTimer(bool pause) {
            if (!isPaused && pause) {
                // pause
                isPaused = true;
                onPausedEvent?.Invoke();
            } else if (isPaused && !pause) {
                // unpause
                isPaused = false;
                onResumedEvent?.Invoke();
            }
        }

        private static int SecToMS(float seconds) {
            return Mathf.FloorToInt(seconds * 1000);
        }
        private static float MSToSec(float milliseconds) {
            return milliseconds * 0.001f;
        }
        public static TickUpdater Create() {
            GameObject go = new GameObject();
            var timer = go.AddComponent<TickUpdater>();
            go.name = "Tick timer";
            return timer;
        }
    }
}