using UnityEditor;
using UnityEngine;
// using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using Kutil.Ref;
using Kutil.PropertyDrawers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// originally from: https://github.com/KyleBanks/scene-ref-attribute

namespace Kutil.Ref {
    /// <summary>
    /// Custom property drawer for the reference attributes.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    [CustomPropertyDrawer(typeof(GetOnSelfAttribute))]
    [CustomPropertyDrawer(typeof(GetOnChildAttribute))]
    [CustomPropertyDrawer(typeof(GetOnParentAttribute))]
    [CustomPropertyDrawer(typeof(GetInSceneAttribute))]
    // use a decorator to work on top level arrays
    public class ComponentRefAttributePropertyDrawer : ExtendedDecoratorDrawer {

        public static readonly string componentRefPropFieldClass = "component-ref-attribute";
        public static readonly string componentRefDecoratorClass = "component-ref-decorator";
        public static readonly string componentRefHelpBoxClass = "component-ref-help-box";

        public override bool needSetupCall => true;
        public override bool registerUpdateCall => true;

        VisualElement propertyFieldVE;
        HelpBox missingRefBox;

        bool isValidFieldType;
        bool isArrayOrList;
        // bool isArray;
        // bool isList;
        Type elementType;
        Component component;
        string typeName;
        bool hasValue = false;

        // todo ISerializableRef support

        bool isSatisfied => componentRefAttribute.HasFlags(ComponentRefFlag.Optional) || hasValue;

        ComponentRefAttribute componentRefAttribute => (ComponentRefAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {

            decorator = new VisualElement();
            decorator.name = "ComponentRefDecorator";
            decorator.AddToClassList(componentRefDecoratorClass);

            missingRefBox = new HelpBox("Missing Reference!", HelpBoxMessageType.Error);
            missingRefBox.AddToClassList(componentRefHelpBoxClass);
            missingRefBox.style.display = DisplayStyle.None;
            decorator.Add(missingRefBox);

            // base.CreatePropertyGUI();
            RegisterSetup();
            return decorator;
        }
        protected override void Setup() {
            if (!HasSerializedProperty()
                || serializedProperty.serializedObject.targetObject is not Component) {
                return;
            }

            // get the property field, as decorators dont have access by default
            PropertyField propertyField = decorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"{this.GetType().Name} failed to find containing property!");
                return;
            }
            propertyField.AddToClassList(componentRefPropFieldClass);
            if (propertyField.tooltip == null || propertyField.tooltip == "") {
                propertyField.tooltip = $"Reference on [{componentRefAttribute.Loc.ToString()}]";
            }
            propertyField.RegisterValueChangeCallback(ce => UpdateField());

            propertyFieldVE = propertyField;
            if (propertyField.childCount > 1) {
                // the field is the first child after the decorator drawers container
                propertyFieldVE = propertyField[1];
            }

            if (!componentRefAttribute.HasFlags(ComponentRefFlag.Editable)) {
                // disable the property
                propertyFieldVE.SetEnabled(false);
            }
            if (componentRefAttribute.HasFlags(ComponentRefFlag.Hidden)) {
                propertyFieldVE.style.display = DisplayStyle.None;
            }

            // setup ref

            component = serializedProperty.serializedObject.targetObject as Component;

            isArrayOrList = serializedProperty.isArray;
            // isArray = false;
            // isList = false;
            elementType = fieldInfo.FieldType;
            if (isArrayOrList) {
                if (elementType.IsArray) {
                    // isArray = true;
                    elementType = elementType.GetElementType();
                } else if (elementType.IsGenericType) {
                    if (elementType.GetGenericTypeDefinition() == typeof(List<>)
                        && elementType.GenericTypeArguments.Length == 1) {
                        // list
                        // isList = true;
                        elementType = elementType.GenericTypeArguments[0];
                    } else {
                        elementType = null;
                    }
                } else {
                    // multiparameter array?
                    elementType = null;
                }
                // if (typeof(ISerializableRef).IsAssignableFrom(elementType)) {
                //     var interfaceType = elementType.GetInterfaces().FirstOrDefault(type =>
                //         type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISerializableRef<>));
                //     if (interfaceType != null) {
                //         elementType = interfaceType.GetGenericArguments()[0];
                //     }
                // }
            }
            isValidFieldType = typeof(Component).IsAssignableFrom(elementType)
                && (serializedProperty.propertyType == SerializedPropertyType.ObjectReference || isArrayOrList);

            // string typeName = serializedProperty.type;
            if (fieldInfo == null) {
                Debug.LogError("no field info!");
                return;
            }
            typeName = fieldInfo.FieldType.Name;
            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GenericTypeArguments.Length >= 1) {
                typeName = typeName.Replace("`1", $"<{fieldInfo.FieldType.GenericTypeArguments[0].Name}>");
            }

