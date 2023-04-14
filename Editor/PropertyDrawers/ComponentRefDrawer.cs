using UnityEditor;
using UnityEngine;
// using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Kutil.Ref;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kutil.Editor.PropertyDrawers;

// originally from: https://github.com/KyleBanks/scene-ref-attribute

namespace Kutil.Editor.Ref {
    /// <summary>
    /// Custom property drawer for the reference attributes.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    [CustomPropertyDrawer(typeof(GetOnSelfAttribute))]
    [CustomPropertyDrawer(typeof(GetOnChildAttribute))]
    [CustomPropertyDrawer(typeof(GetOnParentAttribute))]
    [CustomPropertyDrawer(typeof(GetInSceneAttribute))]
    // use a decorator to work on top level arrays
    public class ComponentRefAttributeDrawer : ExtendedDecoratorDrawer<ComponentRefAttributeDrawer.ComponentRefDrawerData> {

        public static readonly string componentRefPropFieldClass = "kutil-component-ref-attribute";
        public static readonly string componentRefDecoratorClass = "kutil-component-ref-decorator";
        public static readonly string componentRefHelpBoxClass = "kutil-component-ref-help-box";
        public static readonly string componentRefContainerClass = "kutil-component-ref-container";

        public override bool needSetupCall => true;
        public override bool registerUpdateCall => true;


        public class ComponentRefDrawerData : ExtendedDecoratorDrawer.ExtendedDecoratorData {
            public VisualElement refField;
            public VisualElement refFieldContainer;
            public Button refreshButton;
            public HelpBox missingRefBox;

            public bool isValidFieldType;
            public bool isArrayOrList;
            // public // bool isArray;
            // public // bool isList;
            public Type elementType;
            public Component component;
            public string typeName;
            public bool hasValue = false;

            // todo ISerializableRef support

            // todo is runtime option possible?

            public bool isSatisfied => componentRefAttribute.HasFlags(ComponentRefFlag.Optional) || hasValue;
            public ComponentRefAttribute componentRefAttribute;
        }
        ComponentRefAttribute componentRefAttribute => (ComponentRefAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            ComponentRefDrawerData data = new ComponentRefDrawerData();
            data.componentRefAttribute = componentRefAttribute;

            data.decorator = new VisualElement();
            data.decorator.name = "ComponentRefDecorator";
            data.decorator.AddToClassList(componentRefDecoratorClass);

            data.missingRefBox = new HelpBox("Missing Reference!", HelpBoxMessageType.Error);
            data.missingRefBox.AddToClassList(componentRefHelpBoxClass);
            data.missingRefBox.style.display = DisplayStyle.None;
            data.decorator.Add(data.missingRefBox);

            // base.CreatePropertyGUI();
            RegisterSetup(data);
            return data.decorator;
        }
        protected override void Setup(ComponentRefDrawerData data) {
            //     Debug.Log("s1");
            // }
            // protected override void FirstSetup() {
            //     Debug.Log("s2");

            // setup property field
            if (data.propertyField == null) {
                Debug.LogError($"{this.GetType().Name} failed to find containing property!");
                return;
            }

            data.propertyField.AddToClassList(componentRefPropFieldClass);
            if (data.propertyField.tooltip == null || data.propertyField.tooltip == "") {
                data.propertyField.tooltip = $"Reference on [{componentRefAttribute.Loc.ToString()}]";
            }
            data.propertyField.RegisterValueChangeCallback(ce => UpdateField(data));
            // data.propertyField.RegisterCallback<SerializedPropertyChangeEvent, ComponentRefDrawerData>((ce, data) => UpdateField(data), data);

            data.refField = data.propertyField;
            if (data.propertyField.childCount > 1) {
                // the field is the first child after the decorator drawers container
                data.refField = data.propertyField[1];
            }

            // create refresh button
            // refFieldContainer = new VisualElement();
            // refFieldContainer.AddToClassList(componentRefContainerClass);
            // refFieldContainer.layout = 
            // data.propertyField.Add(refFieldContainer);
            // refFieldContainer.Add(refField);
            // refreshButton = new Button();
            // refFieldContainer.Add(refreshButton);

            if (data.isArrayOrList || componentRefAttribute.HasFlags(ComponentRefFlag.Optional)) {
                // todo append to context menu instead of overriding
                //? property in attribute to control?
                data.propertyField.AddManipulator(new ContextualMenuManipulator(cmpe => {
                    cmpe.menu.AppendAction("Force Update Ref", dda => { ForceUpdateRef(data); });
                }));
            }


            if (!componentRefAttribute.HasFlags(ComponentRefFlag.Editable)) {
                // disable the property
                data.refField.SetEnabled(false);
            }
            if (componentRefAttribute.HasFlags(ComponentRefFlag.Hidden)) {
                data.refField.style.display = DisplayStyle.None;
            }



            // Debug.Log($"{Application.isPlaying} setup ");
            if (!data.HasSerializedProperty()
                || data.serializedProperty.serializedObject.targetObject is not Component) {
                return;
            }
            // Debug.Log($"setup p2");

            // setup ref

            data.component = data.serializedProperty.serializedObject.targetObject as Component;

            data.isArrayOrList = data.serializedProperty.isArray;
            // isArray = false;
            // isList = false;
            data.elementType = data.fieldInfo.FieldType;
            if (data.isArrayOrList) {
                if (data.elementType.IsArray) {
                    // isArray = true;
                    data.elementType = data.elementType.GetElementType();
                } else if (data.elementType.IsGenericType) {
                    if (data.elementType.GetGenericTypeDefinition() == typeof(List<>)
                        && data.elementType.GenericTypeArguments.Length == 1) {
                        // list
                        // isList = true;
                        data.elementType = data.elementType.GenericTypeArguments[0];
                    } else {
                        data.elementType = null;
                    }
                } else {
                    // multiparameter array?
                    data.elementType = null;
                }
                // if (typeof(ISerializableRef).IsAssignableFrom(data.elementType)) {
                //     var interfaceType = data.elementType.GetInterfaces().FirstOrDefault(type =>
                //         type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISerializableRef<>));
                //     if (interfaceType != null) {
                //         data.elementType = interfaceType.GetGenericArguments()[0];
                //     }
                // }
            }
            data.isValidFieldType = typeof(Component).IsAssignableFrom(data.elementType)
                && (data.serializedProperty.propertyType == SerializedPropertyType.ObjectReference || data.isArrayOrList);

            // string typeName = data.serializedProperty.type;
            if (data.fieldInfo == null) {
                Debug.LogError("no field info!");
                return;
            }
            data.typeName = data.fieldInfo.FieldType.Name;
            if (data.fieldInfo.FieldType.IsGenericType && data.fieldInfo.FieldType.GenericTypeArguments.Length >= 1) {
                data.typeName = data.typeName.Replace("`1", $"<{data.fieldInfo.FieldType.GenericTypeArguments[0].Name}>");
            }

            UpdateField(data);
        }


