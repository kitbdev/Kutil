using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// Draws a drop down menu according to CustomDropDownData
    /// </summary>
    [CustomPropertyDrawer(typeof(CustomDropDownAttribute))]
    public class CustomDropDownDrawer : PropertyDrawer {

        public static readonly string cdddBaseClass = "kutil-custom-dropdown-drawer";
        public static readonly string cdddDropDownClass = cdddBaseClass + "__dropdown";
        public static readonly string cdddRawPropClass = cdddBaseClass + "__raw-property";
        public static readonly string cdddRawToggleClass = cdddBaseClass + "__raw-edt-toggle";


        // [NonSerialized]
        // public int numLines = 1;


        class CDDDrawerData {

            [NonSerialized]
            public CustomDropDownData customDropDownData;
            // public string lastSelValStr;
            public bool rawEditModeToggle = false;
            // [NonSerialized]
            // public // object value;
            // [NonSerialized]
            // public // bool updateVal = false;

            public VisualElement root;
            public DropdownField dropdownField;
            public PropertyField propField;

            public SerializedProperty property;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            CDDDrawerData data = new();
            data.property = property;
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;
            if (dropdownAtt.dropdownDataFieldName != null) {
                data.customDropDownData = property.GetValueOnPropRefl<CustomDropDownData>(dropdownAtt.dropdownDataFieldName);
                if (data.customDropDownData == null) {
                    Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName} {property.propertyPath}");
                    return null;
                }
            } else if (dropdownAtt.choicesListSourceField != null) {
                data.customDropDownData = CustomDropDownData.Create<object>(
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
            if (data.customDropDownData == null) {
                Debug.LogError($"Invalid CustomDropDownAttribute, no data");
                return null;
            }

            object selectedValue = property.GetValue();

            // todo bug where multiple CDDDs are being made when toggle edit button is clicked

            data.root = new VisualElement();
            data.root.name = "CustomDropDownDrawer";
            data.root.AddToClassList(cdddBaseClass);

            if (data.customDropDownData.showRawEditModeToggle) {
                // raw edit toggle property field
                data.propField = new PropertyField(property);
                // propField.AddToClassList(cdddRawPropClass);
                data.root.Add(data.propField);
                // bind because will be removed probably
                data.propField.Bind(property.serializedObject);
            }

            // dropdown
            data.dropdownField = new DropdownField(property.displayName);
            // dropdownField.label = property.displayName;
            data.root.Add(data.dropdownField);
            data.dropdownField.AddToClassList(cdddDropDownClass);
            data.dropdownField.AddToClassList(DropdownField.alignedFieldUssClassName);

            data.dropdownField.formatListItemCallback += data.customDropDownData.formatListFunc;
            data.dropdownField.formatSelectedValueCallback += data.customDropDownData.formatSelectedValueFunc;

            int selIndex = data.customDropDownData.data.ToList().FindIndex(d => d.value.Equals(selectedValue));
            // if (selIndex == -1) {
            //     // value not found
            //     // Debug.LogWarning($"Cannot find {selectedValue} n{selectedValue==null} in list {customDropDownData.data.ToStringFull(d => d.value.ToString())}");
            //     selIndex = 0;
            // }
            List<string> choicesList = data.customDropDownData.data.Select(d => d.name).ToList();
            if (data.customDropDownData.includeNullChoice) {
                // Debug.Log("null choice test");
                choicesList.Insert(0, "none");
                if (selectedValue == null) {
                    selIndex = 0;
                } else {
                    selIndex += 1;
                }
            }
            if (choicesList.Count == 0) {
                string warningText = data.customDropDownData.noElementsText ?? "No choices found!";
                choicesList.Add(warningText);
                selIndex = 0;
            }
            data.dropdownField.choices = choicesList;
            data.dropdownField.index = selIndex;

            // data.dropdownField.RegisterValueChangedCallback(OnDropdownChangeValue, data);
            data.dropdownField.RegisterCallback<ChangeEvent<string>, CDDDrawerData>(OnDropdownChangeValue, data);


            if (data.customDropDownData.showRawEditModeToggle) {
                UpdateDropdownEditMode(data);
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

                data.root.style.flexDirection = FlexDirection.Row;
                data.root.style.justifyContent = Justify.SpaceBetween;
                data.dropdownField.style.flexGrow = 1;
                data.propField.style.flexGrow = 1;

                rawEditToggle.viewDataKey = $"cddd raw edit toggle {property.serializedObject.targetObject.name} {property.name}";

                rawEditToggle.RegisterValueChangedCallback(ce => {
                    data.rawEditModeToggle = ce.newValue;
                    // Debug.Log(rawEditModeToggle + " toggled");
                    UpdateDropdownEditMode(data);
                });
                data.root.Add(rawEditToggle);
            }

            return data.root;
        }

        private void UpdateDropdownEditMode(CDDDrawerData data) {
            // todo use display instead?
            // dropdownField.style.display = !rawEditModeToggle;
            // property.style.display = rawEditModeToggle;
            if (data.rawEditModeToggle) {
                if (data.root.Contains(data.dropdownField)) {
                    data.root.Remove(data.dropdownField);
                }
                if (!data.root.Contains(data.propField)) {
                    data.root.Add(data.propField);
                    data.propField.SendToBack();
                }
            } else {
                if (data.root.Contains(data.propField)) {
                    data.root.Remove(data.propField);
                }
                if (!data.root.Contains(data.dropdownField)) {
                    data.root.Add(data.dropdownField);
                    data.dropdownField.SendToBack();
                }
            }
        }

        void OnDropdownChangeValue(ChangeEvent<string> changeEvent, CDDDrawerData data) {
            int choiceIndex = data.dropdownField.index;
            if (data.customDropDownData.includeNullChoice) {
                choiceIndex -= 1;
            }
            // Debug.Log($"set {changeEvent.previousValue} to {changeEvent.newValue} {choiceIndex}");
            if (choiceIndex >= data.customDropDownData.data.Length) {
                Debug.LogWarning($"index oob {choiceIndex} / {data.customDropDownData.data.Length}");
                return;
            }
            object value;
            if (choiceIndex == -1) {
                value = null;
            } else {
                CustomDropDownData.Data cdata = data.customDropDownData.data[choiceIndex];
                value = cdata.value;
            }

            data.property.SetValue(value);
            // Debug.Log("Set" + property.GetValue() + " to " + data.value);

            data.customDropDownData.onSelectCallback?.Invoke(null);

            // since we set value via reflection, also call onvalidate that way
            if (data.property.serializedObject.targetObject is MonoBehaviour mb) {
                // Debug.Log(" on mb " + mb);s
                // b.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
                ReflectionHelper.TryCallMethod(mb, "OnValidate", null);
            } else if (data.property.serializedObject.targetObject is ScriptableObject so) {
                ReflectionHelper.TryCallMethod(so, "OnValidate", null);
            }
        }



        // old imgui code



        // void DrawDefGUI(Rect position, SerializedProperty property, GUIContent label) =>
        //     base.OnGUI(position, property, label);
        // // EditorGUI.PropertyField(position, property, label, true);
        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        //     // GUI.Label(position, "CustomDropDownDrawer");
        //     CDDDrawerData data = new();

        //     // get drop down data
        //     CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;
        //     // CustomDropDownData customDropDownData = null; 
        //     if (data.customDropDownData == null) {
        //         if (dropdownAtt.dropdownDataFieldName != null) {
        //             // !todo not set up to work
        //             data.customDropDownData = property.GetValueOnPropRefl<CustomDropDownData>(dropdownAtt.dropdownDataFieldName);
        //             if (data.customDropDownData == null) {
        //                 Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName} {property.propertyPath}");
        //                 numLines = 2;
        //                 // position.height /= 2;
        //                 Rect labelrect = EditorGUI.IndentedRect(position);
        //                 string warningText = $"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName}";
        //                 // EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
        //                 // backup textfield
        //                 // position.y += EditorGUIUtility.singleLineHeight;
        //                 EditorGUI.PropertyField(position, property, label);
        //                 // DrawDefGUI(position, property, label);
        //                 return;
        //             }
        //         } else {
        //             if (dropdownAtt.choicesListSourceField != null) {
        //                 data.customDropDownData = CustomDropDownData.Create<object>(
        //                     property.GetValueOnPropRefl<object[]>(dropdownAtt.choicesListSourceField),
        //                     null,
        //                     formatListFunc: dropdownAtt.formatListFuncField == null ? null :
        //                         property.GetNeighborProperty(dropdownAtt.formatListFuncField)?.GetValue<Func<string, string>>(),
        //                     formatSelectedValueFunc: dropdownAtt.formatSelectedValueFuncField == null ? null :
        //                         property.GetNeighborProperty(dropdownAtt.formatSelectedValueFuncField)?.GetValue<Func<string, string>>(),
        //                     includeNullChoice: dropdownAtt.includeNullChoice,
        //                     noElementsText: dropdownAtt.noElementsText,
        //                     errorText: dropdownAtt.errorText
        //                 );
        //             }
        //             if (data.customDropDownData == null) {
        //                 // Debug.LogError($"Invalid choicesListSourceField {dropdownAtt.choicesListSourceField}");
        //                 DrawDefGUI(position, property, label);
        //                 return;
        //             }
        //         }
        //     }

        //     // get selected value and str
        //     object selectedValue = property.GetValue();
        //     string selectedValueStr = GetSelectedValueStr(selectedValue);

        //     // array fix
        //     string parentPath = property.propertyPath.Replace("." + property.name, "");
        //     if (parentPath.EndsWith(']')) {
        //         // in array element name fix
        //         string propertyPath = property.propertyPath;
        //         int startIndex = propertyPath.LastIndexOf('[') + 1;
        //         // Debug.Log($"array {propertyPath} {startIndex} {parentPath}");
        //         // todo? shouldnt have to do this right?
        //         label.text = "Element " + propertyPath.Substring(startIndex, propertyPath.LastIndexOf(']') - startIndex);
        //     }

        //     // prop
        //     using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
        //         if (data.customDropDownData.data == null || data.customDropDownData.data.Length == 0) {
        //             // numLines = 2;
        //             // position.height /= 2;
        //             Rect labelrect = EditorGUI.IndentedRect(position);
        //             string warningText = label.text + ":  ";
        //             if (data.customDropDownData.data == null) {
        //                 if (data.customDropDownData.errorText != null) {
        //                     warningText += data.customDropDownData.errorText + property.propertyPath;
        //                 } else {
        //                     warningText += $"{property.propertyPath} not found. Set choicesListSourceField to a string array!";
        //                 }
        //             } else {
        //                 warningText += data.customDropDownData.noElementsText ?? "No choices found!";
        //             }
        //             // Debug.LogWarning(warningText);
        //             EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
        //             // Debug.Log("set height: " + numLines);
        //             // backup textfield
        //             // position.y += EditorGUIUtility.singleLineHeight;
        //             // EditorGUI.PropertyField(position, property, label);
        //             return;
        //         }
        //         numLines = 1;
        //         Rect contentrect = EditorGUI.PrefixLabel(position, scope.content);
        //         Rect dropdownrect = contentrect;
        //         if (data.customDropDownData.showRawEditModeToggle) {
        //             float rawEditButtonWid = 20;
        //             float rawEditButtonSpacing = 2;
        //             Rect rawEditButtonRect = new Rect(contentrect.xMax - rawEditButtonWid, contentrect.y, rawEditButtonWid, contentrect.height);
        //             dropdownrect.width -= rawEditButtonRect.width + rawEditButtonSpacing;
        //             GUIContent rawEditToggleContent = new GUIContent("R", "Toggle Raw Edit Mode");
        //             data.rawEditModeToggle = GUI.Toggle(rawEditButtonRect, data.rawEditModeToggle, rawEditToggleContent, EditorStyles.miniButton);
        //         }
        //         if (data.rawEditModeToggle) {
        //             EditorGUI.PropertyField(dropdownrect, property, GUIContent.none);
        //         } else {
        //             // create dropdown button
        //             // todo tooltip option
        //             GUIContent buttonContent = new GUIContent(selectedValueStr);
        //             if (EditorGUI.DropdownButton(dropdownrect, buttonContent, FocusType.Passive)) {
        //                 // Debug.Log("clicked");
        //                 GenericMenu dmenu = new GenericMenu();
        //                 if (data.customDropDownData.includeNullChoice) {//|| customDropDownData.data.Length == 0
        //                     bool isSet = selectedValue.Equals(null);
        //                     // bool isSet = selValue == null;
        //                     string content = "none";
        //                     if (isSet && data.customDropDownData.formatSelectedValueFunc != null) {
        //                         content = data.customDropDownData.formatSelectedValueFunc(content);
        //                         if (content == null) content = "none";
        //                     }
        //                     dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new SetMenuItemEventData() {
        //                         property = property, value = null, action = () => {
        //                             data.customDropDownData.onSelectCallback?.Invoke(null);
        //                             data.customDropDownData = null;
        //                         }
        //                     });
        //                     dmenu.AddSeparator("");
        //                 }
        //                 for (int i = 0; i < data.customDropDownData.data.Length; i++) {
        //                     CustomDropDownData.Data data = data.customDropDownData.data[i];
        //                     object choice = data.value;
        //                     bool isSet = selectedValue.Equals(choice);
        //                     string content = data.customDropDownData.formatListFunc != null ? data.customDropDownData.formatListFunc(data.name) : data.name;
        //                     if (isSet && data.customDropDownData.formatSelectedValueFunc != null) {
        //                         content = data.customDropDownData.formatSelectedValueFunc(content);
        //                     }

        //                     dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new SetMenuItemEventData() {
        //                         property = property, value = choice, action = () => {
        //                             data.customDropDownData.onSelectCallback?.Invoke(choice);
        //                             data.customDropDownData = null;
        //                         }
        //                     });
        //                 }
        //                 dmenu.DropDown(dropdownrect);
        //             }
        //         }
        //     }
        // }

        // public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        //     // int numLines = (choices == null || choices.Count <= 0 ? 2 : 1);
        //     Debug.Log("height: " + numLines);
        //     return EditorGUIUtility.singleLineHeight * numLines;
        //     // return base.GetPropertyHeight(property, label);
        // }

        // private string GetSelectedValueStr(object selectedValue) {
        //     string selectedValueStr;
        //     if (data.customDropDownData.preFormatValueFunc != null) {
        //         selectedValueStr = data.customDropDownData.preFormatValueFunc(selectedValue);
        //     } else {
        //         selectedValueStr = selectedValue?.ToString() ?? "None";
        //     }
        //     if (data.customDropDownData.formatListFunc != null) {
        //         selectedValueStr = data.customDropDownData.formatListFunc(selectedValueStr);
        //     }
        //     if (data.customDropDownData.formatSelectedValueFunc != null) {
        //         selectedValueStr = data.customDropDownData.formatSelectedValueFunc(selectedValueStr);
        //     }
        //     if (data.lastSelValStr != selectedValueStr) {
        //         GUI.changed = true;
        //     }
        //     data.lastSelValStr = selectedValueStr;
        //     return selectedValueStr;
        // }

        // [Serializable]
        // class SetMenuItemEventData {
        //     public SerializedProperty property;
        //     public object value;
        //     // public int index;
        //     public Action action;
        // }
        // void SetMenuItemEvent(object data) {
        //     // Debug.Log("set");
        //     var edata = (SetMenuItemEventData)data;
        //     // todo? object/misc field support too

        //     edata.property.serializedObject.Update();

        //     Undo.RecordObject(edata.property.serializedObject.targetObject, $"Set DropDown '{edata.value}' (by ref)");
        //     // if (edata.property.propertyType == SerializedPropertyType.ObjectReference) {
        //     //     edata.property.objectReferenceValue = (UnityEngine.Object)edata.value;
        //     // } else if (edata.property.propertyType == SerializedPropertyType.ManagedReference) {
        //     //     edata.property.managedReferenceValue = edata.value;
        //     // } else if (edata.property.propertyType == SerializedPropertyType.String) {
        //     //     edata.property.stringValue = (string)edata.value;
        //     // }
        //     bool set = edata.property.TrySetValueOnPropRefl(edata.value);
        //     EditorUtility.SetDirty(edata.property.serializedObject.targetObject);
        //     GUI.changed = true;
        //     edata.property.serializedObject.Update();
        //     // todo why does onvalidate not trigger
        //     var valCheck = edata.property.GetValue();
        //     // clickData.property.serializedObject.UpdateIfRequiredOrScript();
        //     // todo why does it fail?
        //     if (valCheck != edata.value) {
        //         // Debug.Log("failed set ref");
        //         edata.property.serializedObject.Update();
        //         // clickData.property.SetValue(clickData.value);
        //         if (edata.property.propertyType == SerializedPropertyType.Integer) {
        //             edata.property.intValue = (int)edata.value;
        //         }//else 
        //         // todo other types
        //     }
        //     edata.property.serializedObject.ApplyModifiedProperties();
        //     edata.action?.Invoke();
        //     // var v3 = clickData.property.GetValue();
        //     // Debug.Log($"Set {(set ? "success" : "failed")} on {clickData.property.serializedObject.targetObject}.{clickData.property.propertyPath} to {clickData.value}={v3}");// = 0({v0})=1({v1})=2({v2})=3({v3})");

        //     // since we set value via reflection, also call onvalidate that way
        //     if (edata.property.serializedObject.targetObject is MonoBehaviour mb) {
        //         // Debug.Log(" on mb " + mb);s
        //         // b.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
        //         ReflectionHelper.TryCallMethod(mb, "OnValidate", null);
        //     }
        // }

    }
}