            UpdateField();
        }


        void OnEnable() {
            // EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        }
        void OnDisable() {
            // EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        }
        void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange) {
            // todo force references to be set before entering playmode
            //             if (playModeStateChange == PlayModeStateChange.ExitingEditMode) {
            //                 if (!isSatisfied) {
            // // EditorUtility.DisplayDialog()
            //                     EditorApplication.ExitPlaymode();
            //                 }
            //             }
        }

        protected override void OnUpdate(SerializedPropertyChangeEvent changeEvent) {
            // Debug.Log("test");
            if (!HasSerializedProperty()) return;
            if (changeEvent.changedProperty == serializedProperty) {
                UpdateField();
            }
        }

        void UpdateField() {
            if (serializedProperty == null) {
                Debug.LogError($"sprop null for {propertyField?.name}!");
                return;
            }
            if (propertyField == null) {
                Debug.LogError("no property field !");
                return;
            }


            if (!isValidFieldType) {
                string typeName = fieldInfo.FieldType.Name;
                Debug.LogError($"{typeName} '{serializedProperty.propertyPath}' is not a Component reference!");
                return;
            }

            // Debug.Log("Updating " + propertyField.name);
            UpdateRef(component);
            UpdateUI();
        }

        void UpdateUI() {
            // Debug.Log("update ui! " + propertyField.name);

            // update helpbox
            // Debug.Log($"sceneref {propertyFieldVE.name} has:{hasRef} opt{sceneRefAttribute.HasFlags(Flag.Optional)} val:{value?.ToString() ?? "none"}");
            missingRefBox.style.display = isSatisfied ? DisplayStyle.None : DisplayStyle.Flex;

            // update text
            missingRefBox.text = $"{serializedProperty.propertyPath} missing {typeName} reference on {componentRefAttribute.Loc}!";
        }

        void UpdateRef(Component component) {
            // Debug.Log("update ref!" + component.name + " " + propertyField.name);

            serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            CheckHasValue();

            if (hasValue) {
                // no need to set
                return;
            }

            // Debug.Log("Updating sprop " + serializedProperty.propertyType);

            bool includeInactive = componentRefAttribute.HasFlags(ComponentRefFlag.IncludeInactive);
            FindObjectsInactive includeInactiveObjects = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

            Component value = null;
            Component[] values = null;
            switch (componentRefAttribute.Loc) {
                case RefLoc.Anywhere:
                    break;
                case RefLoc.Self:
                    if (isArrayOrList) {
                        values = component.GetComponents(elementType);
                    } else {
                        value = component.GetComponent(elementType);
                    }
                    break;
                case RefLoc.Parent:
                    if (isArrayOrList) {
                        values = component.GetComponentsInParent(elementType, includeInactive);
                    } else {
                        value = component.GetComponentInParent(elementType, includeInactive);
                    }
                    break;
                case RefLoc.Child:
                    if (isArrayOrList) {
                        values = component.GetComponentsInChildren(elementType, includeInactive);
                    } else {
                        value = component.GetComponentInChildren(elementType, includeInactive);
                    }
                    break;
                case RefLoc.Scene:
                    FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None;
                    if (isArrayOrList) {
                        values = GameObject.FindObjectsByType(elementType, includeInactiveObjects, findObjectsSortMode) as Component[];
                    } else {
                        value = GameObject.FindAnyObjectByType(elementType, includeInactiveObjects) as Component;
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unhandled Loc={componentRefAttribute.Loc}");
            }

            // Debug.Log($"{serializedProperty.propertyPath} assigning {value} or {values?.ToStringFull(v => v.name, true)}");

            if (isArrayOrList) {
                if (values == null || values.Length == 0) {
                    return;
                }
                // serializedProperty.ClearArray();
                serializedProperty.arraySize = values.Length;
                for (int i = 0; i < serializedProperty.arraySize; i++) {
                    serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
            } else {
                if (value == null) {
                    return;
                }
                serializedProperty.objectReferenceValue = value;
            }
            // serializedProperty.SetValue(value);
            EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
            serializedProperty.serializedObject.ApplyModifiedProperties();

            hasValue = true;
        }

        private void CheckHasValue() {
            if (isArrayOrList) {
                // serializedProperty.ClearArray();
                hasValue = serializedProperty.arraySize > 0;
                for (int i = 0; i < serializedProperty.arraySize; i++) {
                    // all array elements need to have a value
                    hasValue &= serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue != null;
                    // if (hasValue) return;
                }
            } else {
                hasValue = serializedProperty.objectReferenceValue != null;
            }
        }

        void ClearRef(Component component) {
            serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            CheckHasValue();
            if (!hasValue) return;

            if (isArrayOrList) {
                serializedProperty.ClearArray();
            } else {
                serializedProperty.objectReferenceValue = null;
            }

            EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
            serializedProperty.serializedObject.ApplyModifiedProperties();
            hasValue = false;
        }


        public static bool IsEmptyOrNull(object obj, bool isArray) {
            // if (obj is ISerializableRef ser)
            //     return !ser.HasSerializedObject;

            return obj == null || obj.Equals(null) || (isArray && ((Array)obj).Length == 0);
        }

    }
}