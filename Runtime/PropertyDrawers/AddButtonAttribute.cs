using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Add a button before the next field in the inspector.
    /// Can have multiple in a row.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class AddButtonAttribute : PropertyAttribute {

        public string buttonMethodName { get; set; }
        public string buttonLabel { get; set; }
        /// <summary>parameters to pass into the call (static)</summary>
        public object[] parameters { get; set; }
        public float btnWeight { get; set; }
        /// <summary>when multiselecting, should the call go to all targets or just the first</summary>
        public bool allowMultipleCalls { get; set; }
        /// <summary>can this be called in the editor?</summary>
        public bool allowCallInEditor { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="buttonMethodName">nameof the method to call</param>
        /// <param name="buttonLabel">label on the button. will use method name by default</param>
        /// <param name="parameters">parameters to pass into the call (static)</param>
        /// <param name="allowMultipleCalls">when multiselecting, should the call go to all targets or just the first</param>
        /// <param name="allowCallInEditor">when multiselecting, should the call go to all targets or just the first</param>
        public AddButtonAttribute(string buttonMethodName, string buttonLabel = null, object[] parameters = null, bool allowCallInEditor = true, bool allowMultipleCalls = true) {
            this.buttonMethodName = buttonMethodName;
            this.buttonLabel = buttonLabel;
            this.parameters = parameters;
            this.allowMultipleCalls = allowMultipleCalls;
            this.allowCallInEditor = allowCallInEditor;
        }
    }
}