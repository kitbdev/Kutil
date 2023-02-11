using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(AddButtonAttribute))]
    public class AddButtonDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AddButtonAttribute butAtt = (AddButtonAttribute)attribute;
            // for some reason label gets cleared after get height
            var proplabel = new GUIContent(label);
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            float propHeight = EditorGUI.GetPropertyHeight(property, proplabel);
            Rect buttonRect = position;
            buttonRect.height = buttonHeight;
            Rect propRect = position;
            propRect.height = propHeight;

            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.REPLACE) {
                DrawButton(buttonRect, butAtt, property);
                return;
            }
            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.BEFORE) {
                propRect.y += buttonHeight;
                DrawButton(buttonRect, butAtt, property);
            }
            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.LEFT
            || butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.RIGHT) {
                // small button and move prop
                float btnWidth = butAtt.btnWidth;
                float spacing = 5;
                buttonRect.width = btnWidth;
                propRect.width -= btnWidth - spacing;
                if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.LEFT) {
                    propRect.x = btnWidth + spacing;
                    EditorGUI.indentLevel += 1;
                    DrawButton(buttonRect, butAtt, property);
                }
                if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.RIGHT) {
                    buttonRect.x = propRect.width + spacing;
                    DrawButton(buttonRect, butAtt, property);
                }
            }
            EditorGUI.PropertyField(propRect, property, proplabel, true);
            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.LEFT) {
                EditorGUI.indentLevel -= 1;
            }
            // EditorGUI.PropertyField(propRect, property, label, true);
            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.AFTER) {
                buttonRect.y += buttonHeight;
                DrawButton(buttonRect, butAtt, property);
            }
        }
        void DrawButton(Rect buttonRect, AddButtonAttribute butAtt, SerializedProperty property) {
            string text = butAtt.buttonLabel ?? butAtt.buttonMethodName;
            if (GUI.Button(buttonRect, text)) {
                CallButtonMethod(butAtt, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            AddButtonAttribute butAtt = (AddButtonAttribute)attribute;
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            float propHeight = EditorGUI.GetPropertyHeight(property, label);

            if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.NONE) {
                return propHeight;
            } else if (butAtt.buttonLayout == AddButtonAttribute.ButtonLayout.REPLACE) {
                return buttonHeight;
            }
            // before or after
            float height = buttonHeight + propHeight;
            return height;
        }

        void CallButtonMethod(AddButtonAttribute butAtt, SerializedProperty property) {
            // SerializedProperty sourcePropertyValue = null;
            // use reflection should support arrays and nesting too
            string path = property.propertyPath.Replace(property.name, butAtt.buttonMethodName);
            Object targetObject = property.serializedObject.targetObject;
            ReflectionHelper.TryCallMethod(targetObject, path, butAtt.parameters);
            // todo this doest work when selecting multiple!
        }

    }
}