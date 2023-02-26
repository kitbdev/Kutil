using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// create a foldout from the attribute to the next CollapsableAttribute or the next decorator(header or space)
    /// </summary>
    [CustomPropertyDrawer(typeof(CollapsableAttribute))]
    public class CollapsableDrawer : PropertyDrawer {
        static readonly string decoratorClass = "unity-decorator-drawers-container";
        static readonly string headerFontClass = "unity-header-drawer__label";

        static readonly string baseClass = "kutil-collapsable-drawer";
        static readonly string foldoutClass = "kutil-collapsable-drawer-foldout";
        static readonly string collapsableClass = "kutil-collapsable-marker";

        // todo restore collapsed state?
        bool collapsed = false;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // VisualElement defVE = base.CreatePropertyGUI(property);
            VisualElement defVE = new PropertyField(property);
            defVE.AddToClassList(collapsableClass);
            CollapsableAttribute cAtt = (CollapsableAttribute)attribute;
            // SerializedObject serializedObject = new SerializedObject(this);

            _ = defVE.schedule.Execute(() => {
                // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
                // after layout
                VisualElement collapsableBase = new VisualElement();
                collapsableBase.AddToClassList(baseClass);

                // insert a Foldout
                Foldout foldout = new Foldout();
                collapsableBase.Add(foldout);
                foldout.SetValueWithoutNotify(!collapsed);
                foldout.RegisterValueChangedCallback(ce => {
                    collapsed = !ce.newValue;
                });

                if (cAtt.text != null) {
                    foldout.text = cAtt.text;
                    Label label = foldout.Q<Label>();
                    label.AddToClassList(headerFontClass);
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                foldout.AddToClassList(foldoutClass);

                // get parent
                // parent should be Inspector element (unless nested?)
                InspectorElement inspectorElement = defVE.GetFirstAncestorOfType<InspectorElement>();
                if (inspectorElement == null) {
                    Debug.LogError("cannot find inspector element");
                    return;
                }
                // todo nestable
                VisualElement parent = inspectorElement;
                VisualElement cPropVE = defVE;
                while (cPropVE.parent != parent) {
                    cPropVE = cPropVE.parent;
                }

                // get all top level children to move into the foldout
                VisualElement[] childs = parent.Children()
                        .SkipWhile((el) => el != cPropVE)
                        // ? any other end markers?
                        .TakeWhile((el, i) => {
                            if (i == 0) return true;
                            return el.Q(null, decoratorClass) == null
                                && el.Q(null, collapsableClass) == null;
                        })
                        .ToArray();

                // Debug.Log($"par:{parent.name} c:{cPropVE.name} involved:{childs.ToStringFull(ve => ve.name, true)}");
                // return;
                if (childs.Count() == 0) return;
                int placeIndex = parent.IndexOf(childs.First());
                parent.Insert(placeIndex, collapsableBase);
                collapsableBase.name = $"Collapsable_{childs.First().name}_to_{childs.Last().name}";

                // move to foldout
                foreach (var child in childs) {
                    parent.Remove(child);
                    foldout.Add(child);
                }
            });
            return defVE;
        }
    }
}