using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Register a callback method or property when this field is set in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ValueChangeCallbackAttribute : PropertyAttribute {

        public string callbackMethodName { get; set; }

        public ValueChangeCallbackAttribute(string callbackMethodName) {
            this.callbackMethodName = callbackMethodName;
        }
    }
}