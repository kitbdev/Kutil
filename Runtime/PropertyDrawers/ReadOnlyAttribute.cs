using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Fields with this Attribute will be Read-Only in the Inspector.
    /// Works on fields including top-level Arrays and Lists.
    /// Does not work under an imgui context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute {
        public ReadOnlyAttribute() { 
            order = -100;
        }
    }
}