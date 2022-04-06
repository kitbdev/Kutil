using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();
            var field = new LayerField();
            field.BindProperty(property);
            root.Add(field);
            return root;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty layerProp = property.FindPropertyRelative(nameof(Layer.layerValue));
            int val = layerProp.intValue;
            int newval = EditorGUI.LayerField(position, label, val);
            if (val != newval) {
                layerProp.intValue = newval;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}