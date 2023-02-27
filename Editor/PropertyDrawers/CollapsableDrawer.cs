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

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement defVE = new PropertyField(property);
            defVE.AddToClassList(collapsableClass);
            CollapsableAttribute cAtt = (CollapsableAttribute)attribute;
            // cAtt.isCollapsed = cAtt.startCollapsed;

            // defVE.RegisterCallback<GeometryChangedEvent>(()=>{//?
            _ = defVE.schedule.Execute(() => {
                // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
                // after layout
                VisualElement collapsableBase = new VisualElement();
                collapsableBase.AddToClassList(baseClass);

                bool addspace = false;
                if (addspace) {
                    VisualElement spacer = new VisualElement();
                    spacer.AddToClassList(decoratorClass);
                    // default space height is 8px
                    spacer.style.height = new StyleLength(8f);
                    // default header top margin is 12px
                    // spacer.style.marginTop = new StyleLength(15f);
                    collapsableBase.Add(spacer);
                }

                // insert a Foldout
                Foldout foldout = new Foldout();
                collapsableBase.Add(foldout);
                foldout.value = !cAtt.startCollapsed;
                // foldout.value = !collapsed;
                // foldout.SetValueWithoutNotify(!collapsed);
                // foldout.RegisterValueChangedCallback(ce => {
                //     collapsed = !ce.newValue;
                //     Debug.Log("collapsed " + collapsed);
                // });
                if (cAtt.text != null) {
                    foldout.text = cAtt.text;
                    Label label = foldout.Q<Label>();
                    label.AddToClassList(headerFontClass);
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                foldout.AddToClassList(foldoutClass);

                // remove indent?
                // VisualElement foldoutContainer = foldout.Q("unity-content");
                // foldoutContainer.style.marginLeft = new StyleLength(0f);


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
                        .TakeWhile((el, i) => {
                            if (i == 0) return true;
                            // ? any other end markers?
                            // take while no childs have these classes
                            return el.Q(null, decoratorClass) == null
                                && el.Q(null, collapsableClass) == null
                                && el.Q(null, baseClass) == null
                                ;
                        })
                        .ToArray();

                // Debug.Log($"par:{parent.name} c:{cPropVE.name} involved:{childs.ToStringFull(ve => ve.name, true)}");
                // return;
                if (childs.Count() == 0) return;
                int placeIndex = parent.IndexOf(childs.First());
                parent.Insert(placeIndex, collapsableBase);
                collapsableBase.name = $"Collapsable_{childs.First().name}_to_{childs.Last().name}";
                // set viewdata uniquely to make foldout remember folded state
                // https://forum.unity.com/threads/can-someone-explain-the-view-data-key-and-its-uses.855145/#post-5638936
                foldout.viewDataKey = $"{baseClass}_{inspectorElement.name}_{childs.First().name}";
                // Debug.Log(foldout.viewDataKey);

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