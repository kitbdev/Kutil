using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil.PropertyDrawers {
    // [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryDrawer : PropertyDrawer {
        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        //     // GUI.Label(position, "SerializableDictionaryDrawer");
        //     // EditorGUI.

        //     property.serializedObject.Update();
        //     property.isExpanded =
        //     EditorGUI.PropertyField(position, property.FindPropertyRelative("values"), label, true);
        //     property.serializedObject.ApplyModifiedProperties();
        //     // ListView
        // }
        // public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        //     return EditorGUIUtility.singleLineHeight *
        //         (true ? property.FindPropertyRelative("values").arraySize : 1);
        // }
    }

}
