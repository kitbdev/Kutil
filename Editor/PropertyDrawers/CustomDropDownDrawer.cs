using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kutil.PropertyDrawers {
    // todo have UI toolkit version
    /// <summary>
    /// Draws a drop down menu according to CustomDropDownData
    /// </summary>
    [CustomPropertyDrawer(typeof(CustomDropDownAttribute))]
    public class CustomDropDownDrawer : PropertyDrawer {

        [NonSerialized]
        CustomDropDownData customDropDownData;
        [NonSerialized]
        int numLines = 1;
        string lastSelValStr;
        bool rawEditModeToggle = false;
        // [NonSerialized]
        // object value;
        // [NonSerialized]
        // bool updateVal = false;

        VisualElement root;
        DropdownField dropdownField;
        PropertyField propField;

        SerializedProperty property;

        static readonly string cdddBaseClass = "kutil-custom-dropdown-drawer";
        static readonly string cdddDropDownClass = cdddBaseClass + "__dropdown";
        static readonly string cdddRawPropClass = cdddBaseClass + "__raw-property";
        static readonly string cdddRawToggleClass = cdddBaseClass + "__raw-edt-toggle";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            this.property = property;
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;
            if (dropdownAtt.dropdownDataFieldName != null) {
                customDropDownData = property.GetValueOnPropRefl<CustomDropDownData>(dropdownAtt.dropdownDataFieldName);
                if (customDropDownData == null) {
                    Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName} {property.propertyPath}");
                    return null;
                }
            } else if (dropdownAtt.choicesListSourceField != null) {
                customDropDownData = CustomDropDownData.Create<object>(
                    property.GetValueOnPropRefl<object[]>(dropdownAtt.choicesListSourceField),
                    null,
                    formatListFunc: dropdownAtt.formatListFuncField == null ? null :
                        property.GetNeighborProperty(dropdownAtt.formatListFuncField)?.GetValue<Func<string, string>>(),
                    formatSelectedValueFunc: dropdownAtt.formatSelectedValueFuncField == null ? null :
                        property.GetNeighborProperty(dropdownAtt.formatSelectedValueFuncField)?.GetValue<Func<string, string>>(),
                    includeNullChoice: dropdownAtt.includeNullChoice,
                    noElementsText: dropdownAtt.noElementsText,
                    errorText: dropdownAtt.errorText
                );
            }
            if (customDropDownData == null) {
                Debug.LogError($"Invalid CustomDropDownAttribute, no data");
                return null;
            }

            object selectedValue = property.GetValue();

            // todo bug where multiple CDDDs are being made when toggle edit button is clicked

            root = new VisualElement();
            root.name = "CustomDropDownDrawer";
            root.AddToClassList(cdddBaseClass);

            if (customDropDownData.showRawEditModeToggle) {
                // raw edit toggle property field
                propField = new PropertyField(property);
                // propField.AddToClassList(cdddRawPropClass);
                root.Add(propField);
                // bind because will be removed probably
                propField.Bind(property.serializedObject);
            }

            // dropdown
            dropdownField = new DropdownField(property.displayName);
            // dropdownField.label = property.displayName;
            root.Add(dropdownField);
            dropdownField.AddToClassList(cdddDropDownClass);
            dropdownField.AddToClassList(DropdownField.alignedFieldUssClassName);

            dropdownField.formatListItemCallback += customDropDownData.formatListFunc;
            dropdownField.formatSelectedValueCallback += customDropDownData.formatSelectedValueFunc;

            int selIndex = customDropDownData.data.ToList().FindIndex(d => d.value.Equals(selectedValue));
            // if (selIndex == -1) {
            //     // value not found
            //     // Debug.LogWarning($"Cannot find {selectedValue} n{selectedValue==null} in list {customDropDownData.data.ToStringFull(d => d.value.ToString())}");
            //     selIndex = 0;
            // }
            List<string> choicesList = customDropDownData.data.Select(d => d.name).ToList();
            if (customDropDownData.includeNullChoice) {
                // Debug.Log("null choice test");
                choicesList.Insert(0, "none");
                if (selectedValue == null) {
                    selIndex = 0;
                } else {
                    selIndex += 1;
                }
            }
            if (choicesList.Count == 0) {
                string warningText = customDropDownData.noElementsText ?? "No choices found!";
                choicesList.Add(warningText);
                selIndex = 0;
            }
            dropdownField.choices = choicesList;
            dropdownField.index = selIndex;

            dropdownField.RegisterValueChangedCallback(OnDropdownChangeValue);


            if (customDropDownData.showRawEditModeToggle) {
                UpdateDropdownEditMode();
                Toggle rawEditToggle = new Toggle();
                rawEditToggle.tooltip = "Toggle raw edit mode";
                rawEditToggle.AddToClassList(cdddRawToggleClass);

                // change style to match a button
                rawEditToggle.AddToClassList("unity-button");
                rawEditToggle.RemoveFromClassList("unity-toggle");
                rawEditToggle.label = "R";

                Label label = rawEditToggle.Q<Label>();
                label.style.minWidth = 0f;
                VisualElement toggle_input = rawEditToggle.Q(null, "unity-toggle__input");
                if (toggle_input != null) {
                    toggle_input.visible = false;
                    VisualElement checkmark = toggle_input.Q("unity-checkmark");
                    checkmark.style.width = 0f;
                }

                root.style.flexDirection = FlexDirection.Row;
                root.style.justifyContent = Justify.SpaceBetween;
                dropdownField.style.flexGrow = 1;
                propField.style.flexGrow = 1;

                rawEditToggle.viewDataKey = $"cddd raw edit toggle {property.serializedObject.targetObject.name} {property.name}";

                rawEditToggle.RegisterValueChangedCallback(ce => {
                    rawEditModeToggle = ce.newValue;
                    // Debug.Log(rawEditModeToggle + " toggled");
                    UpdateDropdownEditMode();
                });
                root.Add(rawEditToggle);
            }

            return root;
        }

        private void UpdateDropdownEditMode() {
            // todo use display instead?
            // dropdownField.style.display = !rawEditModeToggle;
            // property.style.display = rawEditModeToggle;
            if (rawEditModeToggle) {
                if (root.Contains(dropdownField)) {
                    root.Remove(dropdownField);
                }
                if (!root.Contains(propField)) {
                    root.Add(propField);
                    propField.SendToBack();
                }
            } else {
                if (root.Contains(propField)) {
                    root.Remove(propField);
                }
                if (!root.Contains(dropdownField)) {
                    root.Add(dropdownField);
                    dropdownField.SendToBack();
                }
            }
        }

        void OnDropdownChangeValue(ChangeEvent<string> changeEvent) {
            int choiceIndex = dropdownField.index;
            if (customDropDownData.includeNullChoice) {
                choiceIndex -= 1;
            }
            // Debug.Log($"set {changeEvent.previousValue} to {changeEvent.newValue} {choiceIndex}");
            if (choiceIndex >= customDropDownData.data.Length) {
                Debug.LogWarning($"index oob {choiceIndex} / {customDropDownData.data.Length}");
                return;
            }
            object value;
            if (choiceIndex == -1) {
                value = null;
            } else {
                CustomDropDownData.Data data = customDropDownData.data[choiceIndex];
                value = data.value;
            }

            property.SetValue(value);
            // Debug.Log("Set" + property.GetValue() + " to " + data.value);

            customDropDownData.onSelectCallback?.Invoke(null);

            // since we set value via reflection, also call onvalidate that way
            if (property.serializedObject.targetObject is MonoBehaviour mb) {
                // Debug.Log(" on mb " + mb);s
                // b.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
                ReflectionHelper.TryCallMethod(mb, "OnValidate", null);
            } else if (property.serializedObject.targetObject is ScriptableObject so) {
                ReflectionHelper.TryCallMethod(so, "OnValidate", null);
            }
        }



        // old imgui code



        void DrawDefGUI(Rect position, SerializedProperty property, GUIContent label) =>
            base.OnGUI(position, property, label);
        // EditorGUI.PropertyField(position, property, label, true);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // GUI.Label(position, "CustomDropDownDrawer");

            // get drop down data
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;
            // CustomDropDownData customDropDownData = null; 
            if (customDropDownData == null) {
                if (dropdownAtt.dropdownDataFieldName != null) {
                    // !todo not set up to work
                    customDropDownData = property.GetValueOnPropRefl<CustomDropDownData>(dropdownAtt.dropdownDataFieldName);
                    if (customDropDownData == null) {
                        Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName} {property.propertyPath}");
                        numLines = 2;
                        // position.height /= 2;
                        Rect labelrect = EditorGUI.IndentedRect(position);
                        string warningText = $"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName}";
                        // EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // backup textfield
                        // position.y += EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(position, property, label);
                        // DrawDefGUI(position, property, label);
                        return;
                    }
                } else {
                    if (dropdownAtt.choicesListSourceField != null) {
                        customDropDownData = CustomDropDownData.Create<object>(
                            property.GetValueOnPropRefl<object[]>(dropdownAtt.choicesListSourceField),
                            null,
                            formatListFunc: dropdownAtt.formatListFuncField == null ? null :
                                property.GetNeighborProperty(dropdownAtt.formatListFuncField)?.GetValue<Func<string, string>>(),
                            formatSelectedValueFunc: dropdownAtt.formatSelectedValueFuncField == null ? null :
                                property.GetNeighborProperty(dropdownAtt.formatSelectedValueFuncField)?.GetValue<Func<string, string>>(),
                            includeNullChoice: dropdownAtt.includeNullChoice,
                            noElementsText: dropdownAtt.noElementsText,
                            errorText: dropdownAtt.errorText
                        );
                    }
                    if (customDropDownData == null) {
                        // Debug.LogError($"Invalid choicesListSourceField {dropdownAtt.choicesListSourceField}");
                        DrawDefGUI(position, property, label);
                        return;
                    }
                }
            }

            // get selected value and str
            object selectedValue = property.GetValue();
            string selectedValueStr = GetSelectedValueStr(selectedValue);

            // array fix
            string parentPath = property.propertyPath.Replace("." + property.name, "");
            if (parentPath.EndsWith(']')) {
                // in array element name fix
                string propertyPath = property.propertyPath;
                int startIndex = propertyPath.LastIndexOf('[') + 1;
                // Debug.Log($"array {propertyPath} {startIndex} {parentPath}");
                // todo? shouldnt have to do this right?
                label.text = "Element " + propertyPath.Substring(startIndex, propertyPath.LastIndexOf(']') - startIndex);
            }

            // prop
            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                if (customDropDownData.data == null || customDropDownData.data.Length == 0) {
                    // numLines = 2;
                    // position.height /= 2;
                    Rect labelrect = EditorGUI.IndentedRect(position);
                    string warningText = label.text + ":  ";
                    if (customDropDownData.data == null) {
                        if (customDropDownData.errorText != null) {
                            warningText += customDropDownData.errorText + property.propertyPath;
                        } else {
                            warningText += $"{property.propertyPath} not found. Set choicesListSourceField to a string array!";
                        }
                    } else {
                        warningText += customDropDownData.noElementsText ?? "No choices found!";
                    }
                    // Debug.LogWarning(warningText);
                    EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                    // Debug.Log("set height: " + numLines);
                    // backup textfield
                    // position.y += EditorGUIUtility.singleLineHeight;
                    // EditorGUI.PropertyField(position, property, label);
                    return;
                }
                numLines = 1;
                Rect contentrect = EditorGUI.PrefixLabel(position, scope.content);
                Rect dropdownrect = contentrect;
                if (customDropDownData.showRawEditModeToggle) {
                    float rawEditButtonWid = 20;
                    float rawEditButtonSpacing = 2;
                    Rect rawEditButtonRect = new Rect(contentrect.xMax - rawEditButtonWid, contentrect.y, rawEditButtonWid, contentrect.height);
                    dropdownrect.width -= rawEditButtonRect.width + rawEditButtonSpacing;
                    GUIContent rawEditToggleContent = new GUIContent("R", "Toggle Raw Edit Mode");
                    rawEditModeToggle = GUI.Toggle(rawEditButtonRect, rawEditModeToggle, rawEditToggleContent, EditorStyles.miniButton);
                }
                if (rawEditModeToggle) {
                    EditorGUI.PropertyField(dropdownrect, property, GUIContent.none);
                } else {
                    // create dropdown button
                    // todo tooltip option
                    GUIContent buttonContent = new GUIContent(selectedValueStr);
                    if (EditorGUI.DropdownButton(dropdownrect, buttonContent, FocusType.Passive)) {
                        // Debug.Log("clicked");
                        GenericMenu dmenu = new GenericMenu();
                        if (customDropDownData.includeNullChoice) {//|| customDropDownData.data.Length == 0
                            bool isSet = selectedValue.Equals(null);
                            // bool isSet = selValue == null;
                            string content = "none";
                            if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                                content = customDropDownData.formatSelectedValueFunc(content);
                                if (content == null) content = "none";
                            }
                            dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new SetMenuItemEventData() {
                                property = property, value = null, action = () => {
                                    customDropDownData.onSelectCallback?.Invoke(null);
                                    customDropDownData = null;
                                }
                            });
                            dmenu.AddSeparator("");
                        }
                        for (int i = 0; i < customDropDownData.data.Length; i++) {
                            CustomDropDownData.Data data = customDropDownData.data[i];
                            object choice = data.value;
                            bool isSet = selectedValue.Equals(choice);
                            string content = customDropDownData.formatListFunc != null ? customDropDownData.formatListFunc(data.name) : data.name;
                            if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                                content = customDropDownData.formatSelectedValueFunc(content);
                            }

                            dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new SetMenuItemEventData() {
                                property = property, value = choice, action = () => {
                                    customDropDownData.onSelectCallback?.Invoke(choice);
                                    customDropDownData = null;
                                }
                            });
                        }
                        dmenu.DropDown(dropdownrect);
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // int numLines = (choices == null || choices.Count <= 0 ? 2 : 1);
            Debug.Log("height: " + numLines);
            return EditorGUIUtility.singleLineHeight * numLines;
            // return base.GetPropertyHeight(property, label);
        }

        private string GetSelectedValueStr(object selectedValue) {
            string selectedValueStr;
            if (customDropDownData.preFormatValueFunc != null) {
                selectedValueStr = customDropDownData.preFormatValueFunc(selectedValue);
            } else {
                selectedValueStr = selectedValue?.ToString() ?? "None";
            }
            if (customDropDownData.formatListFunc != null) {
                selectedValueStr = customDropDownData.formatListFunc(selectedValueStr);
            }
            if (customDropDownData.formatSelectedValueFunc != null) {
                selectedValueStr = customDropDownData.formatSelectedValueFunc(selectedValueStr);
            }
            if (lastSelValStr != selectedValueStr) {
                GUI.changed = true;
            }
            lastSelValStr = selectedValueStr;
            return selectedValueStr;
        }

        [Serializable]
        class SetMenuItemEventData {
            public SerializedProperty property;
            public object value;
            // public int index;
            public Action action;
        }
        void SetMenuItemEvent(object data) {
            // Debug.Log("set");
            var edata = (SetMenuItemEventData)data;
            // todo? object/misc field support too

            edata.property.serializedObject.Update();

            Undo.RecordObject(edata.property.serializedObject.targetObject, $"Set DropDown '{edata.value}' (by ref)");
            // if (edata.property.propertyType == SerializedPropertyType.ObjectReference) {
            //     edata.property.objectReferenceValue = (UnityEngine.Object)edata.value;
            // } else if (edata.property.propertyType == SerializedPropertyType.ManagedReference) {
            //     edata.property.managedReferenceValue = edata.value;
            // } else if (edata.property.propertyType == SerializedPropertyType.String) {
            //     edata.property.stringValue = (string)edata.value;
            // }
            bool set = edata.property.TrySetValueOnPropRefl(edata.value);
            EditorUtility.SetDirty(edata.property.serializedObject.targetObject);
            GUI.changed = true;
            edata.property.serializedObject.Update();
            // todo why does onvalidate not trigger
            var valCheck = edata.property.GetValue();
            // clickData.property.serializedObject.UpdateIfRequiredOrScript();
            // todo why does it fail?
            if (valCheck != edata.value) {
                // Debug.Log("failed set ref");
                edata.property.serializedObject.Update();
                // clickData.property.SetValue(clickData.value);
                if (edata.property.propertyType == SerializedPropertyType.Integer) {
                    edata.property.intValue = (int)edata.value;
                }//else 
                // todo other types
            }
            edata.property.serializedObject.ApplyModifiedProperties();
            edata.action?.Invoke();
            // var v3 = clickData.property.GetValue();
            // Debug.Log($"Set {(set ? "success" : "failed")} on {clickData.property.serializedObject.targetObject}.{clickData.property.propertyPath} to {clickData.value}={v3}");// = 0({v0})=1({v1})=2({v2})=3({v3})");

            // since we set value via reflection, also call onvalidate that way
            if (edata.property.serializedObject.targetObject is MonoBehaviour mb) {
                // Debug.Log(" on mb " + mb);s
                // b.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
                ReflectionHelper.TryCallMethod(mb, "OnValidate", null);
            }
        }

        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        //     // return base.CreatePropertyGUI(property);
        //     VisualElement root = new VisualElement();

        //     CustomDropDownAttribute cddAttribute = (CustomDropDownAttribute)attribute;
        //     var choices = GetChoices(cddAttribute, property);
        //     choices ??= GetChoicesRef(cddAttribute, property);
        //     if (choices == null) {
        //         root.Add(new Label((cddAttribute.errorText ??
        //                 "Set choicesListSourceField to a string array! ") + property.propertyPath));
        //         // backup string field
        //         TextField textField = new TextField(property.displayName);
        //         textField.BindProperty(property);
        //         root.Add(textField);
        //         return root;
        //     }
        //     if (choices.Count == 0) {
        //         root.Add(new Label(cddAttribute.noElementsText ?? "No choices found"));
        //         return root;
        //     }
        //     DropdownField dropdownField = new DropdownField(property.displayName, choices,
        //         property.stringValue,
        //         GetFunc(cddAttribute.formatSelectedValueFuncField, property),
        //         GetFunc(cddAttribute.formatListFuncField, property));
        //     dropdownField.BindProperty(property);
        //     root.Add(dropdownField);
        //     return root;
        // }

    }
}