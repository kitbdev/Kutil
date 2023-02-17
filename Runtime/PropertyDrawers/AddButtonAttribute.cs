using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Add a button before the next field in the inspector.
    /// Can have multiple in a row.
    /// change position with buttonLayout = PropLayout.After
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class AddButtonAttribute : PropertyAttribute {

        // public enum ButtonLayout {
        //     BEFORE, REPLACE, AFTER,
        //     NONE, LEFT, RIGHT
        // }

        public string buttonMethodName;
        public string buttonLabel;
        public PropLayout buttonLayout;
        /// <summary>parameters to pass into the call (static)</summary>
        public object[] parameters;
        public float btnWidth;
        public UnityEngine.UIElements.LengthUnit lengthUnit;
        public float btnWeight;
        /// <summary>when multiselecting, should the call go to all targets or just the first</summary>
        public bool allowMultipleCalls;

        /// <summary>
        /// </summary>
        /// <param name="buttonMethodName">nameof the method to call</param>
        /// <param name="buttonLabel">label on the button. will use method name by default</param>
        /// <param name="parameters">parameters to pass into the call (static)</param>
        /// <param name="buttonLayout">where should the button be relative to the property</param>
        /// <param name="btnWidth"></param>
        /// <param name="btnWeight">weight how much space this element should take up</param>
        /// <param name="allowMultipleCalls">when multiselecting, should the call go to all targets or just the first</param>
        public AddButtonAttribute(string buttonMethodName, string buttonLabel = null, object[] parameters = null, PropLayout buttonLayout = PropLayout.Above, float btnWidth = 50, float btnWeight = 1, bool allowMultipleCalls = true) {
            this.buttonMethodName = buttonMethodName;
            this.buttonLabel = buttonLabel;
            this.buttonLayout = buttonLayout;
            this.parameters = parameters;
            this.btnWidth = btnWidth;
            this.btnWeight = btnWeight;
            this.allowMultipleCalls = allowMultipleCalls;
        }
    }
}