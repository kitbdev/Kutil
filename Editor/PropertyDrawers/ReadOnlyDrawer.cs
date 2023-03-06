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

        public static readonly string readonlyDecoratorClass = "kutil-readonly";
        public static readonly string readonlyFoldoutClass = "kutil-readonly-foldout";
        public static readonly string readonlyPropertyClass = "kutil-readonly-property";
        public static readonly string readonlyListViewClass = "kutil-readonly-listview";

        // public static readonly string unityListViewSizeFieldName = "unity-list-view__size-field";
        // public static readonly string unityFoldoutContentName = "unity-content";
        // public static readonly string unityScrollViewContentName = "unity-content-container";
        public static string unityListViewSizeFieldClass => ListView.arraySizeFieldUssClassName;
        public static string unityFoldoutContentClass => Foldout.contentUssClassName;
        public static string unityScrollViewContentClass => ScrollView.contentUssClassName;

        VisualElement readOnlyDecorator;

        public override VisualElement CreatePropertyGUI() {
            readOnlyDecorator = new VisualElement();
            readOnlyDecorator.name = "ReadOnly";
            readOnlyDecorator.AddToClassList(readonlyDecoratorClass);

            readOnlyDecorator.RegisterCallback<GeometryChangedEvent>(OnDecoratorGeometryChanged);
            return readOnlyDecorator;
        }

        private void OnDecoratorGeometryChanged(GeometryChangedEvent changedEvent) {
            // only need to do once
            readOnlyDecorator.UnregisterCallback<GeometryChangedEvent>(OnDecoratorGeometryChanged);
            // use a changed event so we can access other VisualElements
            PropertyField propertyField = readOnlyDecorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"ReadOnly failed to find containing property! {readOnlyDecorator.name}");
                return;
            }
            //PropDisable(propertyField);
            PropDisableNew(propertyField);
        }

        public static void DisableField(VisualElement field) {
            if (field == null) {
                Debug.LogError($"readonly DisableField null field!");
                return;
            }

        }
        public static void PropDisableNew(PropertyField propField) {
            // dont need to not disable foldouts anymore...
            propField.SetEnabled(false);
            // maybe still remove list stuff?
        }
        /// <summary>
        /// disable all PropertiyFields and Lists without a Foldout in them.
        /// simply disabling the root will disallow foldouts from opening.
        /// </summary>
        /// <param name="propField"></param>
        // todo needs to be a property field? why not any VE?
        public static void PropDisable(PropertyField propField) {
            if (propField == null) {
                Debug.LogError($"readonly PropDisable null propField!");
                return;
            }
            if (propField.ClassListContains(readonlyPropertyClass)) {
                return;
            }

            propField.AddToClassList(readonlyPropertyClass);

            // disable all child ListViews size field, add/remove buttons, and reorderable handles
            //? what if list is not top level?
            List<ListView> listViews = propField.Children().OfType<ListView>().ToList();
            if (listViews != null && listViews.Count > 0) {
                foreach (var listView in listViews) {
                    if (listView.ClassListContains(readonlyListViewClass)) {
                        continue; // already diabled
                    }
                    listView.AddToClassList(readonlyListViewClass);
                    listView.reorderable = false;
                    listView.showAddRemoveFooter = false;
                    var listSize = listView.Query(null, unityListViewSizeFieldClass).Last();
                    if (listSize == null) {
                        // ? could this happen if nonreorderable is true?
                        Debug.LogError($"readonly {propField.name} listview {listView.name} has no listsizefield!");
                        continue;
                    }
                    listSize.SetEnabled(false);
                }
            }

            // disable foldouts label and register callback for references
            List<Foldout> foldouts = propField.Query<Foldout>().ToList();
            // ? ignore children of other properties, as this method will be called there anyway?
            if (foldouts != null && foldouts.Count > 0) {
                foreach (var foldout in foldouts) {
                    if (foldout.ClassListContains(readonlyFoldoutClass)) {
                        // already disabled
                        // Debug.Log($"{foldoutLabel.text} is already disabled! {foldout.bindingPath}");
                        continue;
                    }
                    // if (listView != null) {
                    //     Debug.Log("disabling foldout on list " + listView.name);
                    // } else {
                    //     Debug.Log("disabling foldout " + foldout.name);
                    // }
                    foldout.AddToClassList(readonlyFoldoutClass);

                    Label foldoutLabel = foldout.Q<Label>();
                    if (propField == null) {
                        Debug.LogError($"readonly PropDisable foldoutLabel missing {propField.name} {foldout.name}!");
                        continue;
                    }
                    foldoutLabel.SetEnabled(false);

                    // recursively disable properties on the foldout
                    PropDisableChildren(foldout);

                    Toggle toggle = foldout.Q<Toggle>();
                    if (toggle == null) {
                        Debug.LogError($"readonly PropDisable toggle missing {propField.name} {foldout.name}!");
                        continue;
                    }
                    toggle.RegisterCallback<ClickEvent, Foldout>(FoldoutToggleClickEventHandler, foldout);
                }
                return;
            }

            // no child foldouts, disable the property
            propField.SetEnabled(false);
        }
        /// <summary>
        /// disable all of the property children of this foldout
        /// </summary>
        /// <param name="foldout"></param>
        static void PropDisableChildren(Foldout foldout) {
            if (foldout == null) {
                Debug.LogError($"readonly PropDisableChildren null foldout!");
                return;
            }
            VisualElement foldoutContent = foldout.Q(null, unityFoldoutContentClass);
            if (foldoutContent == null) {
                Debug.LogError($"readonly PropDisableChildren invalid foldout no foldoutContent on foldout {foldout.name}");
                return;
            }

            // Debug.Log($"disabling children of {foldout.name} sv:{scrollView != null} "
            // + foldoutContent.Children().OfType<PropertyField>().ToStringFull(pf => pf.name, true, true) + 
            //" " + (scrollView != null ? (scrollView.Q("unity-content-container").Children().OfType<PropertyField>().ToStringFull(pf => pf.Children().FirstOrDefault()?.name ?? "??", true, true)) : "n/a"));

            // disable properties in the foldout
            // todo not top level only?
            foldoutContent.Children().OfType<PropertyField>().ForEach(pf => PropDisable(pf));

            ScrollView scrollView = foldoutContent.Children().OfType<ScrollView>().FirstOrDefault();
            if (scrollView != null) {
                // disable elements in the scrollview
                VisualElement scrollViewContent = scrollView.Q(null, unityScrollViewContentClass);
                if (scrollViewContent == null) {
                    Debug.LogError($"readonly PropDisableChildren invalid foldout scrollview no scrollViewContent on foldout {foldout.name} ScrollView {scrollView.name}");
                    return;
                }
                IEnumerable<PropertyField> scrollProperties = scrollViewContent.Children().OfType<PropertyField>();
                if (scrollProperties.Count() > 0) {
                    scrollProperties.ForEach(pf => PropDisable(pf));
                } else {
                    // delay to detect potential recursive properties
                    // _ = foldout.schedule.Execute(() => {
                    foldout.RegisterCallback<GeometryChangedEvent>((ce) => {
                        scrollViewContent.Children().OfType<PropertyField>().ForEach(pf => PropDisable(pf));
                    });
                }
            }
            // } else {
            // Debug.LogError($"readonly PropDisableChildren invalid foldout no scrollview on foldout {foldout.name}");
            // return;
        }
        /// <summary>
        /// Handle click events for read only foldouts.
        /// foldouts may not contain property fields until after they are opened (esp for recursion),
        /// make sure to disable them once they are made.
        /// </summary>
        /// <param name="clickEvent"></param>
        /// <param name="foldout"></param>
        static void FoldoutToggleClickEventHandler(ClickEvent clickEvent, Foldout foldout) {
            if (foldout.value) {
                // ? does this get called too much, registering geo changed event every time its clicked?
                // delay until after new fields are created
                // _ = foldout.schedule.Execute(() => {
                foldout.RegisterCallback<GeometryChangedEvent>((ce) => {
                    PropDisableChildren(foldout);
                });
            }
        }
    }
}