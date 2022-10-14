using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kutil {
    /// <summary>
    /// Utility and helper scripts for Input system
    /// </summary>
    public static class InputActionReferenceExt {

#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// if the InputActionReference is assigned, enables it and assigns callbacks. Call in OnEnable
    /// </summary>
    /// <param name="iar">InputActionReference</param>
    /// <param name="performAction"></param>
    /// <param name="cancelAction"></param>
    /// <param name="startedAction"></param>
    public static void ActivateCallbacks(this InputActionReference iar,
    Action<InputAction.CallbackContext> performAction = null,
    Action<InputAction.CallbackContext> cancelAction = null,
    Action<InputAction.CallbackContext> startedAction = null) {
        if (iar != null) {
            iar.action.Enable();
            if (performAction != null) iar.action.performed += performAction;
            if (cancelAction != null) iar.action.canceled += cancelAction;
            if (startedAction != null) iar.action.started += startedAction;
        }
    }

    /// <summary>
    /// if the InputActionReference is assigned, disables it and removes callbacks. Call in OnDisable
    /// </summary>
    /// <param name="iar">InputActionReference</param>
    /// <param name="performAction"></param>
    /// <param name="cancelAction"></param>
    /// <param name="startedAction"></param>
    public static void DeactivateCallbacks(this InputActionReference iar,
    Action<InputAction.CallbackContext> performAction = null,
    Action<InputAction.CallbackContext> cancelAction = null,
    Action<InputAction.CallbackContext> startedAction = null) {
        if (iar != null) {
            iar.action.Disable();
            if (performAction != null) iar.action.performed -= performAction;
            if (cancelAction != null) iar.action.canceled -= cancelAction;
            if (startedAction != null) iar.action.started -= startedAction;
        }
    }
#endif
    }
}