using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// Conditionally hides a decorator based on a bool or enum field
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalHideDecoratorsAttribute))]
    public class ConditionalHideDecoratorsDrawer : ExtendedDecoratorDrawer {

        // public static readonly string conditionalHideClass = "kutil-conditional-hide-decorator";
        public static readonly string conditionalHideDecoratorClass = "kutil-conditional-hide-decorators-decorator";

        ConditionalHideDecoratorsAttribute conditionalHide => (ConditionalHideDecoratorsAttribute)attribute;

        protected override string decoratorName => "ConditionalHideDecoratorsDecorator";
        protected override string decoratorClass => conditionalHideDecoratorClass;

        public override bool registerUpdateCall => true;

        protected override void Setup(ExtendedDecoratorData data) {
            base.Setup(data);
            // Debug.Log("Setting up c hide decorator...");
            // propertyField.AddToClassList(conditionalHideClass);

            UpdateField(data);
        }

        protected override void OnUpdate(SerializedPropertyChangeEvent ce, ExtendedDecoratorData data) => UpdateField(data);
        void UpdateField(ExtendedDecoratorData data) {
            if (!data.HasSerializedProperty()) return;
            // todo? any way to update more often? or trigger manually

            VisualElement decoratorContainer = data.decorator.parent;
            if (decoratorContainer == null) {
                Debug.LogError($"{GetType().Name} {data.decorator.name} missing decorator container!");
                return;
            }
            if (decoratorContainer.childCount <= 1 || conditionalHide.numToHide <= 0) {
                // no other decorators
                return;
            }
            int myDecIndex = decoratorContainer.IndexOf(data.decorator);
            // Debug.Log($"{myDecIndex} / {decoratorContainer.childCount}");
            if (myDecIndex < 0 || myDecIndex >= decoratorContainer.childCount - 1) {
                // no need to move 
                return;
            }

            IEnumerable<VisualElement> decoratorsToAffect = decoratorContainer.Children().Skip(myDecIndex + 1);

            // Debug.Log($"Updating field! on {serializedProperty.propertyPath} o:{serializedProperty.serializedObject.targetObject.name}");
            bool enabled = GetConditionalHideAttributeResult(conditionalHide, data.serializedProperty) == conditionalHide.showIfTrue;
            foreach (var dec in decoratorsToAffect) {
                if (conditionalHide.readonlyInstead) {
                    // propertyField.SetEnabled(enabled);
                    ReadOnlyDrawer.MakeReadOnly(dec, enabled);
                } else {
                    dec.SetDisplay(enabled);
                }
            }
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideDecoratorsAttribute att, SerializedProperty serializedProperty) {
            return ConditionalHideDrawer.GetPropertyConditionalValue(serializedProperty, att.conditionalSourceField, att.enumIndices);
        }
    }
}