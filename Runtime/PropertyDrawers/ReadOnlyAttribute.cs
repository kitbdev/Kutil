using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Fields with this Attribute will be Read-Only in the inspector. Does not work with arrays.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute {
        public ReadOnlyAttribute() { 
            order = -100;
        }
    }
}