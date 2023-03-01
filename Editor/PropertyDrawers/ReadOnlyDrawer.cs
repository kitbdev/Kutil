using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {

        public static readonly string readonlyClass = "kutil-readonly";
        public static readonly string readonlyFoldoutClass = "kutil-readonly-foldout";
        public static readonly string readonlyPropertyClass = "kutil-readonly-property";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // var readonlyDrawer = new ReadOnlyPropertyField(property);
            // return readonlyDrawer;
            var container = new VisualElement();
            container.name = "ReadOnlyDrawer";
            container.AddToClassList(readonlyClass);
            var propField = new PropertyField(property);
            container.Add(propField);
            propField.RegisterCallback<GeometryChangedEvent>(ce => {
                PropDisable(propField);
            });
            return container;
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


        // disable all visual elements without a Foldout in them
        public static void PropDisable(PropertyField propField) {

            propField.AddToClassList(readonlyPropertyClass);

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
                if (foldout.ClassListContains(readonlyFoldoutClass)) {
                    // already disabled
                    // Debug.Log($"{foldoutLabel.text} is already disabled! {foldout.bindingPath}");
                    return;
                }
                // if (listView != null) {
                //     Debug.Log("disabling foldout on list " + listView.name);
                // } else {
                //     Debug.Log("disabling foldout " + foldout.name);
                // }
                foldout.AddToClassList(readonlyFoldoutClass);

                Label foldoutLabel = foldout.Q<Label>();
                foldoutLabel.SetEnabled(false);

                PropDisableChildren(foldout);

                Toggle toggle = foldout.Q<Toggle>();
                toggle.RegisterCallback<ClickEvent, Foldout>(ToggleClickEventHandler, foldout);
                return;
            }

            propField.SetEnabled(false);
        }
        static void PropDisableChildren(Foldout foldout) {
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
                    foldout.RegisterCallback<GeometryChangedEvent>((ce) => {
                        // _ = foldout.schedule.Execute(() => {
                        scrollView.Q("unity-content-container").Children().OfType<PropertyField>().ForEach(pf => PropDisable(pf));
                    });
                }
            }
        }
        static void ToggleClickEventHandler(ClickEvent clickEvent, Foldout foldout) {
            // in case this creates new property drawers, we need to make sure they are still disabled
            if (foldout.value) {
                // delay until after new fields are created
                foldout.RegisterCallback<GeometryChangedEvent>((ce) => {
                    // _ = foldout.schedule.Execute(() => {
                    PropDisableChildren(foldout);
                });
            }
        }
    }
    // /// <summary>
    // /// A PropertyField that can still open foldouts
    // /// </summary>
    // public class ReadOnlyPropertyField : VisualElement {

    //     // doesnt work if top level is a list

    //     PropertyField propertyField;
    //     static readonly string readonlyClass = "kutil-readonly-foldout";

    //     public ReadOnlyPropertyField(SerializedProperty property) {
    //         name = "ReadOnlyField " + name;
    //         propertyField = new PropertyField(property);
    //         Add(propertyField);
    //         this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    //     }

    //     private void OnGeometryChanged(GeometryChangedEvent evt) {
    //         // disabling directly disables opening foldouts, so only disable properties wihtout a foldout
    //         PropDisable(propertyField);
    //     }

    // }

}