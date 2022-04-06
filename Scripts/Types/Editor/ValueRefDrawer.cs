using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(ValueRef<>))]
    public class ValueRefDrawer : PropertyDrawer {
        private const string kTargetPath = "target";
        private const string kFieldName = "fieldName";
        private const string kFieldType = "fieldType";

        // public bool hi;
        // private bool hi2;
        // protected bool hi3 = true;
        // public bool hi4 => hi2 || hi3;
        // public static bool hi5;
        // private bool hi6;
        // const bool hi7 = false;
        // readonly bool hi8 = true;
        // [SerializeField] bool hi9;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label);
            position.y += EditorGUIUtility.singleLineHeight;

            var targetProp = property.FindPropertyRelative(kTargetPath);
            var fieldNameProp = property.FindPropertyRelative(kFieldName);
            var fieldTypeProp = property.FindPropertyRelative(kFieldType);
            // EditorGUI.LabelField(position, label);

            var leftrect = position;
            int spacing = 10;
            leftrect.x += spacing;
            leftrect.width /= 2;
            leftrect.width -= spacing;
            var dropdownrect = leftrect;
            dropdownrect.x += leftrect.width + spacing;

            EditorGUI.BeginChangeCheck();
            var desiredType = typeof(Object);
            var result = EditorGUI.ObjectField(leftrect, GUIContent.none, targetProp.objectReferenceValue, desiredType, true);
            // EditorGUI.PropertyField(leftrect, targetProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) {
                if (targetProp.objectReferenceValue != result) {
                    targetProp.objectReferenceValue = result;
                    ClearEvent(property);
                }
            }
            // property.Next(true);
            using (new EditorGUI.DisabledScope(targetProp.objectReferenceValue == null)) {
                EditorGUI.BeginProperty(dropdownrect, GUIContent.none, fieldNameProp);
                // EditorGUILayout.PropertyField(fieldNameProp);
                GUIContent buttonContent;
                // if (EditorGUI.showMixedValue)
                // {
                //     buttonContent = default; //.showMixedValue;//.mixedValueContent;
                // }
                System.Type valueType = fieldInfo.FieldType.GetGenericArguments()[0];
                string TtypeString = fieldTypeProp?.type ?? "?";

                bool validProp = !string.IsNullOrEmpty(fieldNameProp.stringValue) && targetProp.objectReferenceValue != null;
                string buttonLabel = "";
                if (!validProp) {
                    buttonLabel += "None";
                    buttonLabel += " (" + TtypeString + ")";
                } else {
                    buttonLabel += targetProp.objectReferenceValue.GetType().Name;
                    buttonLabel += ".";
                    MemberInfo[] memberInfos = targetProp.objectReferenceValue.GetType().GetMember(fieldNameProp.stringValue, ValueRef.flags);
                    if (memberInfos.Length > 0 && memberInfos[0] != null) {
                        MemberTypes memtype = memberInfos[0].MemberType;
                        buttonLabel += GetPath(new FieldMap(targetProp.objectReferenceValue, fieldNameProp.stringValue, memtype));
                    } else {
                        buttonLabel += fieldNameProp.stringValue;
                    }
                }
                buttonContent = new GUIContent(buttonLabel);
                if (GUI.Button(dropdownrect, buttonContent, EditorStyles.popup)) {
                    var targetToUse = targetProp.objectReferenceValue;
                    if (targetToUse is Component) {
                        targetToUse = (targetToUse as Component).gameObject;
                    }

                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), !validProp, ClearEvent, property);
                    if (targetToUse != null) {
                        menu.AddSeparator("");
                        // todo GameObject fields dont show
                        if (targetToUse is GameObject targetToUseGO) {
                            GeneratePopUpForType(menu, targetToUseGO, false, property, valueType);
                            // add for each component
                            Component[] comps = targetToUseGO.GetComponents<Component>();
                            var duplicateNames = comps.Where(c => c != null).Select(c => c.GetType().Name).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                            foreach (Component comp in comps) {
                                if (comp == null)
                                    continue;
                                GeneratePopUpForType(menu, comp, duplicateNames.Contains(comp.GetType().Name), property, valueType);
                            }
                        }
                    }
                    menu.DropDown(dropdownrect);
                }
                EditorGUI.EndProperty();
            }

            // EditorGUI.PropertyField(position, property, label);
        }

        public class FieldMap {
            public Object target;
            public string fieldName;
            public MemberTypes type;
            public MemberInfo info;

            public FieldMap(Object target, string fieldName, MemberTypes type = MemberTypes.Field, MemberInfo info = null) {
                this.target = target;
                this.fieldName = fieldName;
                this.type = type;
                this.info = info;
            }
            public string GetTypeStr() {
                return type switch {
                    MemberTypes.Field => ((FieldInfo)info)?.FieldType.Name.ToString(),
                    MemberTypes.Method => ((MethodInfo)info)?.ReturnType.Name.ToString(),
                    MemberTypes.Property => ((PropertyInfo)info)?.PropertyType.Name.ToString(),
                    _ => fieldName + " (unknown type)",
                };
            }
        }
        static string GetPath(FieldMap validField, bool addType = false) {
            string path = "";
            path += validField.type switch {
                MemberTypes.Field => validField.fieldName,
                MemberTypes.Method => validField.fieldName + "()",
                MemberTypes.Property => validField.fieldName + "=>",
                _ => validField.fieldName + " (unknown type)",
            };
            if (addType) {
                path += " " + validField.GetTypeStr();
            }
            return path;
        }
        public static void GeneratePopUpForType(GenericMenu menu, Object target, bool useFullTargetName, SerializedProperty property, Type valueType) {
            List<FieldMap> fieldMaps = new List<FieldMap>();
            string targetName = useFullTargetName ? target.GetType().FullName : target.GetType().Name;

            var targetProp = property.FindPropertyRelative(kTargetPath);
            var fieldNameProp = property.FindPropertyRelative(kFieldName);
            if (targetProp == null) {
                // Debug.Log("target prop null"); 
                return;
            }

            // add fields
            fieldMaps.AddRange(GetFieldMap(target, valueType, true));
            // Debug.Log("checking " + target.GetType().ToString() + " " + fieldMaps.Count);

            // order and add to menu
            IEnumerable<FieldMap> orderedFields = fieldMaps.OrderBy(e => e.fieldName.StartsWith("get_") ? 0 : 1).ThenBy(e => e.fieldName);
            foreach (var validField in orderedFields) {
                if (validField == null) return;
                bool isSet = targetProp.objectReferenceValue == validField.target
                     && fieldNameProp.stringValue == validField.fieldName;
                string path = targetName + "/" + GetPath(validField, valueType == typeof(ValueRef.AnyType));
                menu.AddItem(new GUIContent(path), isSet, SetEvent, new ClickMenuData(property, validField));
            }
        }

        static IEnumerable<FieldMap> GetFieldMap(Object target, Type type, bool allowSubclasses) {
            var validMethods = new List<FieldMap>();
            if (target == null || type == null)
                return validMethods;

            Type componentType = target.GetType();
            BindingFlags flags = ValueRef.flags;
            List<FieldInfo> componentFields = componentType.GetFields(flags).Where(x => !x.IsSpecialName).ToList();
            List<PropertyInfo> propFields = componentType.GetProperties(flags).Where(x => !x.IsSpecialName)
                .Where(x => x.GetMethod != null).ToList();
            List<MethodInfo> methodFields = componentType.GetMethods(flags).Where(x => !x.IsSpecialName)
                .Where(x => x.GetParameters().Length == 0).ToList();

            // Debug.Log($"checking {componentType.FullName} {type} fl{flags} cf{componentFields.Count} pf{propFields.Count} mf{methodFields.Count}");

            foreach (var componentField in componentFields) {
                if (componentField.FieldType != type && type != typeof(ValueRef.AnyType))
                    continue;
                if (componentField.DeclaringType != target.GetType()) // todo test with inheritance
                    continue;
                validMethods.Add(new FieldMap(target, componentField.Name, componentField.MemberType, componentField));
            }
            foreach (var componentField in propFields) {
                if (componentField.PropertyType != type && type != typeof(ValueRef.AnyType))
                    continue;
                if (componentField.DeclaringType != target.GetType())
                    continue;
                validMethods.Add(new FieldMap(target, componentField.Name, componentField.MemberType, componentField));
            }
            foreach (var componentField in methodFields) {
                if (componentField.ReturnType != type && type != typeof(ValueRef.AnyType))
                    continue;
                if (componentField.DeclaringType != target.GetType())
                    continue;
                if (componentField.IsGenericMethod || componentField.ReturnType == typeof(void))
                    continue;
                validMethods.Add(new FieldMap(target, componentField.Name, componentField.MemberType, componentField));
            }

            return validMethods;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            //EditorGUI.GetPropertyHeight(property, label);
            return EditorGUIUtility.singleLineHeight * 2;
        }

        public static void SetEvent(object data) {
            // Debug.Log("set");
            var clickData = (ClickMenuData)data;
            var targetProp = clickData.property.FindPropertyRelative(kTargetPath);
            var fieldNameProp = clickData.property.FindPropertyRelative(kFieldName);

            targetProp.objectReferenceValue = clickData.fieldMap.target;
            fieldNameProp.stringValue = clickData.fieldMap.fieldName;
            clickData.property.serializedObject.ApplyModifiedProperties();
        }
        public static void ClearEvent(object prop) {
            // Debug.Log("clear");
            var property = (SerializedProperty)prop;
            var fieldNameProp = property.FindPropertyRelative(kFieldName);
            fieldNameProp.stringValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }
        public class ClickMenuData {
            public SerializedProperty property;
            public FieldMap fieldMap;

            public ClickMenuData(SerializedProperty property, FieldMap fieldMap) {
                this.property = property;
                this.fieldMap = fieldMap;
            }
        }
    }
}