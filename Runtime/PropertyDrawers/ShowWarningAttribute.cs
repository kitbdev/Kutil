using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;

namespace Kutil {
    /// <summary>
    /// Conditionally show a warning in the inspector
    /// Can use properties or methods to get value, can use lambdas for more flexible behavior
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ShowWarningAttribute : PropertyAttribute {

        public bool isDynamic { get; set; } = false;
        public bool useTextAsSourceField { get; set; } = false;
        public string conditionalSourceField { get; set; } = null;
        public string warningText { get; set; } = "Warning";
        public bool showIfTrue { get; set; } = true;
        public int[] enumIndices { get; set; } = null;
        public HelpBoxMessageType helpBoxMessageType { get; set; } = HelpBoxMessageType.Warning;

        public ShowWarningAttribute(string conditionalSourceField, string warningText, HelpBoxMessageType helpBoxMessageType = HelpBoxMessageType.Warning, bool showIfTrue = true, int[] enumIndices = null) {
            this.isDynamic = true;
            this.conditionalSourceField = conditionalSourceField;
            this.warningText = warningText;
            this.showIfTrue = showIfTrue;
            this.enumIndices = enumIndices;
        }
        public ShowWarningAttribute(string warningText, HelpBoxMessageType helpBoxMessageType = HelpBoxMessageType.Warning, bool showIfTrue = true, int[] enumIndices = null) {
            this.isDynamic = false;
            this.warningText = warningText;
            this.helpBoxMessageType = helpBoxMessageType;
            this.conditionalSourceField = null;
            this.showIfTrue = showIfTrue;
            this.enumIndices = enumIndices;
        }

        public ShowWarningAttribute() { }
    }
}