using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    [AttributeUsage(
        //AttributeTargets.Class | AttributeTargets.Struct
     AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ShowAsChildAttribute : PropertyAttribute {
        public string childSourceField;
        public bool showAsParent = false;// probably really dumb
        // todo? multiple children?
        // todo? keep label
        // todo? show as all children

        public ShowAsChildAttribute(string choicesListSourceField, bool showAsParent = false) {
            this.childSourceField = choicesListSourceField;
            this.showAsParent = showAsParent;
        }
    }
}