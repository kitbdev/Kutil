// using UnityEngine;
// using UnityEditor;
// using UnityEngine.UIElements;
// using UnityEditor.UIElements;
// // using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;

// namespace Kutil {
//     [CustomPropertyDrawer(typeof(ShowAsChildAttribute))]
//     public class ShowAsChildDrawer : PropertyDrawer {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//             ShowAsChildAttribute sacAttribute = (ShowAsChildAttribute)attribute;
//             SerializedProperty newProp;
//             if (sacAttribute.showAsParent) {
//                 string parpath = property.propertyPath.Remove(property.propertyPath.IndexOf(property.name) - 1);
//                 newProp = property.serializedObject.FindProperty(parpath);
//             } else {
//                 newProp = property.FindPropertyRelative(sacAttribute.childSourceField);
//             }
//             EditorGUI.PropertyField(position, newProp, label, false);
//         }
//         public override VisualElement CreatePropertyGUI(SerializedProperty property) {
//             VisualElement root = new VisualElement();
//             // Label label = new Label(property.displayName);
//             ShowAsChildAttribute sacAttribute = (ShowAsChildAttribute)attribute;
//             SerializedProperty newProp;
//             if (sacAttribute.showAsParent) {
//                 string parpath = property.propertyPath.Remove(property.propertyPath.IndexOf(property.name) - 1);
//                 newProp = property.serializedObject.FindProperty(parpath);
//             } else {
//                 newProp = property.FindPropertyRelative(sacAttribute.childSourceField);
//             }
//             PropertyField chieldPropField = new PropertyField(newProp, property.displayName);
//             chieldPropField.BindProperty(newProp);
//             root.Add(chieldPropField);
//             return root;
//         }
//     }
// }