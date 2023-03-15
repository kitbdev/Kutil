// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

// https://gist.github.com/tomkail/ba4136e6aa990f4dc94e0d39ec6a058c

// Must be placed within a folder named "Editor"
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// Extends how ScriptableObject object references are displayed in the inspector
    /// Shows you all values under the object reference
    /// Also provides a button to create a new ScriptableObject if property is null.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ExtendedScriptableObjectDrawer : PropertyDrawer {

        public static readonly string extendedSOClass = "kutil-extented-so";

        public VisualElement CreatePropertyGUI(SerializedProperty property) {
        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new VisualElement();
            root.AddToClassList(extendedSOClass);

            var type = GetFieldType();
            ExtendedSOAttribute extendedSOAttribute = type.GetCustomAttribute<ExtendedSOAttribute>(true);

            if (type == null || ignoreClassFullNames.Contains(type.FullName) || extendedSOAttribute == null) {
                PropertyField propertyField = new PropertyField(property);
                // propertyField.AddToClassList(PropertyField.);
                root.Add(propertyField);
                return root;
            }

            ScriptableObject propertySO = null;
            if (!property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject) {
                propertySO = (ScriptableObject)property.serializedObject.targetObject;
            }
            bool hasValue = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;

            // todo this can change
            if (hasValue) {
                Foldout foldout = new Foldout();
                // foldout.text = "test";
                foldout.viewDataKey = $"{property.propertyPath}-foldout-datakey";
                root.Add(foldout);

                ObjectField objectField = new ObjectField(property.name);
                objectField.objectType = type;
                objectField.bindingPath = property.propertyPath;
                // objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                objectField.style.paddingLeft = 2;
                objectField.style.flexGrow = 1;
                VisualElement foldoutLabelContainer = foldout.Q<Toggle>().Children().FirstOrDefault();
                foldoutLabelContainer.Add(objectField);

                // todo 
                SerializedObject newSO = new SerializedObject(property.objectReferenceValue);
                // InspectorElement defaultInspector = new InspectorElement(property.objectReferenceValue);
                VisualElement defaultInspector = new VisualElement();
                FillDefaultInspector(defaultInspector, newSO, true, true);
                defaultInspector.Bind(newSO);
                foldout.contentContainer.Add(defaultInspector);
                // newSO.Dispose();
            } else {
                VisualElement hbox = new VisualElement();
                hbox.name = "hbox";
                hbox.style.flexDirection = FlexDirection.Row;
                hbox.style.justifyContent = Justify.SpaceBetween;
                root.Add(hbox);

                ObjectField objectField = new ObjectField(property.name);
                objectField.bindingPath = property.propertyPath;
                objectField.objectType = type;
                objectField.AddToClassList(ObjectField.alignedFieldUssClassName);
                hbox.Add(objectField);

                Button addButton = new Button();
                addButton.style.marginLeft = 4;
                addButton.style.marginRight = 4;
                addButton.text = "Create";
                hbox.Add(addButton);
            }

            // root.RegisterCallback<GeometryChangedEvent>(OnGeoChanged);
            return root;
        }
        void OnGeoChanged(GeometryChangedEvent changedEvent) {

        }

        // static VisualElement DrawScriptableObjectChildFieldsUIToolkit<T>(T objectReferenceValue) where T : ScriptableObject {
        //     // Draw a background that shows us clearly which fields are part of the ScriptableObject
        //     VisualElement root;
        //     // todo
        //     EditorGUI.indentLevel++;
        //     EditorGUILayout.BeginVertical(GUI.skin.box);

        //     var serializedObject = new SerializedObject(objectReferenceValue);
        //     // Iterate over all the values and draw them
        //     SerializedProperty prop = serializedObject.GetIterator();
        //     if (prop.NextVisible(true)) {
        //         do {
        //             // Don't bother drawing the class file
        //             if (prop.name == "m_Script") continue;
        //             EditorGUILayout.PropertyField(prop, true);
        //         }
        //         while (prop.NextVisible(false));
        //     }
        //     if (GUI.changed)
        //         serializedObject.ApplyModifiedProperties();
        //     serializedObject.Dispose();
        //     EditorGUILayout.EndVertical();
        //     EditorGUI.indentLevel--;

        //     return root;
        // }
        public static void FillDefaultInspector(VisualElement container, SerializedObject serializedObject, bool hideScript = false, bool hideSDMC = true) {
            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true)) // Expand first child.
            {
                do {
                    if (property.propertyPath == "m_Script" && hideScript) {
                        continue;
                    }
                    if (property.propertyPath == "m_SerializedDataModeController" && hideSDMC) {
                        continue;
                    }
                    var field = new PropertyField(property.Copy());
                    field.name = "PropertyField:" + property.propertyPath;
                    // field.AddToClassList(BaseField<bool>.alignedFieldUssClassName);


                    if (property.propertyPath == "m_Script" && serializedObject.targetObject != null) {
                        field.SetEnabled(false);
                    }

                    container.Add(field);
                }
                while (property.NextVisible(false));
            }
        }



        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float totalHeight = EditorGUIUtility.singleLineHeight;
            if (property.objectReferenceValue == null || !AreAnySubPropertiesVisible(property)) {
                return totalHeight;
            }
            if (property.isExpanded) {
                var data = property.objectReferenceValue as ScriptableObject;
                if (data == null) return EditorGUIUtility.singleLineHeight;
                SerializedObject serializedObject = new SerializedObject(data);
                SerializedProperty prop = serializedObject.GetIterator();
                if (prop.NextVisible(true)) {
                    do {
                        if (prop.name == "m_Script") continue;
                        var subProp = serializedObject.FindProperty(prop.name);
                        float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
                        totalHeight += height;
                    }
                    while (prop.NextVisible(false));
                }
                // Add a tiny bit of height if open for the background
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                serializedObject.Dispose();
            }
            return totalHeight;
        }

        const int buttonWidth = 66;

        static readonly List<string> ignoreClassFullNames = new List<string>{
            "TMPro.TMP_FontAsset", "UnityEngine.InputSystem.InputActionReference"
        };

        bool? hasAttribute;
        bool allowCreation = true;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            var type = GetFieldType();
            ExtendedSOAttribute extendedSOAttribute = type.GetCustomAttribute<ExtendedSOAttribute>(true);
            hasAttribute ??= extendedSOAttribute != null;
            allowCreation = extendedSOAttribute?.allowCreation ?? true;
            if (type == null || ignoreClassFullNames.Contains(type.FullName) || !(hasAttribute ?? false)) {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            ScriptableObject propertySO = null;
            if (!property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject) {
                propertySO = (ScriptableObject)property.serializedObject.targetObject;
            }

            var propertyRect = Rect.zero;
            var guiContent = new GUIContent(property.displayName);
            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (property.objectReferenceValue != null && AreAnySubPropertiesVisible(property)) {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true);
            } else {
                // So yeah having a foldout look like a label is a weird hack 
                // but both code paths seem to need to be a foldout or 
                // the object field control goes weird when the codepath changes.
                // I guess because foldout is an interactable control of its own and throws off the controlID?
                foldoutRect.x += 12;
                EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true, EditorStyles.label);
            }
            var indentedPosition = EditorGUI.IndentedRect(position);
            var indentOffset = indentedPosition.x - position.x;
            propertyRect = new Rect(position.x + (EditorGUIUtility.labelWidth - indentOffset), position.y, position.width - (EditorGUIUtility.labelWidth - indentOffset), EditorGUIUtility.singleLineHeight);

            if (allowCreation && (propertySO != null || property.objectReferenceValue == null)) {
                propertyRect.width -= buttonWidth;
            }

            EditorGUI.ObjectField(propertyRect, property, type, GUIContent.none);
            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();

            var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) {
                var data = (ScriptableObject)property.objectReferenceValue;

                if (property.isExpanded) {
                    // Draw a background that shows us clearly which fields are part of the ScriptableObject
                    GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, Screen.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

                    EditorGUI.indentLevel++;
                    SerializedObject serializedObject = new SerializedObject(data);

                    // Iterate over all the values and draw them
                    SerializedProperty prop = serializedObject.GetIterator();
                    float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (prop.NextVisible(true)) {
                        do {
                            // Don't bother drawing the class file
                            if (prop.name == "m_Script") continue;
                            float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                            EditorGUI.PropertyField(new Rect(position.x, y, position.width - buttonWidth, height), prop, true);
                            y += height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        while (prop.NextVisible(false));
                    }
                    if (GUI.changed)
                        serializedObject.ApplyModifiedProperties();
                    serializedObject.Dispose();

                    EditorGUI.indentLevel--;
                }
            } else {
                if (allowCreation) {
                    if (GUI.Button(buttonRect, "Create")) {
                        if (type.IsAbstract) {
                            GenericMenu typeChooser = new GenericMenu();
                            foreach (var elem in type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t))) {
                                if (elem.IsAbstract) continue;
                                typeChooser.AddItem(new GUIContent(elem.Name), false, (elem) => {
                                    property.objectReferenceValue = CreateAssetWithSavePrompt(elem as Type, GetSelectedAssetPath(property));
                                    property.serializedObject.ApplyModifiedProperties();
                                }, elem);
                            }
                            typeChooser.ShowAsContext();
                        } else {
                            property.objectReferenceValue = CreateAssetWithSavePrompt(type, GetSelectedAssetPath(property));
                        }
                    }
                }
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
        string GetSelectedAssetPath(SerializedProperty property) {
            string selectedAssetPath = "Assets";
            if (property.serializedObject.targetObject is MonoBehaviour) {
                MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
                selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
            }
            return selectedAssetPath;
        }

        public static T _GUILayout<T>(string label, T objectReferenceValue, ref bool isExpanded) where T : ScriptableObject {
            return _GUILayout<T>(new GUIContent(label), objectReferenceValue, ref isExpanded);
        }

        public static T _GUILayout<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded) where T : ScriptableObject {
            Rect position = EditorGUILayout.BeginVertical();

            var propertyRect = Rect.zero;
            var guiContent = label;
            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (objectReferenceValue != null) {
                isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true);

                var indentedPosition = EditorGUI.IndentedRect(position);
                var indentOffset = indentedPosition.x - position.x;
                propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset, EditorGUIUtility.singleLineHeight);
            } else {
                // So yeah having a foldout look like a label is a weird hack 
                // but both code paths seem to need to be a foldout or 
                // the object field control goes weird when the codepath changes.
                // I guess because foldout is an interactable control of its own and throws off the controlID?
                foldoutRect.x += 12;
                EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true, EditorStyles.label);

                var indentedPosition = EditorGUI.IndentedRect(position);
                var indentOffset = indentedPosition.x - position.x;
                propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset - 60, EditorGUIUtility.singleLineHeight);
            }

            EditorGUILayout.BeginHorizontal();
            objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent(" "), objectReferenceValue, typeof(T), false) as T;

            if (objectReferenceValue != null) {

                EditorGUILayout.EndHorizontal();
                if (isExpanded) {
                    DrawScriptableObjectChildFields(objectReferenceValue);
                }
            } else {
                if (GUILayout.Button("Create", GUILayout.Width(buttonWidth))) {
                    string selectedAssetPath = "Assets";
                    var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
                    if (newAsset != null) {
                        objectReferenceValue = (T)newAsset;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            return objectReferenceValue;
        }

        static void DrawScriptableObjectChildFields<T>(T objectReferenceValue) where T : ScriptableObject {
            // Draw a background that shows us clearly which fields are part of the ScriptableObject
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(GUI.skin.box);

            var serializedObject = new SerializedObject(objectReferenceValue);
            // Iterate over all the values and draw them
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true)) {
                do {
                    // Don't bother drawing the class file
                    if (prop.name == "m_Script") continue;
                    EditorGUILayout.PropertyField(prop, true);
                }
                while (prop.NextVisible(false));
            }
            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        public static T DrawScriptableObjectField<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded, bool allowCreation = true) where T : ScriptableObject {
            Rect position = EditorGUILayout.BeginVertical();

            var propertyRect = Rect.zero;
            var guiContent = label;
            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (objectReferenceValue != null) {
                isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true);

                var indentedPosition = EditorGUI.IndentedRect(position);
                var indentOffset = indentedPosition.x - position.x;
                propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset, EditorGUIUtility.singleLineHeight);
            } else {
                // So yeah having a foldout look like a label is a weird hack 
                // but both code paths seem to need to be a foldout or 
                // the object field control goes weird when the codepath changes.
                // I guess because foldout is an interactable control of its own and throws off the controlID?
                foldoutRect.x += 12;
                EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true, EditorStyles.label);

                var indentedPosition = EditorGUI.IndentedRect(position);
                var indentOffset = indentedPosition.x - position.x;
                propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset - 60, EditorGUIUtility.singleLineHeight);
            }

            EditorGUILayout.BeginHorizontal();
            objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent(" "), objectReferenceValue, typeof(T), false) as T;

            if (objectReferenceValue != null) {
                EditorGUILayout.EndHorizontal();
                if (isExpanded) {

                }
            } else {
                if (allowCreation) {
                    if (GUILayout.Button("Create", GUILayout.Width(buttonWidth))) {
                        string selectedAssetPath = "Assets";
                        var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
                        if (newAsset != null) {
                            objectReferenceValue = (T)newAsset;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            return objectReferenceValue;
        }

        // Creates a new ScriptableObject via the default Save File panel
        static ScriptableObject CreateAssetWithSavePrompt(Type type, string path) {
            path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", type.Name + ".asset", "asset", "Enter a file name for the ScriptableObject.", path);
            if (path == "") return null;
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        Type GetFieldType() {
            Type type = fieldInfo.FieldType;
            if (type.IsArray) type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
            return type;
        }

        static bool AreAnySubPropertiesVisible(SerializedProperty property) {
            var data = (ScriptableObject)property.objectReferenceValue;
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();
            while (prop.NextVisible(true)) {
                if (prop.name == "m_Script") continue;
                serializedObject.Dispose();
                return true; //if theres any visible property other than m_script
            }
            serializedObject.Dispose();
            return false;
        }
    }
}