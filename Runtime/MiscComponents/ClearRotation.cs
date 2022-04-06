using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class ClearRotation : MonoBehaviour {
        public bool always = true;

        private void Start() {
            ClearRot();
        }

        private void LateUpdate() {
            if (always) {
                ClearRot();
            }
        }

        [ContextMenu("Clear rotation")]
        public void ClearRot() {
            transform.rotation = Quaternion.identity;
        }
    }
}