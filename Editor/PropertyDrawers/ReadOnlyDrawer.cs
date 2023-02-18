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

        string readonlyClass = "kutil-readonly-foldout";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.name = "ReadOnlyDrawer";
            var propField = new PropertyField(property);
            // disabling directly disables opening foldouts
            // propField.SetEnabled(false);
            container.Add(propField);

            // propField.Bind(property.serializedObject);

            // set style temporarily so theres no delay
            propField.SetEnabled(false);
            // propField.AddToClassList("unity-disabled");

            // var listView = propField.Query<ListView>().ToList();
            // Debug.Log(listView.Count());//0

            // After prop field has binded
            // todo custom ReadOnlyProperty VE OnGeoChange instead
            _ = propField.schedule.Execute(() => {
                PropDisable(propField);
                if (propField.Q<Foldout>() != null) {
                    propField.SetEnabled(true);
                    // Debug.Log("has foldout");
                }
                // propField.RemoveFromClassList("unity-disabled");
            });
            return container;
            // return base.CreatePropertyGUI(property);
        }

        // disable all visual elements without a Foldout in them
        void PropDisable(PropertyField propField) {

            // disable list size field, add/remove buttons, and rearranging
            ListView listView = propField.Children().OfType<ListView>().FirstOrDefault();
            if (listView != null) {
                listView.reorderable = false;
                listView.showAddRemoveFooter = false;
                var listSize = listView.Query("unity-list-view__size-field").Last();
                listSize.SetEnabled(false);
            }

            // disable foldout label and register callback for references
            // todo multiple foldouts on one prop, at the same level?
            // todo dont go into other propertfields, recall this method
            Foldout foldout = propField.Q<Foldout>();
            if (foldout != null) {
                if (foldout.ClassListContains(readonlyClass)) {
                    // already disabled
                    // Debug.Log($"{foldoutLabel.text} is already disabled! {foldout.bindingPath}");
                    return;
                }
                // if (listView != null) {
                //     Debug.Log("disabling foldout on list " + listView.name);
                // } else {
                //     Debug.Log("disabling foldout " + foldout.name);
                // }
                foldout.AddToClassList(readonlyClass);

                Label foldoutLabel = foldout.Q<Label>();
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
            ScrollView scrollView = foldoutContent.Children().OfType<ScrollView>().FirstOrDefault();

            // Debug.Log($"disabling children of {foldout.name} sv:{scrollView != null} " + foldoutContent.Children().OfType<PropertyField>().ToStringFull(pf => pf.name, true, true) + " " + (scrollView != null ? (scrollView.Q("unity-content-container").Children().OfType<PropertyField>().ToStringFull(pf => pf.Children().FirstOrDefault()?.name ?? "??", true, true)) : "n/a"));

            foldoutContent.Children().OfType<PropertyField>().ForEach(pf => PropDisable(pf));

            if (scrollView != null) {
                IEnumerable<PropertyField> scrollProps = scrollView.Q("unity-content-container").Children().OfType<PropertyField>();
                if (scrollProps.Count() > 0) {
                    scrollProps.ForEach(pf => PropDisable(pf));
                } else {
                    // delay to detect recursive properties
                    _ = foldout.schedule.Execute(() => {
                        scrollView.Q("unity-content-container").Children().OfType<PropertyField>().ForEach(pf => PropDisable(pf));
                    });
                }
            }
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