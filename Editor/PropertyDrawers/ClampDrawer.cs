using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kutil.Editor.PropertyDrawers {

    /// <summary>
    /// restricts number input to a field
    /// </summary>
    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampDrawer : PropertyDrawer {

        public static readonly string clampClass = "kutil-clamp-value";

        ClampAttribute clampAttribute => (ClampAttribute)attribute;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            PropertyField propertyField = new PropertyField(property, preferredLabel);
            propertyField.RegisterValueChangeCallback(OnValueChanged);
            return propertyField;
        }

        private void OnValueChanged(SerializedPropertyChangeEvent evt) {
            VisualElement target = evt.currentTarget as VisualElement;
            // evt.changedProperty.serializedObject.Update();
            if (evt.changedProperty.propertyType == SerializedPropertyType.Integer) {
                int minValue = ClampAttribute.IntVal(clampAttribute.minValue);
                int maxValue = ClampAttribute.IntVal(clampAttribute.maxValue);
                
                // Debug.Log($"clamping {target?.ToStringBetter()} v:{evt.changedProperty.intValue} min:{(int)clampAttribute.minValue} max:{Mathf.FloorToInt(clampAttribute.maxValue)} rmax:{clampAttribute.maxValue}");
                evt.changedProperty.intValue = Mathf.Clamp(evt.changedProperty.intValue, minValue, maxValue);
                evt.changedProperty.serializedObject.ApplyModifiedProperties();
            } else if (evt.changedProperty.propertyType == SerializedPropertyType.Float) {
                // todo any way to make cleaner when dragging?
                // new FloatField(). or something?
                evt.changedProperty.floatValue = Mathf.Clamp(evt.changedProperty.floatValue, clampAttribute.minValue, clampAttribute.maxValue);
                evt.changedProperty.serializedObject.ApplyModifiedProperties();
                // todo Vector2, v3, v2int, v3int, rect, bounds
                // } else if (evt.changedProperty.propertyType == SerializedPropertyType.Vector2) {
                // seperate per vector?
                // } else if (evt.changedProperty.propertyType == SerializedPropertyType.Vector3) {
                // } else if (evt.changedProperty.propertyType == SerializedPropertyType.Vector2Int) {
                // } else if (evt.changedProperty.propertyType == SerializedPropertyType.Vector3Int) {
                // todo custom validation?
            } else {
                Debug.LogWarning($"Clamp Attribute {evt.currentTarget.ToString()} does not support property type {evt.changedProperty.propertyType}!");
            }
        }
    }
}