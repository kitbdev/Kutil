using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    /// <summary>
    /// Conditionally hide the following decorator in the inspector.
    /// Can use properties or methods to get value, can use lambdas for more flexible behavior
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConditionalHideDecoratorsAttribute : PropertyAttribute {

        public string conditionalSourceField { get; set; }
        public int numToHide { get; set; } = 1;
        public bool showIfTrue { get; set; } = true;
        public int[] enumIndices { get; set; } = null;

        public bool readonlyInstead { get; set; } = false;

        public ConditionalHideDecoratorsAttribute(string boolVariableName, bool showIfTrue, int numToHide = 1) {
            conditionalSourceField = boolVariableName;
            this.showIfTrue = showIfTrue;
            this.numToHide = numToHide;
        }

        public ConditionalHideDecoratorsAttribute(string enumVariableName, params int[] enumIndices) {
            conditionalSourceField = enumVariableName;
            this.enumIndices = enumIndices;
            this.showIfTrue = true;
        }
        public ConditionalHideDecoratorsAttribute(string enumVariableName, bool showIfTrue = true, params int[] enumIndices) {
            conditionalSourceField = enumVariableName;
            this.enumIndices = enumIndices;
            this.showIfTrue = showIfTrue;
        }
    }
}