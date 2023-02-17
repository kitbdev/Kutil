using System;
using UnityEngine;

namespace Kutil {

    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class Vector2DDrawAttribute : PropertyAttribute {
        public float height;
        public Color color;
        public bool normalize;

        public Vector2DDrawAttribute(float height = 70, bool normalize = true) {
            this.height = height;
            // this.color = color;
            this.normalize = normalize;
        }
    }
}