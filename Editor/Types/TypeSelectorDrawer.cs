using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(TypeSelector<>))]
    public class TypeSelectorDrawer : PropertyDrawer {
        string lastTypeName = null;
        bool show = false;
        bool didChange = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // SerializedProperty selNameProp = property.FindPropertyRelative(nameof(ImplementsType<int>._selectedName));
            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                SerializedProperty typeprop = property.FindPropertyRelative("_type");//nameof(TypeSelector<int>._type)
                SerializedProperty objprop = property.FindPropertyRelative("_objvalue");//nameof(TypeSelector<int>._objvalue)

                Rect typepos = position;
                typepos.height = EditorGUIUtility.singleLineHeight;
                Rect togglePos = typepos;
                togglePos.width = 40;
                using (var changeCheck = new EditorGUI.ChangeCheckScope()) {
                    show = EditorGUI.Foldout(togglePos, show, GUIContent.none);
                    objprop.isExpanded = show;
                    // object beforeObj = typeprop.managedReferenceValue;
                    EditorGUI.PropertyField(typepos, typeprop, label, false);
                    if (changeCheck.changed) didChange = true;
                    if (lastTypeName == null || didChange) {
                        // Debug.Log("updated");
                        if (ReflectionHelper.TryGetValue<System.Object>(property.serializedObject.targetObject, objprop.propertyPath, out var typename)) {
                            // Debug.Log("got " + typeprop.type);
                            string nTypeName = typename?.GetType()?.Name ?? "unknown";
                            lastTypeName = nTypeName;
                            if (lastTypeName != nTypeName) {
                                // keep checking until it updates
                                didChange = false;
                            }
                        }
                        // .SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
                    }
                }
                if (show) {
                    EditorGUI.indentLevel += 1;
                    position.y += EditorGUIUtility.singleLineHeight;
                    GUIContent objContent = new GUIContent(lastTypeName ?? "unknown");
                    EditorGUI.PropertyField(position, objprop, objContent, true);
                    if (!objprop.isExpanded) {
                        show = false;
                    }
                    // if (EditorGUI.EndChangeCheck()) {
                    // if (ReflectionHelper.TryGetValue<TypeSelector<int>>(property.serializedObject.targetObject, property.propertyPath, out var typeSelector)) {

                    // }
                    // }

                    EditorGUI.indentLevel -= 1;
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty objprop = property.FindPropertyRelative("_objvalue");//nameof(TypeSelector<int>._objvalue)
            return EditorGUIUtility.singleLineHeight * (show ? 1 : 0)
                + EditorGUI.GetPropertyHeight(objprop, objprop.isExpanded);
            // + base.GetPropertyHeight(objprop, label); // obj height
        }
        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        //     VisualElement root = new VisualElement();
        //     // // Label label = new Label(property.displayName);
        //     // SerializedProperty selNameProp = property.FindPropertyRelative(nameof(ImplementsType<int>._selectedName));
        //     // PropertyField choicesField = new PropertyField(
        //     //     selNameProp, property.displayName);
        //     // choicesField.BindProperty(selNameProp);
        //     // root.Add(choicesField);
        //     // // label.Add(choicesField);
        //     // // root.Add(label);
        //     return root;
        // }
    }
}