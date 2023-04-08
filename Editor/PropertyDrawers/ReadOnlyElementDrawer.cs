using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil.Editor.PropertyDrawers {
    [CustomPropertyDrawer(typeof(ReadOnlyElementAttribute))]
    public class ReadOnlyElementDrawer : PropertyDrawer {

        public static readonly string readonlyClass = "kutil-readonly-by-element";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.name = "ReadOnlyElementDrawer";
            container.AddToClassList(readonlyClass);
            var propField = new PropertyField(property);
            container.Add(propField);
            propField.RegisterCallback<GeometryChangedEvent>(ce => {
                ReadOnlyDrawer.PropDisable(propField);
            });
            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // for some reason ignores other property drawers
            // x use uitk instead - toxdo need to somehow work on other property drawers that already exist
            // not https://forum.unity.com/threads/drawing-a-field-using-multiple-property-drawers.479377/ < uses attr only
            // also for arrays, this operates on each element of the array, not the array itself

            // var previousGUIState = GUI.enabled;
            // GUI.enabled = false;
            EditorGUI.BeginDisabledGroup(true);
            // new EditorGUI.DisabledGroupScope()
            EditorGUI.PropertyField(position, property, label, property.isExpanded);
            // foreach (var drawer in allDrawers) {
            //     allDrawers
            // }
            EditorGUI.EndDisabledGroup();
            // GUI.enabled = previousGUIState;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }

    }
}