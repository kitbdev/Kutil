using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerDrawer : PropertyDrawer {

        public static readonly string layerClass = "kutil-layer";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Debug.Log("layer drawer p");
            // var root = new VisualElement();
            // root.AddToClassList(layerClass);
            var field = new LayerField();
            field.AddToClassList(layerClass);
            field.AddToClassList(LayerField.alignedFieldUssClassName);
            field.label = property.displayName;
            field.bindingPath = property.FindPropertyRelative("layerValue").propertyPath;
            // root.Add(field);
            return field;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Debug.Log("layer drawer g");
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty layerProp = property.FindPropertyRelative(nameof(Layer.layerValue));
            int val = layerProp.intValue;
            int newval = EditorGUI.LayerField(position, label, val);
            if (val != newval) {
                layerProp.intValue = newval;
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndProperty();
        }
    }
}