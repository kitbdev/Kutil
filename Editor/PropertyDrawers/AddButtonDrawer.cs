using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {

    /// <summary>
    /// Adds a button above the field
    /// </summary>
    [CustomPropertyDrawer(typeof(AddButtonAttribute))]
    public class AddButtonDrawer : DecoratorDrawer {

        public static readonly string addButtonClass = "kutil-add-button";
        AddButtonAttribute addButton => (AddButtonAttribute)attribute;

        SerializedProperty serializedProperty;
        Button btn;

        // todo color

        public override VisualElement CreatePropertyGUI() {
            serializedProperty = null;

            btn = new Button();
            btn.AddToClassList(addButtonClass);
            btn.text = addButton.buttonLabel ?? addButton.buttonMethodName;
            btn.name = $"{btn.text} Button";
            if (addButton.buttonTooltip != null) {
                btn.tooltip = addButton.buttonTooltip;
            }
            btn.enableRichText = addButton.richText;
            btn.clicked += () => {
                // decorators dont have access to the property...
                serializedProperty ??= SerializedPropertyExtensions.GetBindedPropertyFromDecorator(btn);
                if (serializedProperty == null) {
                    Debug.LogWarning($"Cannot call method on button {btn.name} cannot find prop");
                    return;
                }
                CallButtonMethod(serializedProperty);
            };
            btn.RegisterCallback<GeometryChangedEvent>(OnSetup);
            return btn;
        }
        void OnSetup(GeometryChangedEvent changedEvent) {
            btn.UnregisterCallback<GeometryChangedEvent>(OnSetup);
            if (addButton.hideProperty) {
                PropertyField propertyField = btn.GetFirstAncestorOfType<PropertyField>();
                if (propertyField != null && propertyField.childCount > 1) {
                    propertyField[1].style.display = DisplayStyle.None;
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