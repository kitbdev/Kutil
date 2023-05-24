using System;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Clamp a value between two other values.
    /// works with float and int fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ClampAttribute : PropertyAttribute {

        public float minValue { get; set; }
        public float maxValue { get; set; }
        // public float minValueY { get; set; }
        // public float maxValueY { get; set; }
        // public float minValueZ { get; set; }
        // public float maxValueZ { get; set; }

        public ClampAttribute(float minValue = float.NegativeInfinity, float maxValue = float.PositiveInfinity) {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }


        public static int IntVal(float value) {
            if (float.IsNegativeInfinity(value)) {
                return int.MinValue;
            } else if (float.IsPositiveInfinity(value)) {
                return int.MaxValue;
            } else {
                return (int)value;
            }
        }
    }
}