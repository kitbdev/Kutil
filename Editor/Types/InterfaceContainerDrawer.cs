using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    [CustomPropertyDrawer(typeof(InterfaceContainer<>))]
    public class InterfaceContainerDrawer : PropertyDrawer {

        static readonly string interfaceContainerClass = "interface-container";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // VisualElement root = new VisualElement();

            ObjectField field = new ObjectField(property.displayName);
            field.bindingPath = property.propertyPath;
            field.AddToClassList(interfaceContainerClass);
            field.AddToClassList(ObjectField.alignedFieldUssClassName);
            // /property.managedReferenceFullTypename+ err
            System.Type genericType = fieldInfo.FieldType.GetGenericTypeDefinition();
            // Debug.Log(property.type+", "+", "+ genericType);
            // field.objectType = genericType;
            field.objectType = typeof(MonoBehaviour);
            field.RegisterValueChangedCallback(ce => {
                Debug.Log("ce " + ce.previousValue + " to " + ce.newValue + " " + ce.currentTarget);
            });

            return field;
        }
        class InterfaceObjectField : ObjectField {
            public InterfaceObjectField(string label = null) : base(label) {
                
            }
            protected override void ExecuteDefaultAction(EventBase evt)
            {
                base.ExecuteDefaultAction(evt);
            }
        }
    }
}