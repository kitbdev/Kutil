using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            var propField = new PropertyField(property);
            propField.SetEnabled(false);
            container.Add(propField);
            // container.Add(new Label("test"));
            //https://forum.unity.com/threads/disable-size-attribute-for-propertydrawer.462696/
            _ = propField.schedule.Execute(() => {
                // Get size field of array
                var array = container.parent;
                // todo check if actually an array
                // array.Q<IntegerField>("size?")
                // if (array)
                // IntegerField sizeField = array.Q<IntegerField>();

                // Disallow changing array size in inspector
                // array.SetEnabled(false);
            });
            return container;
            // return base.CreatePropertyGUI(property);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // for some reason ignores other property drawers
            // todo need to somehow work on other property drawers that already exist
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