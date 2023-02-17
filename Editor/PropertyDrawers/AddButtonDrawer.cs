using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    // use with note, button, ?
    /// <summary>
    /// Adds a button next to the property
    /// </summary>
    [CustomPropertyDrawer(typeof(AddButtonAttribute))]
    public class AddButtonDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // var container = new VisualElement();
            // container.name = "AddButtonDrawer";
            AddButtonAttribute btnData = (AddButtonAttribute)attribute;

            var propField = new PropertyField(property);

            Button btn = new Button();
            btn.text = btnData.buttonLabel ?? btnData.buttonMethodName;
            btn.name = btn.text + " Button";
            btn.clicked += () => CallButtonMethod(btnData, property);

            // var stylelen = new StyleLength(new Length(btnData.btnWidth, btnData.lengthUnit));

            VisualElement relPropContainer = RelativePropertyDrawer.CreateRelPropertyGUI(propField, btn, btnData.buttonLayout, btnData.btnWeight);
            relPropContainer.name = "AddButton" + relPropContainer.name;
            return relPropContainer;
            // container.Add(relPropContainer);

            // return container;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            AddButtonAttribute butAtt = (AddButtonAttribute)attribute;
            // for some reason label gets cleared after get height
            var proplabel = new GUIContent(label);
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            float propHeight = EditorGUI.GetPropertyHeight(property, proplabel);

            RelativePropertyDrawer.OnGUI(position, propRect => {
                EditorGUI.PropertyField(propRect, property, proplabel, true);
            }, btnRect => DrawButton(btnRect, butAtt, property),
            butAtt.buttonLayout, propHeight, buttonHeight, butAtt.btnWidth);

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
            return RelativePropertyDrawer.GetPropertyHeight(property, label, butAtt.buttonLayout, buttonHeight);
        }

        void CallButtonMethod(AddButtonAttribute butAtt, SerializedProperty property) {
            // SerializedProperty sourcePropertyValue = null;
            // use reflection to support arrays and nesting too
            string path = property.propertyPath.Replace(property.name, butAtt.buttonMethodName);
            Object[] targetObjects = property.serializedObject.targetObjects;
            foreach (var targetObj in targetObjects) {
                ReflectionHelper.TryCallMethod(targetObj, path, butAtt.parameters);
                if (!butAtt.allowMultipleCalls) {
                    break;
                }
            }
        }

    }
}