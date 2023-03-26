using System;
using Unity.Mathematics;
using UnityEngine;

namespace Kutil {
    // todo decorator horizontal container decorator? decoratorgroupstart/end. rearranges decorators locally

    /// <summary>
    /// Have a bounds editor tool for this field.
    /// available on Bounds and BoundsInt
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class BoundsEditorToolAttribute : PropertyAttribute {
        public bool showEditButton { get; set; } = true;
        public bool showResetButton { get; set; } = false;
        public bool showBoundsWhenInactive { get; set; } = true;
        public bool useTransformScaleAndRotation { get; set; } = true;

        // todo some way to offset
        // public float3 offset { get; set; }
        //?
        // public string offsetFieldName { get; set; }

        // todo scale?
        public float scale { get; set; } = 1f;


        public BoundsEditorToolAttribute() { }

        public BoundsEditorToolAttribute(bool showEditButton, bool showResetButton, bool showBoundsWhenInactive) {
            this.showEditButton = showEditButton;
            this.showResetButton = showResetButton;
            this.showBoundsWhenInactive = showBoundsWhenInactive;
        }

    }
}