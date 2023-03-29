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
    /// Use [UnityEditor.CustomPropertyDrawer(typeof(...))] and override childName property.
    /// </summary>
    public abstract class ShowAsChildPropertyDrawer : PropertyDrawer {

        public static readonly string showAsChildClass = "kutil-show-as-child";

        /// <summary>name of the child field to show instead</summary>
        public abstract string childName { get; }

        public virtual bool forceLabelUpdate => true;

        protected PropertyField childField;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (childName == null) {
                Debug.LogWarning($"ShowAsChildPropertyDrawer failed, make sure to override childName");
                base.OnGUI(position, property, label);
                return;
            }
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
            if (childName == null) {
                Debug.LogWarning($"ShowAsChildPropertyDrawer failed, make sure to override childName");
                return base.CreatePropertyGUI(property);
            }
            SerializedProperty selNameProp = property.FindPropertyRelative(childName);
            if (selNameProp == null) {
                Debug.LogWarning($"ShowAsChildPropertyDrawer failed to find child '{childName}' on {(property.serializedObject?.targetObject?.name ?? "unkown")} make sure to set childName correctly");
                return base.CreatePropertyGUI(property);
            }
            VisualElement root = new VisualElement();
            root.AddToClassList(showAsChildClass);
            root.name = "ShowAsChildPropertyDrawer";

            // Debug.Log($"show as child {preferredLabel} {property.displayName}");
            childField = new PropertyField(selNameProp, property.displayName);
            root.Add(childField);

            // childField.name = property.displayName;
            // Debug.Log(property.displayName);
            // childField.tooltip = property.displayName;
            // childField.label = property.displayName;

            // set the label after the property has been binded, cause sometimes it doesnt work
            if (forceLabelUpdate) {
                childField.RegisterCallback<GeometryChangedEvent, SerializedProperty>(OnGeoChanged, property);
            }
            return root;
        }
        private void OnGeoChanged(GeometryChangedEvent gce, SerializedProperty property) {
            childField.UnregisterCallback<GeometryChangedEvent, SerializedProperty>(OnGeoChanged);
            UpdateLabel(property);
        }

        private void UpdateLabel(SerializedProperty property) {
            // fix all fields with a propertyfield label. 
            // unless theyre in another propertyfield?
            // const string labelClass = "unity-property-field__label";
            // const string labelClass = "unity-base-field__label";
            var labels = childField.Query<Label>(null, PropertyField.labelUssClassName).ToList();
            foreach (var label in labels) {
                PropertyField propertyField = label.GetFirstAncestorOfType<PropertyField>();
                if (propertyField != childField) continue;
                label.text = property.displayName;
            }
            // childField.label = property.displayName;
            // Debug.Log(childField.label + " - " + property.displayName);
        }
    }
}