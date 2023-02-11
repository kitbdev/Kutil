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
        public ButtonLayout buttonLayout;
        public object[] parameters;
        public float btnWidth;
        // todo have multiple buttons next to each other?

        public AddButtonAttribute(string buttonMethodName, string buttonLabel = null, object[] parameters = null, ButtonLayout buttonLayout = ButtonLayout.REPLACE, float btnWidth = 50) {
            this.buttonMethodName = buttonMethodName;
            this.buttonLabel = buttonLabel;
            this.buttonLayout = buttonLayout;
            this.parameters = parameters;
            this.btnWidth = btnWidth;
        }
    }
}