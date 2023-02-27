using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();
            var field = new LayerField();
            field.label = property.displayName;
            // field.BindProperty(property);
            field.bindingPath = property.propertyPath;
            root.Add(field);
            return root;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
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