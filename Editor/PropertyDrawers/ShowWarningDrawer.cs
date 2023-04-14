using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {

    [CustomPropertyDrawer(typeof(ShowWarningAttribute))]
    public class ShowWarningDrawer : ExtendedDecoratorDrawer {

        public static readonly string showWarningDecoratorClass = "kutil-show-warning-decorator";

        protected override string decoratorName => "ShowWarning";
        protected override string decoratorClass => showWarningDecoratorClass;

        public override bool registerUpdateCall => showWarning.isDynamic;

        ShowWarningAttribute showWarning => (ShowWarningAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            ExtendedDecoratorData data = new ExtendedDecoratorData();

            var helpBox = new HelpBox(showWarning.warningText ?? "", showWarning.helpBoxMessageType);
            data.decorator = helpBox;
            if (decoratorName != null) {
                data.decorator.name = decoratorName;
            }
            if (decoratorClass != null) {
                data.decorator.AddToClassList(decoratorClass);
            }
            data.decorator.AddToClassList(extendedDecoratorClass);
            if (needSetupCall) {
                RegisterSetup(data);
            }
            return data.decorator;
        }

        protected override void Setup(ExtendedDecoratorData data) {
            base.Setup(data);
            // Debug.Log("Setting up c hide decorator...");

            if (showWarning.isDynamic) {
                UpdateField(data);
            }
        }

        protected override void OnUpdate(SerializedPropertyChangeEvent ce, ExtendedDecoratorData data) => UpdateField(data);
        void UpdateField(ExtendedDecoratorData data) {
            HelpBox helpBox = (HelpBox)data.decorator;
            // todo this updates multiple times even when only one change happens?
            if (!data.HasSerializedProperty()) {
                return;
            }
            // Debug.Log($"Updating field! on {serializedProperty.propertyPath} o:{serializedProperty.serializedObject.targetObject.name}");
            bool enabled = ShouldShowWarning(showWarning, data.serializedProperty) == showWarning.showIfTrue;
            helpBox.SetDisplay(enabled);

            if (enabled && showWarning.useTextAsSourceField) {
                string newText = null;
                if (newText == null) {
                    SerializedProperty warningSprop = data.serializedProperty.GetNeighborProperty(showWarning.warningText);
                    if (warningSprop != null && warningSprop.propertyType == SerializedPropertyType.String) {
                        newText = warningSprop.stringValue;
                    }
                    // Debug.Log("utsf1 " + warningSprop?.propertyPath + " " + warningSprop?.displayName);
                }
                if (newText == null && data.serializedProperty.TryGetValueOnPropRefl<string>(showWarning.warningText, out var propStr)) {
                    newText = propStr;
                    // Debug.Log("utsf2 "+propStr);
                }
                if (newText == null && data.serializedProperty.TryGetValueOnPropRefl<object>(showWarning.warningText, out var propObj)) {
                    newText = propObj.ToString();
                }
                if (newText == null) newText = "";
                helpBox.text = newText;
            }
        }


        private static bool ShouldShowWarning(ShowWarningAttribute att, SerializedProperty property) {
            return ConditionalHideDrawer.GetPropertyConditionalValue(property, att.conditionalSourceField, att.enumIndices);
        }


    }
}