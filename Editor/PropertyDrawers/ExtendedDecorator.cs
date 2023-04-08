using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// DecoratorDrawer with some additional features.
    /// Accessors for PropertyField, SerializedProperty, and FieldInfo.
    /// setup and update method callbacks
    /// default empty decorator created. 
    /// </summary>
    public abstract class ExtendedDecoratorDrawer : DecoratorDrawer {

        public static readonly string extendedDecoratorClass = "kutil-extended-decorator";

        protected virtual string decoratorName => null;
        protected virtual string decoratorClass => null;
        // protected virtual string propertyFieldClass => null;

        protected VisualElement decorator;

        public virtual bool needSetupCall => true;
        public virtual bool registerUpdateCall => false;

        private bool didFirstSetup = false;


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
                    if (_serializedProperty == null) {
                        Debug.LogError($"{GetType().Name} failed to get decorator SerializedProperty! Make sure this is called after first GeometryChange");
                    }
                }
                return _serializedProperty;
            }
        }
        /// <summary>
        /// Is the serialized property currently valid?
        /// Call this instead of checking serializedProperty == null
        /// </summary>
        /// <returns>true if the serialized property is valid</returns>
        protected bool HasSerializedProperty() {
            // try {
            //     _ = _serializedProperty;
            // } catch (System.Exception) {
            //     _serializedProperty = null;
            // }
            if (_serializedProperty != null && serializedProperty.serializedObject.targetObject == null) {
                _serializedProperty = null;
            }
            if (_serializedProperty == null) {
                // _inspectorElement = null;
                // todo sometimes this will fail, not being able to find inspector element
                // especially when updating scripts or entering playmode
                // deselecting and reselecting makes it work

                // Debug.Log($"insp: '{inspectorElement?.name ?? "null"}'");
                _serializedProperty = SerializedPropertyExtensions.GetBindedPropertyFromPropertyField(propertyField, false);
            }
            return _serializedProperty != null;
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
            _serializedProperty = null;
            decorator = new VisualElement();
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

        protected void RegisterSetup() {
            _serializedProperty = null;
            if (decorator == null) {
                Debug.LogError($"{GetType().Name} decorator must not be null to Setup!");
                return;
            }
            didFirstSetup = false;
            // todo on panel attach instead?
            decorator.RegisterCallback<AttachToPanelEvent>(OnAttach);
            decorator.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            // decorator.RegisterCallback<GeometryChangedEvent>(OnDecGeoChange);
        }

        private void OnDecGeoChange(GeometryChangedEvent changedEvent) {
            // _serializedProperty = null;
            // decorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            // Debug.Log("geo changed");
            // Setup();
            // if (registerUpdateCall) {
            //     propertyField.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            //     // this properly responds to all changes
            //     inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            // }
        }

        protected virtual void OnAttach(AttachToPanelEvent attachToPanelEvent) {
            _serializedProperty = null;
            if (!didFirstSetup) {
                didFirstSetup = true;
                // decorator.UnregisterCallback<AttachToPanelEvent>(OnAttach);
                // propertyField.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                if (registerUpdateCall) {
                    // this properly responds to all changes
                    inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
                }

                //     FirstSetup();
                Setup();
            }
            // Setup();
        }
        protected virtual void OnDetach(DetachFromPanelEvent detachFromPanelEvent) {
            if (registerUpdateCall) {
                inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            }
            _serializedProperty = null;
            // OnDetach();
        }


        // protected virtual void FirstSetup() { }
        protected virtual void Setup() { }
        protected virtual void OnUpdate(SerializedPropertyChangeEvent changeEvent) { }
        // protected virtual void OnDetach() { }
    }
}