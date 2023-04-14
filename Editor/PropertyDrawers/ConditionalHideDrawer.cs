using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    /// Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
    /// Modified by: Sebastian Lague and Kit
    /// using a decorator to properly handle top level arrays 
    /// <summary>
    /// Conditionally hides a field based on a bool or enum field
    /// </summary>
#if UNITY_2022_2_OR_NEWER
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHideDrawer : ExtendedDecoratorDrawer {

        public static readonly string conditionalHideClass = "kutil-conditional-hide";
        public static readonly string conditionalHideDecoratorClass = "kutil-conditional-hide-decorator";

        ConditionalHideAttribute conditionalHide => (ConditionalHideAttribute)attribute;

        protected override string decoratorName => "ConditionalHide";
        protected override string decoratorClass => conditionalHideDecoratorClass;

        public override bool registerUpdateCall => true;

        protected override void Setup() {
            base.Setup();
            // Debug.Log("Setting up c hide decorator...");
            propertyField.AddToClassList(conditionalHideClass);

            UpdateField();
        }

        protected override void OnUpdate(SerializedPropertyChangeEvent ce) => UpdateField();
        void UpdateField() {
            if (!HasSerializedProperty()) return;
            // Debug.Log($"update chide {serializedProperty?.propertyPath??"null"}");

            // Debug.Log($"Updating field! on {serializedProperty.propertyPath} o:{serializedProperty.serializedObject.targetObject.name}");
            bool enabled = GetConditionalHideAttributeResult(conditionalHide, serializedProperty) == conditionalHide.showIfTrue;
            if (conditionalHide.readonlyInstead) {
                // propertyField.SetEnabled(enabled);
                ReadOnlyDrawer.MakeReadOnly(propertyField, enabled);
            } else {
                propertyField.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            }
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

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property) {
            return GetPropertyConditionalValue(property, condHAtt.conditionalSourceField, condHAtt.enumIndices);
        }

        public static bool GetPropertyConditionalValue(SerializedProperty property, string conditionalSourceField, int[] enumIndices = null) {
            if (property == null || conditionalSourceField == null) {
                Debug.LogError("GetPropertyConditionalValue failed, null sourceProp or sourcefield!");
                return false;
            }
            //? use attribute interface instead? maybe struct?

            // Get the full relative property path of the sourcefield so we can have nested hiding.
            // string conditionPath = propertyPath.Replace(property.name, condHAtt.conditionalSourceField);
            SerializedProperty sourcePropertyValue = property.GetNeighborProperty(conditionalSourceField);
            // Debug.Log($"cond hide {property.propertyPath} {sourcePropertyValue?.name ?? "no spv"} spvp:{sourcePropertyValue?.propertyPath}");
            if (sourcePropertyValue != null) {
                return CheckPropertyValue(sourcePropertyValue, enumIndices);
            }

            string path = property.GetPathRelative(conditionalSourceField);
            // use reflection instead, should support arrays and nesting
            Object targetObject = property.serializedObject.targetObject;
            if (ReflectionHelper.TryGetValue<bool>(targetObject, path, out var value)) {
                return value;
            } else if (ReflectionHelper.TryGetValue<System.Enum>(targetObject, path, out var evalue)) {
                if (enumIndices == null || enumIndices.Length == 0) {
                    return false;
                }
                if (System.Enum.GetUnderlyingType(typeof(System.Enum)) != typeof(int)) {
                    return true;
                }
                int eintval = (int)System.Convert.ChangeType(evalue, typeof(int));
                return enumIndices.Contains(eintval);
            } else {
                Debug.LogWarning($"GetPropertyConditionalValue failed cannot find property or reflection data of a valid type! path:{path} o:{property.propertyPath} r:{conditionalSourceField} ");
            }
            return true;
        }

        static bool CheckPropertyValue(SerializedProperty sourcePropertyValue, int[] enumIndices = null) {
            if (sourcePropertyValue == null) {
                Debug.LogError("CheckPropertyValue failed, null sourceProp!");
                return false;
            }
            //Note: add others for custom handling if desired
            switch (sourcePropertyValue.propertyType) {
                case SerializedPropertyType.Boolean:
                    return sourcePropertyValue.boolValue;
                case SerializedPropertyType.Enum:
                    if (enumIndices == null || enumIndices.Length == 0) {
                        return false;
                    }
                    return enumIndices.Contains(sourcePropertyValue.enumValueIndex);
                default:
                    Debug.LogError($"CheckPropertyValue Property {sourcePropertyValue.propertyPath} Data type [{sourcePropertyValue.propertyType}] is currently not supported");
                    return true;
            }
        }
    }
}