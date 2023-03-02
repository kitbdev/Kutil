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

        // ? can we find these elsewhere
        static readonly string unityDecoratorContainerClass = "unity-decorator-drawers-container";
        static readonly string unityHeaderDecoratorClass = "unity-header-drawer__label";
        static readonly string unitySpaceDecoratorClass = "unity-space-drawer";

        public static readonly string baseClass = "kutil-collapsable-drawer";
        public static readonly string foldoutClass = "kutil-collapsable-drawer-foldout";
        public static readonly string collapsableClass = "kutil-collapsable-marker";

        CollapsableAttribute collapsable => (CollapsableAttribute)attribute;

        // todo turn into decorator?

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement defVE = new PropertyField(property);
            defVE.AddToClassList(collapsableClass);

            // cAtt.isCollapsed = cAtt.startCollapsed;
            // Debug.Log(property.propertyPath+" "+property.serializedObject.);

            // defVE.RegisterCallback<GeometryChangedEvent>((ce)=>{//! recursive fails
            _ = defVE.schedule.Execute(() => CreateCollapsable(defVE));
            return defVE;
        }

        private void CreateCollapsable(VisualElement defVE) {
            // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
            // after layout
            VisualElement collapsableBase = new VisualElement();
            collapsableBase.AddToClassList(baseClass);

            bool addspace = false;
            if (addspace) {
                VisualElement spacer = new VisualElement();
                spacer.AddToClassList(unityDecoratorContainerClass);
                // default space height is 8px
                spacer.style.height = new StyleLength(8f);
                // default header top margin is 12px
                // spacer.style.marginTop = new StyleLength(15f);
                collapsableBase.Add(spacer);
            }

            // insert a Foldout
            Foldout foldout = new Foldout();
            collapsableBase.Add(foldout);
            foldout.value = !collapsable.startCollapsed;
            // foldout.value = !collapsed;
            // foldout.SetValueWithoutNotify(!collapsed);
            // foldout.RegisterValueChangedCallback(ce => {
            //     collapsed = !ce.newValue;
            //     Debug.Log("collapsed " + collapsed);
            // });
            if (collapsable.text != null) {
                foldout.text = collapsable.text;
                Label label = foldout.Q<Label>();
                label.AddToClassList(unityHeaderDecoratorClass);
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

                        // todo test all cases
                        bool isSpaceDec = el.Q(null, unitySpaceDecoratorClass) == null;
                        bool takeSpaceDec = collapsable.includeSpaces || isSpaceDec;
                        bool isHeaderDec = el.Q(null, unityHeaderDecoratorClass) == null;
                        bool takeHeaderDec = collapsable.includeHeaders || isHeaderDec;
                        bool takeOtherDec = isSpaceDec || isHeaderDec || (collapsable.includeOtherDecorators || el.Q(null, unityDecoratorContainerClass) == null);
                        bool takeDec = takeSpaceDec && takeHeaderDec && takeOtherDec;
                        // ? any other end markers?
                        // take while no childs have these classes
                        bool takeCollapsable = el.Q(null, collapsableClass) == null;
                        bool takeSelf = el.Q(null, baseClass) == null;
                        return takeDec && takeCollapsable && takeSelf;
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
        };
    }
}