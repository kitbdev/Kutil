using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// first person camera control.
    /// Call Turn with mouse delta to use.
    /// </summary>
    public class FPSCameraControl : MonoBehaviour {

        [System.Serializable]
        public class TurnControls {
            // public Transform turnTransform;
            public float turnSensitivity = 2;
            public float turnMin = -180;
            public float turnMax = 180;
            public bool turnWrap = true;
            [ReadOnly] public float turnValue = 0;
            [ReadOnly] public float turnSpeed = 0;
        }

        [Tooltip("base transform")]
        public Transform horizontalTransform;
        [Tooltip("camera transform")]
        public Transform verticalTransform;
        public float turnSmoothing = 100;

        [Space]
        public TurnControls horizontal = new TurnControls() {
            turnSensitivity = 10,
            turnMin = -180,
            turnMax = 180,
            turnWrap = true,
        };
        public TurnControls vertical = new TurnControls() {
            turnSensitivity = 10,
            turnMin = -80,
            turnMax = 80,
            turnWrap = false,
        };

        protected virtual void Reset() {
            if (transform.childCount > 0) {
                horizontalTransform = transform;
                verticalTransform = transform.GetChild(0);
            } else if (transform.parent != null) {
                horizontalTransform = transform.parent;
                verticalTransform = transform;
            } else {
                horizontalTransform = transform;
                verticalTransform = transform;
            }
        }

        public void Turn(Vector2 inputLook) {
            Turn(inputLook.x, horizontal);
            Turn(-inputLook.y, vertical);
            ApplyRotation();
        }
        protected void Turn(float inputLookVal, TurnControls turn, bool instant = false) {
            if (inputLookVal == 0) return;

            // todo add speed tracking and accel/deccel
            // similar to Cinemachine POV
            // todo add support for external rotations
            turn.turnSpeed = inputLookVal * turn.turnSensitivity * Time.deltaTime;
            // if (Time.deltaTime <= 0) {
            //     turnHorizontalSpeed = 0;
            //     turnVerticalSensitivity = 0;
            // }
            float oldTurnValueValue = turn.turnValue;

            turn.turnValue += turn.turnSpeed;
            // wrap turn
            if (turn.turnWrap) {
                turn.turnValue = Wrap(turn.turnValue, turn.turnMin, turn.turnMax);
                oldTurnValueValue = turn.turnValue - turn.turnSpeed;
            }
            turn.turnValue = Mathf.Clamp(turn.turnValue, turn.turnMin, turn.turnMax);

            if (!instant && turnSmoothing > 0) {
                turn.turnValue = Mathf.Lerp(oldTurnValueValue, turn.turnValue, turnSmoothing * Time.deltaTime);
            }
        }

        protected static float Wrap(float turnValue, float turnRangeMin, float turnRangeMax) {
            float turnRange = turnRangeMax - turnRangeMin;
            if (turnRange <= Mathf.Epsilon) {
                // range is negative or zero!
                return turnValue;
            }
            turnValue = (turnValue - turnRangeMin) % turnRange;
            turnValue += turnRangeMin + (turnValue < 0 ? turnRange : 0);
            return turnValue;
        }

        protected void ApplyRotation() {
            //Quaternion newRot = transform.localRotation;
            if (horizontalTransform != null) {
                Vector3 baseEA = horizontalTransform.localEulerAngles;
                horizontalTransform.localRotation = Quaternion.Euler(baseEA.x, horizontal.turnValue, baseEA.z);
            }
            if (verticalTransform != null) {
                Vector3 camEA = verticalTransform.localEulerAngles;
                verticalTransform.localRotation = Quaternion.Euler(vertical.turnValue, camEA.y, camEA.z);
            }
        }
        public void ClearExternalRotation() {
            if (horizontalTransform != null) horizontalTransform.localRotation = Quaternion.identity;
            if (verticalTransform != null) verticalTransform.localRotation = Quaternion.identity;
            ApplyRotation();
        }

        public void RecenterTurn() {
            vertical.turnValue = 0;
            horizontal.turnValue = 0;
            ApplyRotation();
        }

        public void TransferHorizontalTarget(Transform newYawTransform) {
            // clear yaw from old
            Vector3 yawEA = horizontalTransform.localEulerAngles;
            horizontalTransform.localRotation = Quaternion.Euler(yawEA.x, 0, yawEA.z);

            horizontalTransform = newYawTransform;
            ApplyRotation();
        }

        /// <summary>
        /// apply turn to parent transform, setting the local turn to 0
        /// </summary>
        /// <param name="parentTransform"></param>
        public void ApplyYawToParent(Transform parentTransform) {
            if (horizontal.turnValue == 0) return;// dont need to do anything

            Vector3 parentEA = parentTransform.localEulerAngles;
            parentEA.y += horizontal.turnValue;
            // parentEA.y = Wrap(parentEA.y, turnHorizontalMin, turnHorizontalMax);
            parentTransform.localEulerAngles = parentEA;
            // Debug.Log($"rotating {parentTransform.name} to {horizontal.turnValue}");

            horizontal.turnValue = 0;
            ApplyRotation();
        }
        /// <summary>
        /// parent transform turned, undo that rotation here
        /// </summary>
        /// <param name="parentTransform"></param>
        public void CounteractTurnInParent(float yawChange) {
            Turn(yawChange, horizontal, true);
            ApplyRotation();
        }
    }
}