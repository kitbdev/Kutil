using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    /// <summary>
    /// Generic trigger for many use cases
    /// </summary>
    [AddComponentMenu("_Util/GenericTrigger")]
    public class GenericTrigger : MonoBehaviour {

        [ContextMenuItem("Clear tag", "ClearTag")]
        public string checkTag = "";
        public LayerMask checkLayerMask = Physics.DefaultRaycastLayers;
        public float repeatDur = -1;
        protected float lastTriggerRepTime = 0;
        protected float lastTriggerEnterTime = 0;
        public bool onlyOnce = false;
        // public LayerMask validLayers = Physics.DefaultRaycastLayers;

        [ReadOnly] public int numInTrigger = 0;
        [ReadOnly] public GameObject latestEnterGO = null;
        // [ReadOnly] public List<GameObject> GOsinTrigger = new List<GameObject>();
        /// <summary>true when any are in the trigger. from first in to last out</summary>
        public bool inTrigger => numInTrigger > 0;
        public float durInTrigger => inTrigger ? Time.time - lastTriggerEnterTime : -1;

        [Header("Events")]
        public UnityEvent triggerEnteredEvent;
        public UnityEvent triggerStayEvent;
        public UnityEvent triggerExitEvent;
        // public UnityEvent firstEnterEvent;
        // public UnityEvent lastExitEvent;

        [ContextMenu("Clear tag")]
        void ClearTag() {
#if UNITY_EDITOR
    if (!Application.isPlaying)
        Undo.RecordObject(this, "change value");
#endif
            checkTag = "";
        }
        private void Awake() {
            numInTrigger = 0;
        }

        private void OnTriggerEnter(Collider other) {
            if (IsValidTrig(other)) {
                Debug.Log($"Trigger enter {name} o:{other}");
                triggerEnteredEvent.Invoke();
                numInTrigger++;
                lastTriggerEnterTime = Time.time;
                latestEnterGO = other.gameObject;
                if (onlyOnce) {
                    gameObject.SetActive(false);
                }
            }
        }
        private void OnTriggerStay(Collider other) {
            if (IsValidTrig(other)) {
                if (repeatDur > 0) {
                    if (Time.time <= lastTriggerRepTime + repeatDur) {
                        return;
                    }
                }
                lastTriggerRepTime = Time.time;
                triggerStayEvent.Invoke();
            }
        }
        private void OnTriggerExit(Collider other) {
            if (IsValidTrig(other)) {
                triggerExitEvent.Invoke();
                numInTrigger--;
            }
        }
        protected bool IsValidTrig(Collider other) {
            bool isValid = true;
            if (!((Layer)other.gameObject.layer).InLayerMask(checkLayerMask)) {
                isValid = false;
            }
            if (checkTag.Length > 0) {
                if (!other.CompareTag(checkTag)) {
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}