using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// Inherit this for an easy way to show as child.
    /// Use [CustomPropertyDrawer(typeof())] and override childName property.
    /// </summary>
    public class ShowAsChildPropertyDrawer : PropertyDrawer {
        public virtual string childName => "";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty selNameProp = property.FindPropertyRelative(childName);
            if (selNameProp == null) {
                base.OnGUI(position, property, label);
                return;
            }
            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                EditorGUI.PropertyField(position, selNameProp, scope.content, false);
            }
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty selNameProp = property.FindPropertyRelative(childName);
            if (selNameProp == null) {
                return base.CreatePropertyGUI(property);
            }
            VisualElement root = new VisualElement();
            root.name = "ShowAsChildPropertyDrawer";
            PropertyField childField = new PropertyField(selNameProp, property.displayName);
            // childField.name = property.displayName;
            // Debug.Log(property.displayName);
            root.Add(childField);
            // childField.BindProperty(selNameProp);
            childField.label = property.displayName;
            
            // set the label after the property has been binded, cause sometimes it doesnt work
            childField.RegisterCallback<GeometryChangedEvent>(gce => {
                UpdateLabel(property, childField);

            });
            // _ = childField.schedule.Execute(() => {
            //     UpdateLabel(property, childField);
            // });
            return root;
        }

        private static void UpdateLabel(SerializedProperty property, PropertyField childField) {
            // property label should be first
            // const string labelClass = "unity-property-field__label";
            const string labelClass = "unity-base-field__label";
            var myLabel = childField.Q<Label>(null, labelClass);
            if (myLabel != null) {
                myLabel.text = property.displayName;
            }
            childField.label = property.displayName;
            // Debug.Log(childField.label + " - " + property.displayName);
        }
    }
}