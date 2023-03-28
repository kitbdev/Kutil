using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// DecoratorDrawer with some additional features.
    /// Accessors for PropertyField, SerializedProperty, and FieldInfo.
    /// setup and update method callbacks
    /// default empty decorator created. 
    /// </summary>
    public abstract class ExtendedDecoratorDrawer : DecoratorDrawer {

        public static readonly string extendedDecoratorClass = "kutil-extended-decorator";

        // can this be overriden?
        // public static string additionalDecoratorClass => null;

        protected VisualElement decorator;

        public virtual bool needSetupCall => true;
        public virtual bool registerUpdateCall => false;


        private PropertyField _propertyField = null;
        protected PropertyField propertyField {
            get {
                if (_propertyField == null) {
                    _propertyField = decorator?.GetFirstAncestorOfType<PropertyField>();
                    if (_propertyField == null) {
                        Debug.LogError($"{GetType().Name} failed to get decorator PropertyField! Make sure this is called after first GeometryChange");
                    }
                }
                return _propertyField;
            }
        }

        private InspectorElement _inspectorElement;
        protected InspectorElement inspectorElement {
            get {
                if (_inspectorElement == null) {
                    // get inspector element to register an onvalidate callback
                    _inspectorElement = propertyField?.GetFirstAncestorOfType<InspectorElement>();
                    if (_inspectorElement == null) {
                        Debug.LogError($"{GetType().Name} failed to get decorator InspectorElement! Make sure this is called after first GeometryChange");
                    }
                }
                return _inspectorElement;
            }
        }

        private SerializedProperty _serializedProperty;
        protected SerializedProperty serializedProperty {
            get {
                // todo manage this better?
                if (_serializedProperty == null) {
                    _serializedProperty = SerializedPropertyExtensions.GetBindedPropertyFromPropertyField(propertyField);
                }
                return _serializedProperty;
            }
        }

        private System.Reflection.FieldInfo _fieldInfo;
        protected System.Reflection.FieldInfo fieldInfo {
            get {
                if (_fieldInfo == null) {
                    _fieldInfo = serializedProperty.GetFieldInfoOnProp();
                    if (_fieldInfo == null) {
                        Debug.LogError($"{GetType().Name} failed to get decorator fieldInfo! Make sure this is called after first GeometryChange");
                    }
                }
                return _fieldInfo;
            }
        }


        public override VisualElement CreatePropertyGUI() {
            decorator = new VisualElement();
            decorator.AddToClassList(extendedDecoratorClass);
            if (needSetupCall) {
                // todo on panel attach instead?
                decorator.RegisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            }
            return decorator;
        }
        private void OnDecGeoChange(GeometryChangedEvent changedEvent) {
            decorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            Setup();
            if (registerUpdateCall) {
                propertyField.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                // this properly responds to all changes
                inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            }
        }

        void OnAttach(AttachToPanelEvent attachToPanelEvent) {

        }
        void OnDetach(DetachFromPanelEvent detachFromPanelEvent) {
            if (registerUpdateCall) {
                inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            }
            // todo properly handle property
            // property = null;
            // ClearData();
        }


        protected virtual void Setup() { }
        protected virtual void OnUpdate(SerializedPropertyChangeEvent changeEvent) { }
        // public virtual void ClearData() {
        // }
    }
}