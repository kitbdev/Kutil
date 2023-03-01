using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Fields with this Attribute will be Read-Only in the inspector. 
    /// Does not work with top level arrays and lists. 
    /// Can work with IMGUI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyElementAttribute : PropertyAttribute {
        public ReadOnlyElementAttribute() { 
            order = -100;
        }
    }
}