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
            //  this disables opening foldouts
            // propField.SetEnabled(false);

            container.Add(propField);

            // propField.AddToClassList("unity-disabled");

            // Debug.Log("c" + propField.childCount);

            // After prop field has binded
            _ = propField.schedule.Execute(() => {
                // todo does not propagate through nested serialized references 
                // disable all visual elements without a Foldout in them
                PropDisable(propField);

                // List<VisualElement> ds = propField.Query<VisualElement>().Where(p => p.Q<Foldout>() == null).ToList();
                // Debug.Log("disables: " + ds.ToStringFull());
                // foreach (var d in ds) {
                //     d.SetEnabled(false);
                // }

                // propField.Q<Foldout>()

                // DFSPropDisable(propField, propField);
                // Debug.Log("cb" + propField.childCount);
                // List<VisualElement> toggles = new();
                // ForEachChildRecursively(propField, ve => {
                //     if (ve is PropertyField || ve is Label) {
                //         ve.SetEnabled(false);
                //     }
                //     if (ve is Toggle) {
                //         toggles.Add(ve);
                //         Label l = ve.Q<Label>();
                //         if (l == null) {
                //             Debug.LogWarning("cannot find toggle label");
                //         } else {
                //             l.SetEnabled(false);
                //         }
                //     }
                // }, ve => ve is not Toggle
                // );
                // propField.SetEnabled(true);
                // Debug.Log(toggles.ToStringFull());
                // // make sure toggles are not disabled by parents
                // foreach (var toggle in toggles) {
                //     var p = toggle;
                //     while (p != null && p != propField) {
                //         p.SetEnabled(true);
                //         p = p.parent;
                //     }
                // }
            });
            // propField.EnableInClassList("disabled", true);
            // propField.EnableInClassList("unity-disabled", true);
            // propField.RemoveFromClassList("enabled");
            // container.Add(new Label("test"));
            //https://forum.unity.com/threads/disable-size-attribute-for-propertydrawer.462696/
            // _ = propField.schedule.Execute(() => {
            //     // Get size field of array
            //     var array = container.parent;
            //     // todo check if actually an array
            //     // array.Q<IntegerField>("size?")
            //     // if (array)
            //     // IntegerField sizeField = array.Q<IntegerField>();

            //     // Disallow changing array size in inspector
            //     // array.SetEnabled(false);
            // });
            return container;
            // return base.CreatePropertyGUI(property);
        }

        void PropDisable(PropertyField propField) {
            ListView listView = propField.Q<ListView>();
            if (listView != null) {
                var listSize = listView.Query("unity-list-view__size-field").Last();
                listSize.SetEnabled(false);
            }
            Foldout foldout = propField.Q<Foldout>();
            if (foldout == null) {
                propField.SetEnabled(false);
                return;
            }
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
            // toggle.RegisterValueChangedCallback<bool, VisualElement>(ToggleClickEventHandler, toggle);
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


        // void DFSPropDisable(VisualElement cur, PropertyField propField) {
        //     if (cur is not Foldout) {
        //         cur.SetEnabled(false);
        //         foreach (var child in cur.Children()) {
        //             DFSPropDisable(child, propField);
        //         }
        //     } else {
        //         Label l = cur.Q<Label>();
        //         l.SetEnabled(false);
        //         VisualElement foldoutContainer = cur.Q("unity-content");
        //         DFSPropDisable(foldoutContainer, propField);
        //         var p = cur;
        //         while (p != null && p != propField) {
        //             p.SetEnabled(true);
        //             p = p.parent;
        //         }
        //     }
        // }

        void ForEachChildRecursively(VisualElement root, System.Action<VisualElement> action, System.Func<VisualElement, bool> searchChildren = null) {
            List<VisualElement> searched = new();
            Queue<VisualElement> frontier = new();
            frontier.Enqueue(root);
            while (frontier.Count > 0) {
                var cur = frontier.Dequeue();
                searched.Add(cur);
                action?.Invoke(cur);
                if (searchChildren != null && !searchChildren.Invoke(cur)) {
                    continue;
                }
                foreach (var child in cur.Children()) {
                    if (!searched.Contains(child) && !frontier.Contains(child)) {
                        frontier.Enqueue(child);
                    }
                }
            }
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