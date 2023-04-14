using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System;
using static Kutil.Editor.PropertyDrawers.ExtendedDecoratorDrawer;

namespace Kutil.Editor.PropertyDrawers {
    // [DefaultExecutionOrder(10)]// after other drawers
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : DecoratorDrawer {

        public static readonly string readonlyDecoratorClass = "kutil-readonly";
        public static readonly string readonlyFoldoutClass = "kutil-readonly-foldout";
        public static readonly string readonlyPropertyClass = "kutil-readonly-property";
        public static readonly string readonlyListViewClass = "kutil-readonly-listview";
        public static readonly string readonlyVEClass = "kutil-readonly-ve";

        // public static readonly string unityListViewSizeFieldName = "unity-list-view__size-field";
        // public static readonly string unityFoldoutContentName = "unity-content";
        // public static readonly string unityScrollViewContentName = "unity-content-container";
        public static string unityListViewSizeFieldClass => ListView.arraySizeFieldUssClassName;
        public static string unityFoldoutContentClass => Foldout.contentUssClassName;
        public static string unityScrollViewContentClass => ScrollView.contentUssClassName;

        ReadOnlyAttribute readOnlyAttribute => (ReadOnlyAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            ExtendedDecoratorData data = new ExtendedDecoratorData();
            // property = null;
            data.decorator = new VisualElement();
            data.decorator.name = "ReadOnly";
            data.decorator.AddToClassList(readonlyDecoratorClass);

            // RegisterSetup(data);
            // readOnlyDecorator.RegisterCallback<GeometryChangedEvent>(OnDecoratorGeometryChanged);
            data.decorator.RegisterCallback<AttachToPanelEvent, ExtendedDecoratorData>(OnAttach, data);
            data.decorator.RegisterCallback<DetachFromPanelEvent, ExtendedDecoratorData>(OnDetach, data);
            return data.decorator;
        }

        private void OnAttach(AttachToPanelEvent evt, ExtendedDecoratorData data) {
            data.decorator.UnregisterCallback<AttachToPanelEvent, ExtendedDecoratorData>(OnAttach);
            // use a changed event so we can access other VisualElements

            if (data.propertyField == null) {
                Debug.LogError($"ReadOnly failed to find containing property! {data.decorator.name}");
                return;
            }
            data.propertyField.RegisterCallback<GeometryChangedEvent, ExtendedDecoratorData>(OnPropGeometryChanged, data);
            if (readOnlyAttribute.allowArrayScrolling) {
                MakeReadOnly(data.propertyField);
            } else {
                data.propertyField.SetEnabled(false);
            }

            //PropDisable(propertyField);
            // PropDisableNew(propertyField);
            // Debug.Log("on attach update");

            // MakeReadOnly(propertyField);
        }

        private void OnDetach(DetachFromPanelEvent evt, ExtendedDecoratorData data) {
            data.decorator.UnregisterCallback<DetachFromPanelEvent, ExtendedDecoratorData>(OnDetach);
            data.propertyField.UnregisterCallback<GeometryChangedEvent, ExtendedDecoratorData>(OnPropGeometryChanged);

        }

        // private void OnDecoratorGeometryChanged(GeometryChangedEvent changedEvent, ExtendedDecoratorData data) {
        //     // only need to do once?
        //     data.decorator.UnregisterCallback<GeometryChangedEvent, ExtendedDecoratorData>(OnDecoratorGeometryChanged);

        //     // use a changed event so we can access other VisualElements

        //     data.propertyField.RegisterCallback<GeometryChangedEvent, ExtendedDecoratorData>(OnPropGeometryChanged, data);


        //     //PropDisable(propertyField);
        //     // PropDisableNew(propertyField);
        //     // if (readOnlyAttribute.allowArrayScrolling) {
        //     //     MakeReadOnly(propertyField);
        //     // } else {
        //     //     propertyField.SetEnabled(false);
        //     // }
        // }


        private void OnPropGeometryChanged(GeometryChangedEvent changedEvent, ExtendedDecoratorData data) {
            if (!readOnlyAttribute.updateOften) {
                data.propertyField.UnregisterCallback<GeometryChangedEvent, ExtendedDecoratorData>(OnPropGeometryChanged);
            }
            // Debug.Log("PropGeoChanged update");
            // todo not on every geo changed event...
            // need to keep calling because may not have a scrollview, then might get one
            // and only update for that is GeoChanged...
            if (readOnlyAttribute.allowArrayScrolling) {
                MakeReadOnly(data.propertyField);
            } else {
                data.propertyField.SetEnabled(false);
            }
        }
        // void OnUpdate(SerializedPropertyChangeEvent changeEvent) {
        // }

        // public static void DisableField(VisualElement field) {
        //     if (field == null) {
        //         Debug.LogError($"readonly DisableField null field!");
        //         return;
        //     }
        // }
        static void PropDisableNew(PropertyField propertyField) {
            // dont need to not disable foldouts anymore...
            propertyField.AddToClassList(readonlyPropertyClass);
            propertyField.SetEnabled(false);
            // maybe still remove list stuff?
        }

        /// <summary>
        /// Make a visual element read only.
        /// does not disable scrollbars.
        /// </summary>
        /// <param name="visualElement">the visual element target</param>
        /// <param name="enabled">set to false to make read only. set to true to undo, make writable</param>
        /// <param name="recClass">used to class all disabled elements, 
        /// prevents making visualelement children not readonly when made readonly by another element
        /// set to null to generate one from the name automatically.
        /// </param>
        public static void MakeReadOnly(VisualElement visualElement, bool enabled = false, string recClass = null) {
            if (visualElement == null) {
                return;
            }
            if (recClass == null) {
                recClass = $"{readonlyVEClass}-{visualElement.name}";
                if (visualElement.name == null || visualElement.name == "") {
                    // Debug.LogWarning($"Readonly {visualElement.ToStringBetter()} has no name!");
                    // use class list instead
                    recClass += visualElement.GetClasses().ToStringFull(seperator: "---", includeBraces: false);
                }
                visualElement.AddToClassList(recClass);
            }

            // todo this throws exceptions when a list is changed during runtime, sometimes?
            // maybe not?

            // Debug.Log($"a:make readonly  {visualElement.name} {visualElement.GetType().Name} c:{visualElement.childCount}");

            // dont check cause we might want to make enabled
            // if (visualElement.enabledSelf == false) {
            //     // fully disabled already
            //     Debug.Log($"ES make readonly {visualElement.ToStringBetter()} p:{visualElement.parent.ToStringBetter()} c:{visualElement.Children().ToStringFull(v => v.ToStringBetter(), true)}");
            //     return;
            // }
            if (!enabled && visualElement.enabledInHierarchy == false) {
                // dont need to do anything if were already disabled
                // probably, unless that changes...
                // Debug.Log($"EIH make readonly {visualElement.ToStringBetter()} c:{visualElement.childCount} p:{visualElement.parent.ToStringBetter()}");
                return;
            }

            // dont disable if has any scroll views
            List<ScrollView> scrollViews = visualElement.Query<ScrollView>().ToList();
            bool canDisable = scrollViews == null || scrollViews.Count == 0;
            // Debug.Log($"make readonly {canDisable} {visualElement.ToStringBetter()} c:{visualElement.childCount} {visualElement.Children().ToStringFull(v => v.ToStringBetter())})");
            if (canDisable) {
                // just disable self
                if (!visualElement.ClassListContains(recClass)) {
                    visualElement.AddToClassList(recClass);
                }
                visualElement.SetEnabled(enabled);
                return;
            } else {
                if (!visualElement.enabledSelf) {
                    // only if we disabled previously
                    // can multiple use?
                    if (visualElement.ClassListContains(recClass)) {
                        // cant just force to be enabled, maybe someone else wants it disabled
                        visualElement.SetEnabled(true);
                    } else {
                        // someone else disabled, return
                        return;
                    }
                }
            }
            // make readonly on children
            IEnumerable<VisualElement> allChildren = visualElement.Children();

            // handle certain elements
            if (visualElement is ListView listView) {
                // disable controls and children
                Label foldoutLabel = listView.Q<Label>();
                if (foldoutLabel == null) {
                    Debug.LogError($"MakeReadOnly listView Label missing {visualElement.name} {listView.name}!");
                    return;
                }
                foldoutLabel.SetEnabled(enabled);
                listView.reorderable = enabled;
                listView.showAddRemoveFooter = enabled;
                var listSize = listView.Query(null, unityListViewSizeFieldClass).Last();
                if (listSize == null) {
                    // ? could this happen if nonreorderable is true?
                    Debug.LogError($"MakeReadOnly {visualElement.name} listview {listView.name} has no listsizefield!");
                    return;
                }
                listSize.SetEnabled(enabled);

                // get list children, cause .children() doesnt work
                // const string ClassName = "unity-list-view__item";
                allChildren = listView.Query<VisualElement>(className: ListView.itemUssClassName).ToList();
            } else if (visualElement is ScrollView scrollView) {
                // disable children
                // todo check
                // allChildren = scrollView.Query<VisualElement>(className: "unity-list-view__item").ToList();
                // pass
            } else if (visualElement is Foldout foldout) {
                Label foldoutLabel = foldout.Q<Label>();
                if (foldoutLabel == null) {
                    Debug.LogError($"MakeReadOnly foldout Label missing {visualElement.name} {foldout.name}!");
                    return;
                }
                foldoutLabel.SetEnabled(enabled);

                // Toggle toggle = foldout.Q<Toggle>();
                // if (toggle == null) {
                //     Debug.LogError($"MakeReadOnly foldout toggle missing {visualElement.name} {foldout.name}!");
                //     return;
                // }
                // toggle.RegisterCallback<ClickEvent, Foldout>(FoldoutToggleClickEventHandler, foldout);

            }

            // note children returns content container children, not actual children, how to disable labels?
            foreach (var child in allChildren) {
                MakeReadOnly(child, enabled, recClass);
            }
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