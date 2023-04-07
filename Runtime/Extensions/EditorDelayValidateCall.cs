using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kutil {
    //https://github.com/Demigiant/dotween/issues/539#issuecomment-1133940533
    /// <summary>
    /// Fix issue when doing things during awake or onvalidate in the editor
    /// </summary>
    public static class EditorDelayValidateCall {
    
    #if UNITY_EDITOR
        struct CallbackData {
            public MonoBehaviour monoBehaviour;
            public Action action;
        }
        static List<CallbackData> callbackQueue;
    #endif
    
        /// <summary>
        /// Use this to avoid Warning for SendMessage cannot be called during Awake, CheckConsistency, or OnValidate
        /// </summary>
        /// <param name="monoBehaviour">this</param>
        /// <param name="action"></param>
        public static void Handle(MonoBehaviour monoBehaviour, Action action) {
    #if UNITY_EDITOR
            if (callbackQueue == null) callbackQueue = new();
            callbackQueue.Add(new() { monoBehaviour = monoBehaviour, action = action });
            if (callbackQueue.Count == 1) {
                UnityEditor.EditorApplication.delayCall += ValidateCallback;
            }
    #endif
        }
    
    #if UNITY_EDITOR
        static void ValidateCallback() {
            UnityEditor.EditorApplication.delayCall -= ValidateCallback;
            foreach (var callback in callbackQueue) {
                if (callback.monoBehaviour == null) {
                    // MissingRefException if managed in the editor - uses the overloaded Unity == operator.
                    continue;
                }
                // do action here
                callback.action?.Invoke();
            }
            // clear every time
            callbackQueue.Clear();
        }
    #endif
    }
}