using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    [CustomPropertyDrawer(typeof(AddButtonAttribute))]
    public class AddButtonDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.name = "AddButtonDrawer";
            AddButtonAttribute btnData = (AddButtonAttribute)attribute;
            FlexDirection fd = FlexDirection.Column;

            var propField = new PropertyField(property);
            if (btnData.buttonLayout != AddButtonAttribute.ButtonLayout.REPLACE) {
                container.Add(propField);
            }

            Button btn = new Button();
            btn.text = btnData.buttonLabel ?? btnData.buttonMethodName;
            btn.name = btn.text + " Button";
            btn.clicked += () => CallButtonMethod(btnData, property);
            container.Add(btn);

            if (btnData.buttonLayout == AddButtonAttribute.ButtonLayout.BEFORE
            || btnData.buttonLayout == AddButtonAttribute.ButtonLayout.LEFT) {
                btn.SendToBack();
            }
            if (btnData.buttonLayout == AddButtonAttribute.ButtonLayout.LEFT
            || btnData.buttonLayout == AddButtonAttribute.ButtonLayout.RIGHT) {
                // btn.style.width = $"{btnData.btnWidth}px";
                fd = FlexDirection.Row;
                btn.style.width = new StyleLength(new Length(btnData.btnWidth, btnData.lengthUnit));
            }

            container.style.flexDirection = new StyleEnum<FlexDirection>(fd);
            container.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);

            return container;
        }


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
            // use reflection to support arrays and nesting too
            string path = property.propertyPath.Replace(property.name, butAtt.buttonMethodName);
            Object[] targetObjects = property.serializedObject.targetObjects;
            foreach (var targetObj in targetObjects) {
                ReflectionHelper.TryCallMethod(targetObj, path, butAtt.parameters);
                if (!butAtt.allowMultipleCalls){
                    break;
                }
            }
        }

    }
}