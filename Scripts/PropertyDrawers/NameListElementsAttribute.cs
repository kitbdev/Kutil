using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class NameListElementsAttribute : PropertyAttribute {
        public string elementNameFuncField;
        // public string[] elementNames = null;

        /// <summary>
        /// NameListElementsAttribute use a System.Func<int,string> to determine element names
        /// </summary>
        /// <param name="elementNameFuncField">nameof a System.Func<int,string> (index is passed)</param>
        public NameListElementsAttribute(string elementNameFuncField) {
            this.elementNameFuncField = elementNameFuncField;
        }
        // public NameListElementsAttribute(params string[] elementNames) {
        //     this.elementNames = elementNames;
        // }
    }
}