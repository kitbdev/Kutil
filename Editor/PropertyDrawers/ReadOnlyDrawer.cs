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
            container.name = "ReadOnlyDrawer";
            var propField = new PropertyField(property);
            // disabling directly disables opening foldouts
            // propField.SetEnabled(false);

            container.Add(propField);

            // After prop field has binded
            _ = propField.schedule.Execute(() => {
                // disable all visual elements without a Foldout in them
                PropDisable(propField);

            });
            return container;
            // return base.CreatePropertyGUI(property);
        }

        void PropDisable(PropertyField propField) {

            // disable list size field, add/remove buttons, and rearranging
            ListView listView = propField.Query().Children<ListView>();
            if (listView != null) {
                listView.reorderable = false;
                listView.showAddRemoveFooter = false;
                var listSize = listView.Query("unity-list-view__size-field").Last();
                listSize.SetEnabled(false);
            }

            // disable foldout label and register callback for references
            Foldout foldout = propField.Q<Foldout>();
            if (foldout != null) {
                Label foldoutLabel = foldout.Q<Label>();
                if (!foldoutLabel.enabledSelf) {
                    // already disabled
                    // Debug.Log($"{foldoutLabel.text} is already disabled! {foldout.bindingPath}");
                    return;
                }
                foldoutLabel.SetEnabled(false);

                PropDisableChildren(foldout);

                Toggle toggle = foldout.Q<Toggle>();
                toggle.RegisterCallback<ClickEvent, Foldout>(ToggleClickEventHandler, foldout);
                return;
            }

            propField.SetEnabled(false);
        }
        void PropDisableChildren(Foldout foldout) {
            VisualElement foldoutContent = foldout.Q("unity-content");
            foldoutContent.Query().Children<PropertyField>().ForEach(pf => PropDisable(pf));
        }
        void ToggleClickEventHandler(ClickEvent clickEvent, Foldout foldout) {
            // in case this creates new property drawers, we need to make sure they are still disabled
            if (foldout.value) {
                // delay until after new fields are created
                _ = foldout.schedule.Execute(() => {
                    PropDisableChildren(foldout);
                });
            }
        }

        // void ForEachChildRecursively(VisualElement root, System.Action<VisualElement> action, System.Func<VisualElement, bool> searchChildren = null) {
        //     List<VisualElement> searched = new();
        //     Queue<VisualElement> frontier = new();
        //     frontier.Enqueue(root);
        //     while (frontier.Count > 0) {
        //         var cur = frontier.Dequeue();
        //         searched.Add(cur);
        //         action?.Invoke(cur);
        //         if (searchChildren != null && !searchChildren.Invoke(cur)) {
        //             continue;
        //         }
        //         foreach (var child in cur.Children()) {
        //             if (!searched.Contains(child) && !frontier.Contains(child)) {
        //                 frontier.Enqueue(child);
        //             }
        //         }
        //     }
        // }

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