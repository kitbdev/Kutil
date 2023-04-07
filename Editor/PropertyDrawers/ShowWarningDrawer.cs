using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {

    [CustomPropertyDrawer(typeof(ShowWarningAttribute))]
    public class ShowWarningDrawer : ExtendedDecoratorDrawer {

        public static readonly string showWarningDecoratorClass = "kutil-show-warning-decorator";

        protected override string decoratorName => "ShowWarning";
        protected override string decoratorClass => showWarningDecoratorClass;

        public override bool registerUpdateCall => showWarning.isDynamic;

        HelpBox helpBox;

        ShowWarningAttribute showWarning => (ShowWarningAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            helpBox = new HelpBox(showWarning.warningText ?? "", showWarning.helpBoxMessageType);
            decorator = helpBox;
            if (decoratorName != null) {
                decorator.name = decoratorName;
            }
            if (decoratorClass != null) {
                decorator.AddToClassList(decoratorClass);
            }
            decorator.AddToClassList(extendedDecoratorClass);
            if (needSetupCall) {
                RegisterSetup();
            }
            return decorator;
        }

        protected override void Setup() {
            base.Setup();
            // Debug.Log("Setting up c hide decorator...");

            if (showWarning.isDynamic) {
                UpdateField();
            }
        }

        protected override void OnUpdate(SerializedPropertyChangeEvent ce) => UpdateField();
        void UpdateField() {
            // todo this updates multiple times even when only one change happens?
            if (!HasSerializedProperty()) {
                return;
            }
            // Debug.Log($"Updating field! on {serializedProperty.propertyPath} o:{serializedProperty.serializedObject.targetObject.name}");
            bool enabled = ShouldShowWarning(showWarning, serializedProperty) == showWarning.showIfTrue;
            helpBox.SetDisplay(enabled);

            if (enabled && showWarning.useTextAsSourceField) {
                string newText = null;
                if (newText == null) {
                    SerializedProperty warningSprop = serializedProperty.GetNeighborProperty(showWarning.warningText);
                    if (warningSprop != null && warningSprop.propertyType == SerializedPropertyType.String) {
                        newText = warningSprop.stringValue;
                    }
                    // Debug.Log("utsf1 " + warningSprop?.propertyPath + " " + warningSprop?.displayName);
                }
                if (newText == null && serializedProperty.TryGetValueOnPropRefl<string>(showWarning.warningText, out var propStr)) {
                    newText = propStr;
                    // Debug.Log("utsf2 "+propStr);
                }
                if (newText == null && serializedProperty.TryGetValueOnPropRefl<object>(showWarning.warningText, out var propObj)) {
                    newText = propObj.ToString();
                }
                if (newText == null) newText = "";
                helpBox.text = newText;
            }
        }


        private bool ShouldShowWarning(ShowWarningAttribute att, SerializedProperty property) {
            return ConditionalHideDrawer.GetPropertyConditionalValue(property, att.conditionalSourceField, att.enumIndices);
        }


    }
}