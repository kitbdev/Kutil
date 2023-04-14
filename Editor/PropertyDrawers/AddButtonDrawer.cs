using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {

    /// <summary>
    /// Adds a button above the field
    /// </summary>
    [CustomPropertyDrawer(typeof(AddButtonAttribute))]
    public class AddButtonDrawer : ExtendedDecoratorDrawer {

        public static readonly string addButtonClass = "kutil-add-button";
        AddButtonAttribute addButton => (AddButtonAttribute)attribute;


        // todo color

        // todo option to add button to the right of the field, instead of on top

        public override VisualElement CreatePropertyGUI() {
            ExtendedDecoratorData data = new();

            Button btn = new Button();
            data.decorator = btn;
            btn.AddToClassList(addButtonClass);
            btn.text = addButton.buttonLabel ?? addButton.buttonMethodName;
            btn.name = $"{btn.text} Button";
            if (addButton.buttonTooltip != null) {
                btn.tooltip = addButton.buttonTooltip;
            }
            btn.enableRichText = addButton.richText;
            btn.clicked += () => {
                CallButtonMethod(data.serializedProperty);
            };
            RegisterSetup(data);
            return btn;
        }
        void OnSetup(GeometryChangedEvent changedEvent, ExtendedDecoratorData data) {
            Button btn = (Button)data.decorator;
            if (addButton.hideProperty) {
                PropertyField propertyField = btn.GetFirstAncestorOfType<PropertyField>();
                if (propertyField != null && propertyField.childCount > 1) {
                    propertyField[1].SetDisplay(false);
                }
            }
        }

        void CallButtonMethod(SerializedProperty property) {
            if (!addButton.allowCallInEditor && !Application.isPlaying) {
                return;// cannot call while in edit mode
            }
            // use reflection to support arrays and nesting too
            string path = property.propertyPath.Replace(property.name, addButton.buttonMethodName);
            Object[] targetObjects = property.serializedObject.targetObjects;
            foreach (var targetObj in targetObjects) {
                ReflectionHelper.TryCallMethod(targetObj, path, addButton.parameters);
                if (!addButton.allowMultipleCalls) {
                    break;
                }
            }
        }

    }
}