using System.Collections;
using System.Collections.Generic;
using Kutil;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    /// <summary>
    /// Top down 2d free camera move system.
    /// to use: attach to the camera and assign the camFollowTarget.
    /// </summary>
    [DefaultExecutionOrder(4)]
    public class CameraControls : MonoBehaviour {


        // [ConditionalHide(nameof(disableDragMovement), true)]
        // [AddNote("Attach to camera", style: AddNoteAttribute.NoteStyle.HELPINFO, layout:AddNoteAttribute.NoteLayout.REPLACE)]
        // [SerializeField] private bool dummy;
        // these can be set by other scripts to disable camera movement temporarily
        [SerializeField] public bool disableMovement = false;
        [SerializeField] public bool disableDragMovement = false;
        [SerializeField] public bool disableInputMovement = false;
        [SerializeField] public bool disableEdgeMovement = false;
        [SerializeField] public bool disableZoom = false;

        [Header("Move Drag")]
        [SerializeField] bool clickDragToMove = true;
        [SerializeField][Min(0f)] float moveDragSmoothing = 50;
        [SerializeField] bool moveDragWhenMouseOverUI = true;

        [Header("Move Input")]
        [SerializeField] float moveInputSpeed = 10;
        [SerializeField] float moveInputSpeedByZoom = 10;
        [SerializeField][Min(0f)] float moveInputSmoothing = 10;
        [SerializeField] bool moveInputWhenMouseOverUI = true;

        [Header("Move Edge")]
        [SerializeField] bool hoverAtEdgeToMove = true;
        [SerializeField] float moveEdgeSpeed = 10;
        [SerializeField] float moveEdgeDist = 0.05f;
        [SerializeField][Min(0f)] float moveEdgeSmoothing = 10;
        [SerializeField] bool ignoreMouseOutOfViewport = true;
        [SerializeField] bool doesEdgeMoveNeedReenter = true;
        [SerializeField] bool moveEdgeWhenMouseOverUI = true;

        [Header("Zoom")]
        [SerializeField] float zoomContRate = 0.01f;
        [SerializeField][Min(0f)] float zoomSmoothing = 0;
        [SerializeField] float zoomBtnRate = 0.1f;
        [SerializeField][Min(0f)] float zoomToCursorRate = 1f;
        // public enum ZoomMethod{
        //     CamZ, Orthographic, Perspective
        // }
        // [SerializeField] ZoomMethod zoomMethod = ZoomMethod.CamZ;
        [Tooltip("Use camera height rather than FOV")]
        [SerializeField] bool zoomUseCamZ = true;
        [ConditionalHide(nameof(zoomUseCamZ), true)]
        [SerializeField] float zoomMinHeight = -20;
        [ConditionalHide(nameof(zoomUseCamZ), true)]
        [SerializeField] float zoomMaxHeight = -5;
        [ConditionalHide(nameof(zoomUseCamZ), true)]
        [SerializeField] float zoomDefHeight = -10;
        // todo seperate perspective?
        [ConditionalHide(nameof(zoomUseCamZ), false)]
        [SerializeField] float zoomMinValue = 4;
        [ConditionalHide(nameof(zoomUseCamZ), false)]
        [SerializeField] float zoomMaxValue = 20;
        [ConditionalHide(nameof(zoomUseCamZ), false)]
        [SerializeField] float zoomDefValue = 6;

        [SerializeField] bool zoomContWhenMouseOverUI = true;
        [SerializeField] bool zoomBtnWhenMouseOverUI = true;

        [Header("Follow Target")]
        // if dont want to follow target, set to this transform
        // [SerializeField] bool shouldFollowTarget = true;
        [SerializeField] Transform camFollowTarget;
        [SerializeField][Min(0f)] float finalSmoothing = 0;
        [SerializeField][Min(0f)] float followSmoothing = 100;
        [SerializeField] Camera targetCam;
        [SerializeField] bool updateFollowOnFixedUpdate = false;

        [Header("Bounds")]
        public bool boundaryActive = true;
        public bool boundaryLocalToTarget = true;
        [SerializeField] bool drawBounds = true;
        // [SerializeField] Rect mapLocalBounds;
        // todo arbitrary polygon boundary - like cinemachine confiner
        public Vector2 boundsCenterOffset = Vector2.zero;
        public bool boundAsCircle = true;
        [ConditionalHide(nameof(boundAsCircle), true)]
        public float boundsMaxRadius = 10;
        [ConditionalHide(nameof(boundAsCircle), false)]
        public Vector2 boundsSize = Vector2.one * 10;

        [Header("Misc")]
        [SerializeField][Min(0f)] float recenterDuration = 0.2f;
        [SerializeField] bool recenterOnStart = true;
        [SerializeField] bool recenterActionIncludeZoom = true;
        [SerializeField] bool useUnscaledTime = false;

        [Header("Input")]
#if ENABLE_INPUT_SYSTEM
        [Tooltip("Button input - mouse button")]
        [SerializeField] InputActionReference moveDragAction;
        [Tooltip("Vector2 input - keyboard")]
        [SerializeField] InputActionReference moveAction;
        [Tooltip("Axis input - scrollwheel")]
        [SerializeField] InputActionReference zoomContAction;
        [Tooltip("Axis input - keys")]
        [SerializeField] InputActionReference zoomBtnAction;
        [Tooltip("Button input")]
        [SerializeField] InputActionReference recenterAction;
#endif
        [Space]
        [SerializeField, ReadOnly] Vector2 moveInput = Vector2.zero;
        [SerializeField, ReadOnly] float zoomInput = 0f;
        [SerializeField, ReadOnly] bool dragStartInput;
        [SerializeField, ReadOnly] bool dragEndInput;

        [Header("Info")]
        [SerializeField, ReadOnly] Vector3 camTargetOffset;
        [SerializeField, ReadOnly] bool recentering = false;
        Vector3 recenterStartPos = Vector3.zero;
        float recenterStartTime = 0;


        [SerializeField, ReadOnly] bool isMouseOverUI = false;
        [SerializeField, ReadOnly] Vector3 cursorpos;
        [SerializeField, ReadOnly] bool isMouseDragging = false;
        [SerializeField, ReadOnly] Vector3 dragStartPos;
        [SerializeField, ReadOnly] Vector3 dragOffset;
        [SerializeField, ReadOnly] bool ignoreMouseUI;
        [SerializeField, ReadOnly] bool edgeMoveNeedReenter = false;
        [SerializeField, ReadOnly, Range(0f, 1f)] float zoomPerc = 0f;

        [SerializeField, HideInInspector]
        public Plane gamePlane = new Plane(Vector3.back, Vector3.zero);

        public Vector3 targetOffset => camTargetOffset;
        public Transform followTarget { get => camFollowTarget; set => camFollowTarget = value; }
        // public bool isBoundToCamCenter { get => boundaryAroundFollowTarget; set => boundaryAroundFollowTarget = value; }
        public Camera targetCamera => targetCam;

        float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        public float ZoomInput {
            get => zoomInput;
            set {
                if (disableZoom) return;
                zoomInput = value;
            }
        }
        public Vector2 MoveInput { get => moveInput; set => moveInput = value; }

        private void Reset() {
            targetCam = Camera.main;
            camFollowTarget = transform;
        }
        private void OnValidate() {
            targetCam ??= Camera.main;
            camFollowTarget ??= transform;
            zoomMaxHeight = Mathf.Max(zoomMinHeight, zoomMaxHeight);
            zoomDefHeight = Mathf.Clamp(zoomDefHeight, zoomMinHeight, zoomMaxHeight);
            zoomMaxValue = Mathf.Max(zoomMinValue, zoomMaxValue);
            zoomDefValue = Mathf.Clamp(zoomDefValue, zoomMinValue, zoomMaxValue);
        }
        private void Awake() {
            OnValidate();
        }
        private void Start() {
            if (recenterOnStart) {
                RecenterCamImm(false);
            }
        }
        private void OnEnable() {
#if ENABLE_INPUT_SYSTEM
            moveAction.ActivateCallbacks(OnMoveAction, OnMoveActionCancelled);
            moveDragAction.ActivateCallbacks(OnMoveDragAction, OnMoveDragActionCancelled);
            zoomContAction.ActivateCallbacks(OnZoomContAction);
            zoomBtnAction.ActivateCallbacks(OnZoomBtnAction, OnZoomBtnActionCancelled);
            recenterAction.ActivateCallbacks(OnRecenterAction);
#endif
        }

        private void OnDisable() {
#if ENABLE_INPUT_SYSTEM
            moveAction.DeactivateCallbacks(OnMoveAction, OnMoveActionCancelled);
            moveDragAction.DeactivateCallbacks(OnMoveDragAction, OnMoveDragActionCancelled);
            zoomContAction.DeactivateCallbacks(OnZoomContAction);
            zoomBtnAction.DeactivateCallbacks(OnZoomBtnAction, OnZoomBtnActionCancelled);
            recenterAction.DeactivateCallbacks(OnRecenterAction);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnMoveAction(InputAction.CallbackContext c) => MoveInput = c.ReadValue<Vector2>();
        private void OnMoveActionCancelled(InputAction.CallbackContext c) => MoveInput = Vector2.zero;
        private void OnMoveDragAction(InputAction.CallbackContext c) {
            if (!useUnscaledTime && Time.timeScale == 0) return;
            SetMoveDragStart();
        }
        private void OnMoveDragActionCancelled(InputAction.CallbackContext c) {
            if (!useUnscaledTime && Time.timeScale == 0) return;
            SetMoveDragEnd();
        }
        private void OnZoomContAction(InputAction.CallbackContext c) => SetZoomCont(c.ReadValue<float>());
        private void OnZoomBtnAction(InputAction.CallbackContext c) => ZoomInput = c.ReadValue<float>();
        private void OnZoomBtnActionCancelled(InputAction.CallbackContext c) => ZoomInput = 0;
        private void OnRecenterAction(InputAction.CallbackContext c) => RecenterCameraAction();
#endif

        public void SetMoveDragStart() {
            if (disableMovement || disableDragMovement) return;
            // ignoreMouseUI = EventSystem.current.IsPointerOverGameObject();
            dragStartInput = true;
        }
        public void SetMoveDragEnd() {
            if (disableMovement || disableDragMovement) return;
            dragEndInput = true;
        }
        public void SetZoomCont(float delta) {
            if (!useUnscaledTime && Time.timeScale == 0) return;
            if (!zoomContWhenMouseOverUI && isMouseOverUI) return;
            Zoom(delta, zoomContRate, false, false);
            // zoomInput = c.ReadValue<float>();
        }

        public void SetTargetOffset(Vector3 targetOffset, bool includeZ = false) {
            float oldHeight = camTargetOffset.z;
            camTargetOffset = ClampPosBounds(targetOffset);
            if (!includeZ) {
                camTargetOffset.z = oldHeight;
            } else {
                //handled in zoom
            }
        }
        private Vector3 ClampPosBounds(Vector3 newPos) {
            // var opos = newPos;
            float zoom = newPos.z;
            newPos.z = 0;
            if (boundaryActive) {
                if (boundaryLocalToTarget) {
                    if (boundAsCircle) {
                        newPos = Vector2.ClampMagnitude((Vector2)newPos + boundsCenterOffset, boundsMaxRadius) - boundsCenterOffset;
                    } else {
                        newPos.x = Mathf.Clamp(newPos.x + boundsCenterOffset.x, -boundsSize.x / 2f, boundsSize.x / 2f) - boundsCenterOffset.x;
                        newPos.y = Mathf.Clamp(newPos.y + boundsCenterOffset.y, -boundsSize.y / 2f, boundsSize.y / 2f) - boundsCenterOffset.y;
                    }
                }// else handled in updatepos
            }
            if (zoomUseCamZ) {
                zoom = Mathf.Clamp(zoom, zoomMinHeight, zoomMaxHeight);
                newPos.z = zoom;
            }
            // if (newPos != opos) {
            //     Debug.Log($"Clamping from:{opos} to:{newPos}");
            // }
            return newPos;
        }


        public void RecenterCameraAction() => RecenterCamImm(true, recenterActionIncludeZoom);
        public void RecenterCamImm(bool smooth = false, bool resetZoom = true) {
            float oldHeight = camTargetOffset.z;
            camTargetOffset = Vector3.zero;
            camTargetOffset.z = oldHeight;

            if (resetZoom) {
                if (zoomUseCamZ) {
                    camTargetOffset.z = zoomDefHeight;
                } else {
                    if (targetCam.orthographic) {
                        targetCam.orthographicSize = zoomDefValue;
                    } else {
                        targetCam.fieldOfView = zoomDefValue;
                    }
                }
            }
            if (smooth && recenterDuration > 0f) {
                recentering = true;
                recenterStartTime = Time.unscaledTime;
                recenterStartPos = transform.position;
            } else {
                transform.position = ClampPosBounds(camFollowTarget.position + camTargetOffset);
            }
        }
        private void FixedUpdate() {
            // MoveCam();
            if (updateFollowOnFixedUpdate) {
                UpdatePosition();
            }
        }
        private void Update() {
            MoveCam();
            // UpdatePosition();
        }
        private void LateUpdate() {
            // MoveCam();
            UpdatePosition();
        }
        private void MoveCam() {
            if (recentering) {
                // todo tween instead?
                // cant just lerp cause if target is moving we never reach it
                Vector3 camtarget = camFollowTarget.position + camTargetOffset;
                float percent = (Time.unscaledTime - recenterStartTime) / recenterDuration;
                if (percent < 1f) {
                    Vector3 newPos = camtarget;
                    newPos = Vector3.Lerp(recenterStartPos, newPos, percent);
                    // Debug.Log("Recentering d" + Vector3.Distance(transform.position, camtarget) + " op" + transform.position + " tp" + camtarget + " np" + newPos + " t:" + (Time.unscaledDeltaTime * recenterSmoothing));
                    transform.position = newPos;
                } else {
                    recentering = false;
                    transform.position = camtarget;
                }
                return;
            }

            if (Time.timeScale == 0) return;

            // todo will this work with new UI system?
            isMouseOverUI = EventSystem.current.IsPointerOverGameObject();

            if (ignoreMouseUI) dragStartInput = false;

            // zoom
            bool canZoomUI = !(isMouseOverUI && !zoomBtnWhenMouseOverUI);
            if (canZoomUI) {//zoomInput != 0 
                Zoom(zoomInput, zoomBtnRate);
            }
            if (disableMovement) return;

            // input
#if ENABLE_INPUT_SYSTEM
            Vector2 mousepos = Mouse.current.position.ReadValue();
#else
            Vector2 mousepos = Input.mousePosition;
#endif

            // bool dragStartInput = Mouse.current.leftButton.wasPressedThisFrame;
            // bool dragEndInput = Mouse.current.leftButton.wasReleasedThisFrame;

            // viewport input
            Vector3 mouseViewportPos = targetCam.ScreenToViewportPoint(mousepos);
            // Debug.Log("vp" + mouseViewportPos);
            bool ignoreMouseOOVPX = ignoreMouseOutOfViewport && (mouseViewportPos.x < 0f || mouseViewportPos.x > 1f);
            bool ignoreMouseOOVPY = ignoreMouseOutOfViewport && (mouseViewportPos.y < 0f || mouseViewportPos.y > 1f);
            bool ignoreMouseOOVP = ignoreMouseOOVPX || ignoreMouseOOVPY;

            // get cursor pos
            UpdateCursorPos();

            // check mouse vp edge
            Vector3 edgeMoveVel = Vector3.zero;
            if (mouseViewportPos.x <= moveEdgeDist && !ignoreMouseOOVPX) {
                edgeMoveVel.x = -1;
            }
            if (mouseViewportPos.x >= 1f - moveEdgeDist && !ignoreMouseOOVPX) {
                edgeMoveVel.x = 1;
            }
            if (mouseViewportPos.y <= moveEdgeDist && !ignoreMouseOOVPY) {
                edgeMoveVel.y = -1;
            }
            if (mouseViewportPos.y >= 1f - moveEdgeDist && !ignoreMouseOOVPY) {
                edgeMoveVel.y = 1;
            }
            if (edgeMoveVel == Vector3.zero) {
                // could have also used mousemovement, but whatever
                edgeMoveNeedReenter = false;
            }

            // move priority mouse drag -> keyboard input -> edge move
            if (isMouseDragging) {
                // drag move
                Vector3 deltadrag = -((cursorpos - camFollowTarget.position) - dragStartPos);
                Vector3 newPos = camTargetOffset + deltadrag;
                if (moveDragSmoothing > 0) newPos = Vector3.Lerp(camTargetOffset, newPos, DeltaTime * moveDragSmoothing);
                SetTargetOffset(newPos);
                if (dragEndInput || disableDragMovement) {
                    // finish drag
                    isMouseDragging = false;
                    edgeMoveNeedReenter = true;
                }
            } else {
                // check drag start
                //? move to input action func
                bool canMoveDragUI = !(isMouseOverUI && !moveDragWhenMouseOverUI);
                if (clickDragToMove && dragStartInput && !disableDragMovement && canMoveDragUI) {
                    // only need this if using leftclick
                    // todo? custom check func instead
                    // if (!checkBuilderMouseUseDrag || (builder == null || !builder.isUsingMouseDrag)) {
                    if (cursorpos != null) {
                        // start drag
                        isMouseDragging = true;
                        dragStartPos = cursorpos - camFollowTarget.position;
                        // dragOffset = camTargetOffset - dragStartPos;
                    }
                }

                float zoomFac = moveInputSpeedByZoom == 0 ? 1 : moveInputSpeedByZoom * zoomPerc + 1f;

                // input move
                bool canInputMoveUI = !(isMouseOverUI && !moveInputWhenMouseOverUI);
                if (moveInput.sqrMagnitude > 0.001f && !disableInputMovement && canInputMoveUI) {
                    // todo? combine all movetypes?
                    Vector3 moveInputVel = new Vector3(moveInput.x, moveInput.y, 0);
                    moveInputVel.Normalize();
                    Vector3 newPos = camTargetOffset + moveInputVel * moveInputSpeed * zoomFac * DeltaTime;
                    if (moveInputSmoothing > 0) newPos = Vector3.Lerp(camTargetOffset, newPos, DeltaTime * moveInputSmoothing);
                    SetTargetOffset(newPos);
                    edgeMoveNeedReenter = true;
                } else {
                    // edge move
                    bool canEdgeMoveUI = !(isMouseOverUI && !moveEdgeWhenMouseOverUI);
                    if (!edgeMoveNeedReenter || !doesEdgeMoveNeedReenter) {
                        if (hoverAtEdgeToMove && edgeMoveVel.sqrMagnitude > 0.001f && !disableEdgeMovement && canEdgeMoveUI) {
                            edgeMoveVel.Normalize();
                            Vector3 newPos = camTargetOffset;
                            newPos += edgeMoveVel * moveEdgeSpeed * zoomFac * DeltaTime;
                            if (moveEdgeSmoothing > 0) newPos = Vector3.Lerp(camTargetOffset, newPos, DeltaTime * moveEdgeSmoothing);
                            SetTargetOffset(newPos);
                        }
                    }
                }
            }

            dragStartInput = false;
            dragEndInput = false;
        }

        private void UpdateCursorPos() {
            Vector2 mousepos = Mouse.current.position.ReadValue();
            cursorpos = GetCursorPos(mousepos) ?? cursorpos;
        }

        public Vector2? GetCursorPos(Vector2 mousepos) {
            Ray mouseRay = targetCam.ScreenPointToRay(mousepos);
            if (gamePlane.Raycast(mouseRay, out var dist)) {
                return mouseRay.GetPoint(dist);
            } else {
                // invalid cursor position
                // use last one?
                return null;
            }
        }

        private void UpdatePosition(bool smoothing = true) {
            if (recentering) return;
            Vector3 followPos = camFollowTarget.position;
            if (smoothing && followSmoothing > 0) {
                followPos = Vector3.Lerp(camFollowTarget.position, followPos, DeltaTime * followSmoothing);
            }
            Vector3 camtarget = followPos + camTargetOffset;
            if (boundaryActive) {
                if (!boundaryLocalToTarget) {
                    if (boundAsCircle) {
                        camtarget = Vector2.ClampMagnitude((Vector2)camtarget + boundsCenterOffset, boundsMaxRadius) - boundsCenterOffset;
                    } else {
                        camtarget.x = Mathf.Clamp(camtarget.x + boundsCenterOffset.x, -boundsSize.x / 2f, boundsSize.x / 2f) - boundsCenterOffset.x;
                        camtarget.y = Mathf.Clamp(camtarget.y + boundsCenterOffset.y, -boundsSize.y / 2f, boundsSize.y / 2f) - boundsCenterOffset.y;
                    }
                }
            }// else handled in set target pos
            if (smoothing && finalSmoothing > 0) {
                camtarget = Vector3.Lerp(transform.position, camtarget, DeltaTime * finalSmoothing);
            }
            transform.position = camtarget;
        }

        void Zoom(float delta, float zoomRate, bool smooth = true, bool blockCursorZoom = true) {
            if (disableZoom) return;
            // Vector3 prezoomSP = targetCamera.WorldToScreenPoint(cursorpos);
            Vector3 oldCursorPos = cursorpos;
            float oldZoom;
            float zoom;
            if (zoomUseCamZ) {
                oldZoom = camTargetOffset.z;
                // float oldZoomPerc = zoomPerc;
                // bool didZoom = oldZoomPerc > 0f && oldZoomPerc < 1f || zoomPerc > 0f && zoomPerc < 1f;

                // Debug.Log($"Zooming {delta}");
                zoom = oldZoom + delta * zoomRate;
                if (smooth && zoomSmoothing > 0) zoom = Mathf.Lerp(oldZoom, zoom, DeltaTime * zoomSmoothing);

                Vector3 newPos = camTargetOffset;
                newPos.z = zoom;
                // this will clamp and zoom
                SetTargetOffset(newPos, true);
                zoomPerc = 1f - Mathf.InverseLerp(zoomMinHeight, zoomMaxHeight, camTargetOffset.z);
            } else {
                // todo test
                if (targetCam.orthographic) {
                    oldZoom = targetCam.orthographicSize;
                    zoom = oldZoom + -delta * zoomRate;
                    if (smooth && zoomSmoothing > 0) zoom = Mathf.Lerp(oldZoom, zoom, DeltaTime * zoomSmoothing);

                    zoom = Mathf.Clamp(zoom, zoomMinValue, zoomMaxValue);
                    targetCam.orthographicSize = zoom;
                } else {
                    oldZoom = targetCam.fieldOfView;
                    zoom = oldZoom + delta * zoomRate;
                    if (smooth && zoomSmoothing > 0) zoom = Mathf.Lerp(oldZoom, zoom, DeltaTime * zoomSmoothing);

                    zoom = Mathf.Clamp(zoom, zoomMinValue, zoomMaxValue);
                    targetCam.fieldOfView = zoom;
                }
                zoomPerc = 1f - Mathf.InverseLerp(zoomMinValue, zoomMaxValue, zoom);
            }
            bool didZoom = oldZoom != zoom;

            if (zoomToCursorRate > 0 && cursorpos != null && !blockCursorZoom) {
                // dont move to cursor if at max or min zoom
                if (didZoom) {
                    // zoom instantly
                    UpdatePosition(false);
                    UpdateCursorPos();
                    // zoom into cursor pos
                    // Debug.Log($"{oldCursorPos} {cursorpos} {camTargetOffset} {delta}");
                    // note fails if boundaries clamps pos
                    // want whatever was under mouse to be at same relative position
                    // this moves the camera laterally, so dont need to rezoom
                    Vector3 targetPos = oldCursorPos - cursorpos + camTargetOffset;
                    targetPos.z = camTargetOffset.z;
                    SetTargetOffset(targetPos);
                    UpdatePosition(false);
                    UpdateCursorPos();
                }
            }
        }
        private void OnDrawGizmos() {
#if UNITY_EDITOR
            if (drawBounds && boundaryActive) {
                using (new Handles.DrawingScope(Color.black)) {
                    Vector3 center = Vector3.zero + (Vector3)boundsCenterOffset;
                    if (boundaryLocalToTarget) {
                        if (camFollowTarget == null) return;
                        center += camFollowTarget.position;
                    }
                    if (boundAsCircle) {
                        Handles.DrawWireDisc(center, Vector3.back, boundsMaxRadius);
                    } else {
                        Vector3 p0 = center + -boundsSize.x / 2 * Vector3.left + boundsSize.y / 2 * Vector3.up;
                        Vector3 p1 = center + boundsSize.x / 2 * Vector3.left + boundsSize.y / 2 * Vector3.up;
                        Vector3 p2 = center + boundsSize.x / 2 * Vector3.left - boundsSize.y / 2 * Vector3.up;
                        Vector3 p3 = center + -boundsSize.x / 2 * Vector3.left - boundsSize.y / 2 * Vector3.up;
                        Vector3[] points = new Vector3[5] { p0, p1, p2, p3, p0 };
                        Handles.DrawAAPolyLine(5, points);
                    }
                }
            }
#endif
        }
    }
}