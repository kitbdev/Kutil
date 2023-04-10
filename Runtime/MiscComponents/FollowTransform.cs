using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Follows a target position, rotation, and/or scale
    /// </summary>
    public class FollowTransform : MonoBehaviour {

        [SerializeField] bool targetMainCam = false;
        [ConditionalHide(nameof(targetMainCam), false, readonlyInstead = true)]
        [SerializeField]
        private Transform _target;
        public Transform target { get => _target; set => SetTarget(value); }

        [Space]
        public bool followPosition = true;
        public bool followRotation = true;
        public bool followScale = false;

        [Space]
        [Min(0f)] public float smoothPositionRate = 0;
        [Min(0f)] public float smoothRotationRate = 0;

        [Space]
        [SerializeField] bool useExistingPosOffset = false;
        [SerializeField] bool useExistingRotOffset = false;

        [ConditionalHide(nameof(useExistingPosOffset), false, readonlyInstead = true)]
        [ContextMenuItem("Reset", nameof(ZeroPosition))]
        public Vector3 positionOffset = Vector3.zero;

        [ConditionalHide(nameof(useExistingRotOffset), false, readonlyInstead = true)]
        [ContextMenuItem("Reset", nameof(ZeroRotation))]
        [SerializeField] Vector3 rotationOffsetEuler = Vector3.zero;
        [SerializeField, HideInInspector] Quaternion rotationOffset = Quaternion.identity;
        [ContextMenuItem("Reset", nameof(ZeroScale))]
        [SerializeField] Vector3 scaleOffset = Vector3.one;

        [Space]
        [SerializeField] bool useUpdate = true;
        [SerializeField] bool useFixedUpdate = false;
        [SerializeField] bool useLateUpdate = false;



        void OnValidate() {
            ValidateFields();
        }
        void Awake() {
            ValidateFields();
        }

        void ValidateFields() {
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
                rotationOffsetEuler = transform.rotation.eulerAngles;
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
        [ContextMenu("Reset offsets")]
        public void ZeroOffsets() {
            ZeroPosition();
            ZeroRotation();
            ZeroScale();
        }

        private void ZeroScale() {
            scaleOffset = Vector3.one;
        }

        // seperated for context menu
        private void ZeroPosition() {
            positionOffset = Vector3.zero;
        }
        private void ZeroRotation() {
            rotationOffset = Quaternion.identity;
        }


        //? any way to not have a callback if not needed
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