using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kutil {
    public class OutOfBoundsCheck : MonoBehaviour {

        [BoundsEditorTool(handleColorHtmlString = "#EE2233FF", handleInactiveColorHtmlString = "#EE223344")]
        [SerializeField]
        public Bounds outOfBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        public bool doOnUpdate = true;
        // public bool doOnFixedUpdate = false;

        [Collapsable("Event")]
        public UnityEvent onOutOfBoundsEvent;


        void Update() {
            if (doOnUpdate) {
                DoOutOfBoundsCheck();
            }
        }


        public void DoOutOfBoundsCheck() {
            if (!outOfBounds.Contains(transform.position)) {
                onOutOfBoundsEvent.Invoke();
            }
        }


        //     protected virtual void OnDrawGizmosSelected() {
        // #if UNITY_EDITOR
        //         if (doOObCheck) {
        //             UnityEditor.Handles.color = Color.red;
        //             outOfBounds.DrawBoundsHandles();
        //         }
        // #endif
        //     }
    }
}