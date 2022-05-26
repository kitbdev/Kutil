using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kutil {
    /// <summary>
    /// Reacts to Pause and Unpause events from PauseManager while enabled.
    /// Kind of a buffer to the PauseManager, to allow easier controlling of events
    /// </summary>
    public class OnPause : MonoBehaviour {

        public UnityEvent OnPauseEvent;
        public UnityEvent OnUnpauseEvent;
        public UnityEvent<bool> OnPauseToggleEvent;
        public UnityEvent<bool> OnPauseToggleInvertedEvent;

        private void OnEnable() {
            PauseManager pauseManager = PauseManager.Instance;
            if (pauseManager == null) {
                Debug.LogError("No PauseManager found!");
                return;
            }
            pauseManager.pauseEvent.AddListener(OnPausedCallback);
            pauseManager.unpauseEvent.AddListener(OnResumedCallback);
        }
        private void OnDisable() {
            PauseManager.Instance?.pauseEvent.RemoveListener(OnPausedCallback);
            PauseManager.Instance?.unpauseEvent.RemoveListener(OnResumedCallback);
        }

        public void OnPausedCallback() {
            // Debug.Log("paused");
            OnPauseEvent?.Invoke();
            OnPauseToggleEvent?.Invoke(true);
            OnPauseToggleInvertedEvent?.Invoke(!true);
        }
        public void OnResumedCallback() {
            // Debug.Log("resumed");
            OnUnpauseEvent?.Invoke();
            OnPauseToggleEvent?.Invoke(false);
            OnPauseToggleInvertedEvent?.Invoke(!false);
        }

        // passthrough functions to avoid referencing pausemanager in events

        public void TogglePause() {
            PauseManager.Instance.TogglePause();
        }
        public void Pause() {
            SetPaused(true);
        }
        public void UnPause() {
            SetPaused(false);
        }
        public void SetPaused(bool pause = true) {
            PauseManager.Instance.SetPaused(pause);
        }
    }
}