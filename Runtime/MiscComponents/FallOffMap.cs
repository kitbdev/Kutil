using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kutil {
    /// <summary>
    /// trigger event when ypos is below threshold
    /// </summary>
    public class FallOffMap : MonoBehaviour {
        public float minHeight = -50f;
        public bool justDestroy = false;
        public UnityEvent fallEvent;

        private void Update() {
            if (transform.position.y <= minHeight) {
                Debug.Log(name + " fell off the map!");
                if (justDestroy) {
                    Destroy(gameObject);
                }
                fallEvent.Invoke();
            }
        }
    }
}