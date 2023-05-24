using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Marks an end to a Collapsable section
    /// Optional, Collapsables will end on their own according to their settings. 
    /// Does nothing if not below a Collapsable attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CollapsableEndAttribute : PropertyAttribute {
        public string connectedFieldName { get; set; } = null;

        public CollapsableEndAttribute() { }
        public CollapsableEndAttribute(string connectedFieldName) {
            this.connectedFieldName = connectedFieldName;
        }
    }
}