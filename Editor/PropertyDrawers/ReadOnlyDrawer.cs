using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : DecoratorDrawer {

        public static readonly string readonlyClass = "kutil-readonly";
        public static readonly string readonlyFoldoutClass = "kutil-readonly-foldout";
        public static readonly string readonlyPropertyClass = "kutil-readonly-property";

        public override VisualElement CreatePropertyGUI() {
            var root = new VisualElement();
            root.name = "ReadOnly";
            root.AddToClassList(readonlyClass);

            root.RegisterCallback<GeometryChangedEvent>(ce => {
                PropertyField propertyField = root.GetFirstAncestorOfType<PropertyField>();
                if (propertyField == null) {
                    Debug.LogError($"ReadOnly failed to find property! {root.name}");
                    return;
                }
                PropDisable(propertyField);
            });
            return root;
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
}