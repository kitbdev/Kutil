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
    public abstract class ExtendedDecoratorDrawer : ExtendedDecoratorDrawer<ExtendedDecoratorDrawer.ExtendedDecoratorData> {

        public class ExtendedDecoratorData {

            public VisualElement decorator;
            public bool didFirstSetup = false;


            public PropertyField _propertyField = null;
            public PropertyField propertyField {
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

            public InspectorElement _inspectorElement;
            public InspectorElement inspectorElement {
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

            public SerializedProperty _serializedProperty;
            public SerializedProperty serializedProperty {
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
            public bool HasSerializedProperty() {
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

            public System.Reflection.FieldInfo _fieldInfo;
            public System.Reflection.FieldInfo fieldInfo {
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

            public object customData;

        }
    }
    public abstract class ExtendedDecoratorDrawer<TData> : DecoratorDrawer where TData : ExtendedDecoratorDrawer.ExtendedDecoratorData, new() {

        public static readonly string extendedDecoratorClass = "kutil-extended-decorator";



        protected virtual string decoratorName => null;
        protected virtual string decoratorClass => null;
        // protected virtual string propertyFieldClass => null;


        public virtual bool needSetupCall => true;
        public virtual bool registerUpdateCall => false;




        public override VisualElement CreatePropertyGUI() {
            TData data = new TData();
            data.decorator = new VisualElement();
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

        protected void RegisterSetup(TData data) {
            data._serializedProperty = null;
            if (data.decorator == null) {
                Debug.LogError($"{GetType().Name} decorator must not be null to Setup!");
                return;
            }
            data.didFirstSetup = false;
            // todo on panel attach instead?
            data.decorator.RegisterCallback<AttachToPanelEvent, TData>(OnAttach, data);
            data.decorator.RegisterCallback<DetachFromPanelEvent, TData>(OnDetach, data);
            // decorator.RegisterCallback<GeometryChangedEvent,TData>(OnDecGeoChange);
        }

        // private void OnDecGeoChange(GeometryChangedEvent changedEvent,TData data) {
        // _serializedProperty = null;
        // decorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
        // Debug.Log("geo changed");
        // Setup();
        // if (registerUpdateCall) {
        //     propertyField.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        //     // this properly responds to all changes
        //     inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
        // }
        // }

        protected virtual void OnAttach(AttachToPanelEvent attachToPanelEvent, TData data) {
            data._serializedProperty = null;
            if (!data.didFirstSetup) {
                data.didFirstSetup = true;
                // decorator.UnregisterCallback<AttachToPanelEvent>(OnAttach);
                // propertyField.RegisterCallback<DetachFromPanelEvent>(OnDetach);
                if (registerUpdateCall) {
                    // this properly responds to all changes
                    data.inspectorElement.RegisterCallback<SerializedPropertyChangeEvent, TData>(OnUpdate, data);
                }

                //     FirstSetup();
                Setup(data);
            }
            // Setup();
        }
        protected virtual void OnDetach(DetachFromPanelEvent detachFromPanelEvent, TData data) {
            if (registerUpdateCall) {
                data.inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent, TData>(OnUpdate);
            }
            data._serializedProperty = null;
            // OnDetach();
        }


        // protected virtual void FirstSetup() { }
        protected virtual void Setup(TData data) { }
        protected virtual void OnUpdate(SerializedPropertyChangeEvent changeEvent, TData data) { }
        // protected virtual void OnDetach() { }
    }
}