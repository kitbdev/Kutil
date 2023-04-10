using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kutil {
    public class OutOfBoundsCheck : MonoBehaviour {


        public bool checkOnUpdate = true;
        // public bool doOnFixedUpdate = false;

        public bool justVerticalMinCheck = false;



        [BoundsEditorTool(handleColorHtmlString = "#EE2233FF", handleInactiveColorHtmlString = "#EE223344")]
        [ConditionalHide(nameof(justVerticalMinCheck), false)]
        public Bounds outOfBounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        // todo list instead?
        // public List<Bounds> allInBounds = new List<Bounds>();

        [ConditionalHide(nameof(justVerticalMinCheck))]
        public float verticalMinHeight = -10;


        [Collapsable("Event")]
        public UnityEvent onOutOfBoundsEvent;


        void Update() {
            if (checkOnUpdate) {
                DoOutOfBoundsCheck();
            }
        }


        public void DoOutOfBoundsCheck() {
            if (justVerticalMinCheck) {
                if (transform.position.y <= verticalMinHeight) {
                    onOutOfBoundsEvent.Invoke();
                }
                return;
            }
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