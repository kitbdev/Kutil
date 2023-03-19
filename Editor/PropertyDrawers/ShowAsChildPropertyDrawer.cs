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

        protected PropertyField childField;

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
                Debug.LogWarning($"ShowAsChildPropertyDrawer failed to find child '{childName}' on {(property.serializedObject?.targetObject?.name ?? "unkown")}");
                return base.CreatePropertyGUI(property);
            }
            VisualElement root = new VisualElement();
            root.name = "ShowAsChildPropertyDrawer";

            childField = new PropertyField(selNameProp, property.displayName);
            // childField.AddToClassList(PropertyField.)
            // childField.name = property.displayName;
            // Debug.Log(property.displayName);
            root.Add(childField);
            childField.tooltip = property.displayName;
            // childField.label = property.displayName;

            // set the label after the property has been binded, cause sometimes it doesnt work
            // childField.RegisterCallback<GeometryChangedEvent>(OnGeoChanged);
            childField.RegisterCallback<GeometryChangedEvent, SerializedProperty>(OnGeoChanged, property);
            // _ = childField.schedule.Execute(() => {
            //     UpdateLabel(property, childField);
            // });
            return root;
        }

        private void OnGeoChanged(GeometryChangedEvent gce, SerializedProperty property) {
            childField.UnregisterCallback<GeometryChangedEvent, SerializedProperty>(OnGeoChanged);
            UpdateLabel(property);
        }

        private void UpdateLabel(SerializedProperty property) {
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