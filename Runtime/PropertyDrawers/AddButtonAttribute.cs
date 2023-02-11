using System;
using UnityEngine;

namespace Kutil {
    public class AddButtonAttribute : PropertyAttribute {

        public enum ButtonLayout {
            BEFORE, REPLACE, AFTER,
            NONE, LEFT, RIGHT
        }

        public string buttonMethodName;
        public string buttonLabel;
        public AddButtonAttribute.ButtonLayout buttonLayout;
        /// <summary>parameters to pass into the call (static)</summary>
        public object[] parameters;
        public float btnWidth;
        public UnityEngine.UIElements.LengthUnit lengthUnit;
        /// <summary>when multiselecting, should the call go to all targets or just the first</summary>
        public bool allowMultipleCalls;

        // todo have multiple buttons next to each other?

        public AddButtonAttribute(string buttonMethodName, string buttonLabel = null, object[] parameters = null, ButtonLayout buttonLayout = ButtonLayout.REPLACE, float btnWidth = 50, UnityEngine.UIElements.LengthUnit lengthUnit = UnityEngine.UIElements.LengthUnit.Percent, bool allowMultipleCalls=true) {
            this.buttonMethodName = buttonMethodName;
            this.buttonLabel = buttonLabel;
            this.buttonLayout = buttonLayout;
            this.parameters = parameters;
            this.btnWidth = btnWidth;
            this.lengthUnit = lengthUnit;
            this.allowMultipleCalls = allowMultipleCalls;
        }
    }
}