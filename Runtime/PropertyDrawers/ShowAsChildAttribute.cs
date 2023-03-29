// using UnityEngine;
// using System;
// using System.Collections;

//! ths only way this might be possible is to have a custom decorator that replaces the parent's property field

// namespace Kutil {
//     [AttributeUsage(
//      //AttributeTargets.Class | AttributeTargets.Struct
//      AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
//      /// <summary>
//      /// </summary>
//     public class ShowAsChildAttribute : PropertyAttribute {

//         public string childSourceField { get; set; }

//         // public bool showAsParent { get; set; } = false;// probably really dumb
//         // todo? multiple children?
//         // todo? keep label
//         // todo? show as all children

//         // this is a valid param
//         public Type test { get; set; }

//         public ShowAsChildAttribute(string childSourceField, bool showAsParent = false) {
//             this.childSourceField = childSourceField;
//             this.showAsParent = showAsParent;
//         }
//     }
// }