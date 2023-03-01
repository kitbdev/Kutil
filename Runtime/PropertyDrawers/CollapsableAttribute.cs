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
        public bool includeHeaders { get; set; } = false;
        public bool includeSpaces { get; set; } = false;
        public bool includeOtherDecorators { get; set; } = false;
        // public bool isCollapsed;


        public CollapsableAttribute(string text = "", bool startCollapsed = false, bool includeHeaders = false, bool includeSpaces = true, bool includeOtherDecorators = true) {
            this.text = text;
            this.startCollapsed = startCollapsed;
            this.includeHeaders = includeHeaders;
            this.includeSpaces = includeSpaces;
            this.includeOtherDecorators = includeOtherDecorators;
        }
    }
}