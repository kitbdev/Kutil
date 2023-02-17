using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    /// <summary>
    /// generic relative property drawer with left,right,above,below,replace options
    /// </summary>
    public static class RelativePropertyDrawer {

        // todo add foldout options?

        /// <summary>
        /// Returns a container that holds the two VEs in the specified layout.
        /// the basePropertyVE may not be added at all, if the layout is replace
        /// </summary>
        /// <param name="basePropertyVE"></param>
        /// <param name="relativePropertyVE"></param>
        /// <param name="propLayout"></param>
        /// <param name="relPropWeight"></param>
        /// <param name="widthLen"></param>
        /// <returns>a RelativePropertyVisualElement container</returns>
        public static VisualElement CreateRelPropertyGUI(VisualElement basePropertyVE, VisualElement relativePropertyVE, PropLayout propLayout, float relPropWeight = 1, StyleLength? widthLen = null) {
            var container = new RelativePropertyVisualElement();
            container.name = "RelativePropertyDrawer";
            // const string relPropClassName = "relative-property";
            // container.AddToClassList(relPropClassName);

            FlexDirection fd = FlexDirection.Column;
            float totalWeight = relPropWeight;

            if (propLayout != PropLayout.Replace) {
                container.Add(basePropertyVE);
                // totalWeight += basePropWeight;
            } else {
                container.weight = relPropWeight;
                // container.style.flexGrow = relPropWeight;
            }
            container.Add(relativePropertyVE);

            if (propLayout == PropLayout.Above
            || propLayout == PropLayout.Left) {
                relativePropertyVE.SendToBack();
            }
            if (propLayout == PropLayout.Left
            || propLayout == PropLayout.Right) {
                // btn.style.width = $"{btnData.btnWidth}px";
                fd = FlexDirection.Row;
                if (widthLen != null) {
                    // relativePropertyVE.style.width = (StyleLength)widthLen;// ?? new StyleLength(new Length(50, LengthUnit.Percent));
                    // basePropertyVE.style.width = widthLen ?? new StyleLength(new Length(50, LengthUnit.Percent));
                }
            }

            container.style.flexDirection = new StyleEnum<FlexDirection>(fd);
            container.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
            relativePropertyVE.style.flexGrow = relPropWeight;
            // container.style.flexGrow = 1;
            // basePropertyVE.style.flexGrow = basePropWeight;

            // after property has been binded, 
            _ = basePropertyVE.schedule.Execute(() => {
                // if the child is a RelPropVE, get the weight of it and set the flexgrow, so it expands properly 
                float basePropWeight = 1;
                RelativePropertyVisualElement baseChild = basePropertyVE.Children()
                                                        .OfType<RelativePropertyVisualElement>()
                                                        // .Where(c => c.ClassListContains(relPropClassName))
                                                        .FirstOrDefault();
                if (baseChild != null) {
                    // basePropWeight = baseChild.style.flexGrow.value;
                    basePropWeight = baseChild.weight;
                }
                // Debug.Log($"{relativePropertyVE.name} children: {basePropertyVE.Children().ToStringFull(ve => ve.name)} c{(baseChild != null ? baseChild.weight:0)} {basePropWeight}");
                if (propLayout != PropLayout.Replace) {
                    totalWeight += basePropWeight;
                }
                basePropertyVE.style.flexGrow = basePropWeight;
                container.weight = totalWeight;
            });

            return container;
        }


        public static void OnGUI(Rect position, System.Action<Rect> drawBaseProp, System.Action<Rect> drawRelProp, PropLayout propLayout, float propHeight, float relHeight = -1, float relWidth = 50, float spacing = 5) {
            // for some reason label gets cleared after get height
            if (relHeight < 0) relHeight = EditorGUIUtility.singleLineHeight;
            Rect relRect = position;
            relRect.height = relHeight;
            Rect propRect = position;
            propRect.height = propHeight;

            if (propLayout == PropLayout.Replace) {
                drawRelProp(relRect);
                return;
            }
            if (propLayout == PropLayout.Above) {
                propRect.y += relHeight;
                drawRelProp(relRect);
            }
            if (propLayout == PropLayout.Left
            || propLayout == PropLayout.Right) {
                // small button and move prop
                float otherWidth = relWidth;
                relRect.width = otherWidth;
                propRect.width -= otherWidth - spacing;
                if (propLayout == PropLayout.Left) {
                    propRect.x = otherWidth + spacing;
                    EditorGUI.indentLevel += 1;
                    drawRelProp(relRect);
                }
                if (propLayout == PropLayout.Right) {
                    relRect.x = propRect.width + spacing;
                    drawRelProp(relRect);
                }
            }
            drawBaseProp(propRect);
            if (propLayout == PropLayout.Left) {
                EditorGUI.indentLevel -= 1;
            }
            // EditorGUI.PropertyField(propRect, property, label, true);
            if (propLayout == PropLayout.Below) {
                relRect.y += relHeight;
                drawRelProp(relRect);
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label, PropLayout layout, float otherHeight = -1) {
            if (otherHeight < 0) otherHeight = EditorGUIUtility.singleLineHeight;
            float propHeight = EditorGUI.GetPropertyHeight(property, label);
            if (layout == PropLayout.Replace) {
                return otherHeight;
            } else if (layout == PropLayout.Left || layout == PropLayout.Right) {
                return Mathf.Max(otherHeight, propHeight);
            }
            // before or after
            float height = otherHeight + propHeight;
            return height;
        }
    }

    public class RelativePropertyVisualElement : VisualElement {
        public new class UxmlFactory : UxmlFactory<RelativePropertyVisualElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits {

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription {
                get { yield break; }
            }
            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext) {
                base.Init(visualElement, attributes, creationContext);
                var element = visualElement as RelativePropertyVisualElement;
                if (element != null) {

                }
            }
        }
        public float weight = 1;

        // public RelativePropertyVisualElement() {
        //     this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        // }

        // private void OnGeometryChanged(GeometryChangedEvent evt) {

        // }
    }
}