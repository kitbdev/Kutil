using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(NameListElementsAttribute))]
    public class NameListElementsDrawer : PropertyDrawer {

        static System.Func<int, string> defFunc = i => "Element " + i;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Debug.Log(property.propertyPath);

            NameListElementsAttribute nameListElementAtt = (NameListElementsAttribute)attribute;
            System.Func<int, string> elementNamer =
                GetFunc(nameListElementAtt.elementNameFuncField, property) ?? defFunc;

            string parentPath = property.propertyPath.Replace("." + property.name, "");
            if (parentPath.EndsWith(']')) {
                // in an array element name
                string propertyPath = property.propertyPath;
                int startIndex = propertyPath.LastIndexOf('[') + 1;
                if (int.TryParse(propertyPath.Substring(startIndex, propertyPath.LastIndexOf(']') - startIndex), out int index)) {
                    label.text = elementNamer(index);
                } else {
                    Debug.LogError("NameListElementsDrawer couldnt get array index");
                }
            } else if (property.propertyPath.EndsWith(']')) {
                // we are a child of an array
                // todo create list of child elements
                EditorGUI.PropertyField(position, property, label, true);
            } else {
                // ignore
                // EditorGUI.PropertyField(position, property, label, true);
                base.OnGUI(position, property, label);
            }
        }
        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        //     VisualElement root = new VisualElement();
        //     return root;
        // }

        public static System.Func<int, string> GetFunc(string fieldname, SerializedProperty property) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(property.name, fieldname);
            if (ReflectionHelper.TryGetValue<System.Func<int, string>>(targetObject, path, out var val)) {
                return val;
            }
            return null;
        }
    }
}