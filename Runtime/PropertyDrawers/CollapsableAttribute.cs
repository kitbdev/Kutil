using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Puts this field and those below it into a collapsable foldout
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CollapsableAttribute : PropertyAttribute {
        public string text { get; set; }
        public bool startCollapsed { get; set; }
        // public bool isCollapsed;

        public CollapsableAttribute(string text = "", bool startCollapsed = false) {
            this.text = text;
            this.startCollapsed = startCollapsed;
            // this.isCollapsed = this.startCollapsed;
        }
    }
}