using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(AnimPlayer.AnimTrigger))]
    public class AnimPlayerDrawer : PropertyDrawer {

        Animator animator;
        private void OnEnable() {
            // animator = attribute
        }
        private void OnDisable() {
            animator = null;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!animator && property.serializedObject.targetObject is Component tcomp) {
                animator = tcomp.gameObject.GetComponent<Animator>();
            }
            if (animator) {
                // todo give dropdown of valid fields instead?
                var pnProp = property.FindPropertyRelative("_" + nameof(AnimPlayer.AnimTrigger.paramName));
                string typedParam = pnProp?.stringValue;
                if (animator.parameters.Any(p => p.name == typedParam)) {
                    label.text += "  valid param";
                } else {
                    label.text += "  invalid param, not found in animator";
                }
            } else {
                label.text += "  no animator found!";
            }
            EditorGUI.PropertyField(position, property, label, true);
            // position.height = EditorGUIUtility.singleLineHeight;
            // position.y = EditorGUIUtility.singleLineHeight;
            // property.serializedObject.Update();
            // property.Next(true);
            // int val = property.intValue;
            // property.intValue = EditorGUI.LayerField(position, label, val);
            // property.serializedObject.ApplyModifiedProperties();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label);// + EditorGUIUtility.singleLineHeight;
        }
    }
}