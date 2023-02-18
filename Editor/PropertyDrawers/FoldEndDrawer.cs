using System;
using UnityEditor;
using UnityEngine;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// Ends a foldout section.
    /// Must be paired with a FoldStart!
    /// </summary>
    [CustomPropertyDrawer(typeof(FoldEndAttribute))]
    public class FoldEndDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndFoldoutHeaderGroup();
        }
    }
}