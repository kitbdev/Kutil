using System;
using UnityEditor;
using UnityEngine;

namespace Kutil {
    [CustomPropertyDrawer(typeof(FoldStartAttribute))]
    public class FoldStartDrawer : PropertyDrawer {
        // todo keep state?
        bool foldOpen = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            FoldStartAttribute foldStart = (FoldStartAttribute)attribute;
            GUIContent header = foldStart.header == null ? label : new GUIContent(foldStart.header);
            foldOpen = EditorGUI.BeginFoldoutHeaderGroup(position, foldOpen, header);
            position.height -= EditorGUIUtility.singleLineHeight;
            position.y += EditorGUIUtility.singleLineHeight;
            if (foldOpen) {
                EditorGUI.PropertyField(position, property, label);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (foldOpen) height += base.GetPropertyHeight(property, label);
            return height;
        }
    }
}