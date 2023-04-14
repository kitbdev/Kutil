using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    // Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
    // Modified by: Sebastian Lague
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    // | AttributeTargets.Class | AttributeTargets.Struct
    /// <summary>
    /// Conditionally hide a serialized property in the inspector.
    /// Can use properties or methods to get value, can use lambdas for more flexible behavior
    /// </summary>
    public class ConditionalHideAttribute : PropertyAttribute {

        public string conditionalSourceField { get; set; }
        public bool showIfTrue { get; set; } = true;
        public int[] enumIndices { get; set; } = null;

        public bool readonlyInstead { get; set; } = false;

        public ConditionalHideAttribute(string boolVariableName, bool showIfTrue) {
            conditionalSourceField = boolVariableName;
            this.showIfTrue = showIfTrue;
        }

        public ConditionalHideAttribute(string enumVariableName, params int[] enumIndices) {
            conditionalSourceField = enumVariableName;
            this.enumIndices = enumIndices;
            this.showIfTrue = true;
        }
        public ConditionalHideAttribute(string enumVariableName, bool showIfTrue = true, params int[] enumIndices) {
            conditionalSourceField = enumVariableName;
            this.enumIndices = enumIndices;
            this.showIfTrue = showIfTrue;
        }
        
    }
}