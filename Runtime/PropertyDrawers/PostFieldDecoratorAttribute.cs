using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Puts Decorators after this attribute after the field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class PostFieldDecoratorAttribute : PropertyAttribute {

        public PostFieldDecoratorAttribute() {

        }
    }
}