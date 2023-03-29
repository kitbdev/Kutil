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
        /// <summary>When the tool is inactive, should the bounds be shown?</summary>
        public bool showBoundsWhenInactive { get; set; } = true;

        /// <summary>
        /// #RRGGBBAA color hexadecimal.
        /// ex: 'red' or '#FF0000FF' or '#FF0000'.
        /// set to null to use default.
        /// ref: https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html
        /// </summary>
        public string handleColorHtmlString { get; set; } = null;
        /// <summary>handle color to use when inactive. see handleColorHtmlString</summary>
        public string handleInactiveColorHtmlString { get; set; } = null;

        /// <summary>should the transforms scale and rotation be applied to the handle</summary>
        public bool useTransformScaleAndRotation { get; set; } = true;
        //? split scale and rot


        // todo? some simple way to offset
        // public float3 offset { get; set; }
        //?
        // public string offsetFieldName { get; set; }

        public float scale { get; set; } = 1f;


        public BoundsEditorToolAttribute() { }

        public BoundsEditorToolAttribute(bool showEditButton, bool showResetButton, bool showBoundsWhenInactive) {
            this.showEditButton = showEditButton;
            this.showResetButton = showResetButton;
            this.showBoundsWhenInactive = showBoundsWhenInactive;
        }

    }
}