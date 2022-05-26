using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Starts a foldout section.
    /// Must be paired with a FoldEnd!
    /// Cannot be nested
    /// </summary>
    //todo doesnt work
    // [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class FoldStartAttribute : PropertyAttribute {
        public string header = null;

        public FoldStartAttribute(string header = null) {
            this.header = header;
        }
    }
}