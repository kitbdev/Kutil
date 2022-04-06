using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Follows a target position, rotation, and/or scale
    /// </summary>
    public class FollowTransform : MonoBehaviour {
        [SerializeField]
        private Transform _target;
        public Transform target { get => _target; set => SetTarget(value); }

        public bool followPosition = true;
        public bool followRotation = true;
        public bool followScale = false;
        public float smoothPositionRate = 0;
        public float smoothRotationRate = 0;

        [SerializeField] bool targetMainCam = false;
        [SerializeField] bool useExistingPosOffset = false;
        [SerializeField] bool useExistingRotOffset = false;

        public Vector3 positionOffset = Vector3.zero;
        [SerializeField, HideInInspector] Quaternion rotationOffset = Quaternion.identity;
        [SerializeField] Vector3 rotationOffsetEuler = Vector3.zero;
        [SerializeField] Vector3 scaleOffset = Vector3.one;
        [SerializeField] bool useUpdate = true;
        [SerializeField] bool useFixedUpdate = false;
        [SerializeField] bool useLateUpdate = false;

        private void Awake() {
            if (targetMainCam) {
                target = Camera.main.transform;
            } else if (target) {
                UpdateOffsets();
            }
        }
        void UpdateOffsets() {
            if (useExistingPosOffset) {
                positionOffset = transform.position - target.position;
            }
            if (useExistingRotOffset) {
                rotationOffset = Quaternion.Inverse(transform.rotation) * target.rotation;
            } else {
                rotationOffset = Quaternion.Euler(rotationOffsetEuler);
            }
            // scaleOffset = Vector3.Scale(transform.localScale, 1f / target.localScale);
            scaleOffset = Vector3.one;
        }
        /// <summary>
        /// Set the target to follow, or null to stop. 
        /// optionally use the current position and rotation offsets
        /// </summary>
        /// <param name="newTarget">new target to follow</param>
        /// <param name="resetOffset"></param>
        public void SetTarget(Transform newTarget, bool resetOffset = true) {
            _target = newTarget;
            if (target && resetOffset) {
                UpdateOffsets();
            }
        }
        /// <summary>
        /// Set target and easily set follow pos, scale, or rot
        /// </summary>
        /// <param name="newTarget"></param>
        /// <param name="followpos"></param>
        /// <param name="followrot"></param>
        /// <param name="followscale"></param>
        public void SetTarget(Transform newTarget, bool followpos, bool followrot = false, bool followscale = false) {
            SetTarget(newTarget);
            followPosition = followpos;
            followRotation = followrot;
            followScale = followscale;
        }
        public void ZeroOffsets() {
            positionOffset = Vector3.zero;
            rotationOffset = Quaternion.identity;
            scaleOffset = Vector3.one;
        }
        private void Update() {
            if (useUpdate) {
                Follow();
            }
        }
        private void FixedUpdate() {
            if (useFixedUpdate) {
                Follow();
            }
        }
        private void LateUpdate() {
            if (useLateUpdate) {
                Follow();
            }
        }
        protected void Follow() {
            if (!target) return;
            if (followScale) {
                // local scale, so not completely matching
                transform.localScale = Vector3.Scale(target.localScale, scaleOffset);
            }
            if (followRotation) {
                Quaternion nrot = target.rotation * rotationOffset;
                if (smoothRotationRate > 0) {
                    // todo smooth rot
                    nrot = Quaternion.Slerp(transform.rotation, nrot, Time.deltaTime * smoothRotationRate);
                }
                transform.rotation = nrot;
            }
            if (followPosition) {
                Vector3 npos = target.position + positionOffset;
                if (smoothPositionRate > 0) {
                    npos = Vector3.Lerp(transform.position, npos, Time.deltaTime * smoothPositionRate);
                }
                transform.position = npos;
            }
        }
    }
}