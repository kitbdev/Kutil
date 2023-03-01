using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Fields with this Attribute will be Read-Only in the inspector. Works with arrays.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyArrayAttribute : PropertyAttribute {
        public ReadOnlyArrayAttribute() { 
            order = -100;
        }
    }
}