using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Puts this field and those below it into a collapsable foldout
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CollapsableAttribute : PropertyAttribute {
        public string text { get; set; }

        public CollapsableAttribute(string text = "") {
            this.text = text;
        }
    }
}