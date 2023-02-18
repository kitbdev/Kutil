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
        public bool clampOne;

        public Vector2DDrawAttribute(float height = 70, bool normalize = false, bool clampOne = true) {
            this.height = height;
            // this.color = color;
            this.normalize = normalize;
            this.clampOne = clampOne;
        }
    }
}