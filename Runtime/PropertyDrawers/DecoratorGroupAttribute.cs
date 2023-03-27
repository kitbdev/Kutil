using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;

namespace Kutil {
    /// <summary>
    /// Start a decorator group to hold other decorators
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DecoratorGroupAttribute : PropertyAttribute {
        // public DisplayStyle displayStyle { get; set; }
        public FlexDirection flexDirection { get; set; } = FlexDirection.Row;
        public Justify justifyContent { get; set; } = Justify.SpaceBetween;
        // color or something?
        public float elementsflexGrow { get; set; } = 1f;

    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DecoratorGroupEndAttribute : PropertyAttribute {

    }
}