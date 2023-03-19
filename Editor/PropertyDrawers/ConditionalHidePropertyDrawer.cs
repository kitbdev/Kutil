﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
    /// Modified by: Sebastian Lague and Kit
    /// using a decorator to properly handle top level arrays 
    /// <summary>
    /// Conditionally hides a field based on a bool or enum field
    /// </summary>
#if UNITY_2022_2_OR_NEWER
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : DecoratorDrawer {

        public static readonly string conditionalHideClass = "kutil-conditional-hide";
        public static readonly string conditionalHideDecoratorClass = "kutil-conditional-hide-decorator";

        ConditionalHideAttribute conditionalHide => (ConditionalHideAttribute)attribute;

        VisualElement decorator;
        PropertyField propertyField;
        SerializedProperty serializedProperty;
        InspectorElement inspectorElement;

        public override VisualElement CreatePropertyGUI() {
            decorator = new VisualElement();
            decorator.AddToClassList(conditionalHideDecoratorClass);
            decorator.RegisterCallback<GeometryChangedEvent>(SetupField);
            return decorator;
        }

        private void SetupField(GeometryChangedEvent evt) {
            decorator.UnregisterCallback<GeometryChangedEvent>(SetupField);
            // Debug.Log("Setting up c hide decorator...");
            // get propertyfield
            propertyField = decorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"{GetType().Name} decorator failed to find containing property!");
                return;
            }
            propertyField.AddToClassList(conditionalHideClass);

            // get serialized property
            serializedProperty = SerializedPropertyExtensions.GetBindedPropertyFromDecorator(decorator);
            if (serializedProperty == null) {
                Debug.LogError($"{GetType().Name} decorator cannot find serialized property!, cannot bind.");
                return;
            }

            // get inspector element to register an onvalidate callback
            inspectorElement = propertyField.GetFirstAncestorOfType<InspectorElement>();
            if (inspectorElement == null) {
                Debug.LogError($"Conditional Hide - inspectorElement null!");
                return;
            }
            // this properly responds to all changes
            inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            decorator.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        void OnDetach(DetachFromPanelEvent detachFromPanelEvent) {
            inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
        }
        void OnUpdate(SerializedPropertyChangeEvent ce) => UpdateField();
        void UpdateField() {
            // Debug.Log($"Updating field! on {serializedProperty.propertyPath} o:{serializedProperty.serializedObject.targetObject.name}");
            bool enabled = GetConditionalHideAttributeResult(conditionalHide, serializedProperty) == conditionalHide.showIfTrue;
            propertyField.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

#else
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;

            bool enabled = GetConditionalHideAttributeResult(condHAtt, property) == condHAtt.showIfTrue;

            if (enabled) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property) == condHAtt.showIfTrue;

            if (enabled) {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            //We want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;

        }
#endif

        bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property) {
            SerializedProperty sourcePropertyValue = null;

            // Get the full relative property path of the sourcefield so we can have nested hiding.
            // Use old method when dealing with arrays
            if (!property.isArray) {
                // returns the property path of the property we want to apply the attribute to
                string propertyPath = property.propertyPath;
                // changes the path to the conditionalsource property path
                string conditionPath = propertyPath.Replace(property.name, condHAtt.conditionalSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            } else {
                // original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);
            }

            // if the find failed->fall back to the old system
            if (sourcePropertyValue == null) {
                // original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);
            }
            if (sourcePropertyValue != null) {
                return CheckPropertyType(condHAtt, sourcePropertyValue);
            } else {
                // use reflection instead, should support arrays and nesting
                string path = property.propertyPath.Replace(property.name, condHAtt.conditionalSourceField);
                Object targetObject = property.serializedObject.targetObject;
                if (ReflectionHelper.TryGetValue<bool>(targetObject, path, out var value)) {
                    return value;
                } else if (ReflectionHelper.TryGetValue<System.Enum>(targetObject, path, out var evalue)) {
                    if (condHAtt.enumIndices == null || condHAtt.enumIndices.Length == 0) {
                        return false;
                    }
                    if (System.Enum.GetUnderlyingType(typeof(System.Enum)) != typeof(int)) {
                        return true;
                    }
                    // todo test this
                    int eintval = (int)System.Convert.ChangeType(evalue, typeof(int));
                    return condHAtt.enumIndices.Contains(eintval);
                }
            }
            return true;
        }

        bool CheckPropertyType(ConditionalHideAttribute condHAtt, SerializedProperty sourcePropertyValue) {
            //Note: add others for custom handling if desired
            switch (sourcePropertyValue.propertyType) {
                case SerializedPropertyType.Boolean:
                    return sourcePropertyValue.boolValue;
                case SerializedPropertyType.Enum:
                    if (condHAtt.enumIndices == null || condHAtt.enumIndices.Length == 0) {
                        return false;
                    }
                    return condHAtt.enumIndices.Contains(sourcePropertyValue.enumValueIndex);
                default:
                    Debug.LogError($"Data type of the property used for conditional hiding [{sourcePropertyValue.propertyType}] is currently not supported");
                    return true;
            }
        }
    }
}