        // void OnEnable() {
        //     // EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        // }
        // void OnDisable() {
        //     // EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        // }
        // void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange) {
        // todo force references to be set before entering playmode
        //             if (playModeStateChange == PlayModeStateChange.ExitingEditMode) {
        //                 if (!isSatisfied) {
        // // EditorUtility.DisplayDialog()
        //                     EditorApplication.ExitPlaymode();
        //                 }
        //             }
        // }

        protected override void OnUpdate(SerializedPropertyChangeEvent changeEvent, ComponentRefDrawerData data) {
            // Debug.Log("test");
            if (!data.HasSerializedProperty()) return;
            if (changeEvent.changedProperty == data.serializedProperty) {
                UpdateField(data);
            }
        }

        void UpdateField(ComponentRefDrawerData data) {
            if (data.serializedProperty == null) {
                Debug.LogError($"sprop null for {data.propertyField?.name}!");
                return;
            }
            if (data.propertyField == null) {
                Debug.LogError("no property field !");
                return;
            }


            if (!data.isValidFieldType) {
                string typeName = data.fieldInfo.FieldType.Name;
                Debug.LogError($"{typeName} '{data.serializedProperty.propertyPath}' is not a Component reference!");
                return;
            }

            // Debug.Log("Updating " + data.propertyField.name);
            UpdateRef(data, data.component);
            UpdateUI(data);
        }

        void UpdateUI(ComponentRefDrawerData data) {
            // Debug.Log("update ui! " + data.propertyField.name);

            // update helpbox
            // Debug.Log($"sceneref {data.propertyFieldVE.name} has:{hasRef} opt{sceneRefAttribute.HasFlags(Flag.Optional)} val:{value?.ToString() ?? "none"}");
            data.missingRefBox.style.display = data.isSatisfied ? DisplayStyle.None : DisplayStyle.Flex;

            // update text
            data.missingRefBox.text = $"{data.serializedProperty.propertyPath} missing {data.typeName} reference on {componentRefAttribute.Loc}!";
        }

        void ForceUpdateRef(ComponentRefDrawerData data) {
            UpdateRef(data, data.component, true);
        }

