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
        
        /// <summary>more complex disable logic that permits scrolling in scrollviews. may be slower</summary>
        public bool allowArrayScrolling { get; set; } = false;
        
        /// <summary>does this property need to update whenever geometry changes?</summary>
        public bool updateOften { get; set; } = false;

        public ReadOnlyAttribute() { }
    }
}