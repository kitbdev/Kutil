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
        public static VisualElement CreateRelPropertyGUI(VisualElement basePropertyVE, VisualElement relativePropertyVE, PropLayout propLayout, float relPropWeight = 1) {
            var container = new RelativePropertyVisualElement(basePropertyVE, relativePropertyVE, propLayout, relPropWeight);
            container.name = "RelativeProperty" + propLayout.ToString();
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

        public float weight = 1;
        public VisualElement basePropertyVE;
        public VisualElement relativePropertyVE;
        public PropLayout propLayout;
        public float relPropWeight;

        public RelativePropertyVisualElement(VisualElement basePropertyVE, VisualElement relativePropertyVE, PropLayout propLayout, float relPropWeight = 1) {
            this.weight = 1;
            this.basePropertyVE = basePropertyVE;
            this.relativePropertyVE = relativePropertyVE;
            this.propLayout = propLayout;
            this.relPropWeight = relPropWeight;
            Add(basePropertyVE);
            Add(relativePropertyVE);
            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            UpdateLayout();
        }

        public void UpdateLayout() {

            if (propLayout == PropLayout.Replace) {
                Remove(basePropertyVE);
                weight = relPropWeight;
            } else {
                Add(basePropertyVE);
            }
            if (propLayout == PropLayout.Above || propLayout == PropLayout.Left) {
                // rel prop should be first
                relativePropertyVE.SendToBack();
            } else {
                basePropertyVE.SendToBack();
            }
            FlexDirection fd = FlexDirection.Column;
            if (propLayout == PropLayout.Left || propLayout == PropLayout.Right) {
                fd = FlexDirection.Row;
            }

            style.flexDirection = new StyleEnum<FlexDirection>(fd);
            style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
            // style.flexGrow = 0;
            relativePropertyVE.style.flexGrow = relPropWeight;
            basePropertyVE.style.flexGrow = 1;// temp until after bind
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            UpdateWeight();
            // Debug.Log(name+" geo changed");
        }
        public void UpdateWeight() {
            // if the child is a RelPropVE, get the weight of it and set the flexgrow, so it expands properly 
            float basePropWeight = 1;
            RelativePropertyVisualElement baseChild = basePropertyVE.Children()
                                                    .OfType<RelativePropertyVisualElement>()
                                                    .FirstOrDefault();
            if (baseChild != null) {
                // updating children recursively in case they havent updated yet. if they have, oh well
                baseChild.UpdateWeight();
                basePropWeight = baseChild.weight;
            }
            // Debug.Log($"{relativePropertyVE.name} children: {basePropertyVE.Children().ToStringFull(ve => ve.name)} c{(baseChild != null ? baseChild.weight:0)} {basePropWeight}");
            float totalWeight = relPropWeight;
            if (propLayout != PropLayout.Replace) {
                totalWeight += basePropWeight;
            }
            basePropertyVE.style.flexGrow = basePropWeight;
            weight = totalWeight;
        }
    }
}