        void UpdateRef(ComponentRefDrawerData data, Component component, bool forceUpdate = false) {
            // Debug.Log("update ref!" + component.name + " " + data.propertyField.name);

            data.serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            CheckHasValue(data);

            // arrays need to update more often
            bool needUpdate = false;// = data.isArrayOrList && !componentRefAttribute.HasFlags(ComponentRefFlag.Optional);

            if (!forceUpdate && data.hasValue && !needUpdate) {
                // no need to set
                return;
            }

            // Debug.Log("Updating sprop " + data.serializedProperty.propertyType);

            bool includeInactive = componentRefAttribute.HasFlags(ComponentRefFlag.IncludeInactive);
            FindObjectsInactive includeInactiveObjects = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

            Component value = null;
            Component[] values = null;
            switch (componentRefAttribute.Loc) {
                case RefLoc.Anywhere:
                    break;
                case RefLoc.Self:
                    if (data.isArrayOrList) {
                        values = component.GetComponents(data.elementType);
                    } else {
                        value = component.GetComponent(data.elementType);
                    }
                    break;
                case RefLoc.Parent:
                    if (data.isArrayOrList) {
                        values = component.GetComponentsInParent(data.elementType, includeInactive);
                    } else {
                        value = component.GetComponentInParent(data.elementType, includeInactive);
                    }
                    break;
                case RefLoc.Child:
                    if (data.isArrayOrList) {
                        values = component.GetComponentsInChildren(data.elementType, includeInactive);
                    } else {
                        value = component.GetComponentInChildren(data.elementType, includeInactive);
                    }
                    break;
                case RefLoc.Scene:
                    FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None;
                    if (data.isArrayOrList) {
                        values = GameObject.FindObjectsByType(data.elementType, includeInactiveObjects, findObjectsSortMode) as Component[];
                    } else {
                        value = GameObject.FindAnyObjectByType(data.elementType, includeInactiveObjects) as Component;
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unhandled Loc={componentRefAttribute.Loc}");
            }

            // Debug.Log($"{data.serializedProperty.propertyPath} assigning {value} or {values?.ToStringFull(v => v.name, true)}");

            if (data.isArrayOrList) {
                if (values == null || values.Length == 0) {
                    return;
                }
                if (data.serializedProperty.arraySize == values.Length) {
                    bool isSame = true;
                    for (int i = 0; i < data.serializedProperty.arraySize; i++) {
                        if (data.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue != values[i]) {
                            isSame = false;
                            break;
                        }
                    }
                    if (isSame) {
                        // same, no update
                        return;
                    }
                }
                // data.serializedProperty.ClearArray();
                data.serializedProperty.arraySize = values.Length;
                for (int i = 0; i < data.serializedProperty.arraySize; i++) {
                    data.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
            } else {
                if (value == null) {
                    return;
                }
                if (data.serializedProperty.objectReferenceValue == value) {
                    // no update
                    return;
                }
                data.serializedProperty.objectReferenceValue = value;
            }
            // data.serializedProperty.SetValue(value);
            EditorUtility.SetDirty(data.serializedProperty.serializedObject.targetObject);
            data.serializedProperty.serializedObject.ApplyModifiedProperties();

            data.hasValue = true;
        }

        private void CheckHasValue(ComponentRefDrawerData data) {
            if (data.isArrayOrList) {
                // data.serializedProperty.ClearArray();
                data.hasValue = data.serializedProperty.arraySize > 0;
                for (int i = 0; i < data.serializedProperty.arraySize; i++) {
                    // all array elements need to have a value
                    data.hasValue &= data.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue != null;
                    // if (data.hasValue) return;
                }
            } else {
                data.hasValue = data.serializedProperty.objectReferenceValue != null;
            }
        }

        void ClearRef(ComponentRefDrawerData data, Component component) {
            data.serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            CheckHasValue(data);
            if (!data.hasValue) return;

            if (data.isArrayOrList) {
                data.serializedProperty.ClearArray();
            } else {
                data.serializedProperty.objectReferenceValue = null;
            }

            EditorUtility.SetDirty(data.serializedProperty.serializedObject.targetObject);
            data.serializedProperty.serializedObject.ApplyModifiedProperties();
            data.hasValue = false;
        }


        public static bool IsEmptyOrNull(object obj, bool isArray) {
            // if (obj is ISerializableRef ser)
            //     return !ser.HasSerializedObject;

            return obj == null || obj.Equals(null) || (isArray && ((Array)obj).Length == 0);
        }

    }
}