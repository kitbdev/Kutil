using UnityEngine;

namespace Kutil {
    /// <summary>
    /// For billboarding
    /// </summary>
    public class FaceCamera : MonoBehaviour {
        Transform cam;

        void Awake() {
            cam = Camera.main.transform;
        }

        void Update() {
            transform.LookAt(cam, Vector3.up);
        }
    }
}