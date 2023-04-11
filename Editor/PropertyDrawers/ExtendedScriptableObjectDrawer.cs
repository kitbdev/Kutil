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

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// Extends how ScriptableObject object references are displayed in the inspector
    /// Shows you all values under the object reference
    /// Also provides a button to create a new ScriptableObject if property is null.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ExtendedScriptableObjectDrawer : PropertyDrawer {

        public static readonly string extendedSOClass = "kutil-extented-so";

        VisualElement root;
        Foldout hasValueFoldout;
        VisualElement noValueHBox;
        // SerializedProperty fieldProperty;

        SerializedObject hasValueSO;
        VisualElement defaultInspector;
        Object valueReference;

        string lastUsedAssetPath = null;

        // public VisualElement CreatePropertyGUI(SerializedProperty property) {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            root = new VisualElement();
            root.name = $"ExtendedSO:{property.propertyPath}";
            root.AddToClassList(extendedSOClass);
            // this.fieldProperty = property;//.Copy();

            var type = GetFieldType();
            ExtendedSOAttribute extendedSOAttribute = type.GetCustomAttribute<ExtendedSOAttribute>(true);

            if (type == null || ignoreClassFullNames.Contains(type.FullName) || extendedSOAttribute == null) {
                PropertyField propertyField = new PropertyField(property);
                // propertyField.AddToClassList(PropertyField.);
                root.Add(propertyField);
                return root;
            }

            bool allowCreation = extendedSOAttribute?.allowCreation ?? true;

            ScriptableObject propertySO = null;
            if (!property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject) {
                propertySO = (ScriptableObject)property.serializedObject.targetObject;
            }



            // has value ui
            hasValueFoldout = new Foldout();
            hasValueFoldout.name = "hasValueFoldout";
            // foldout.text = "test";
            // start closed
            hasValueFoldout.value = false;
            hasValueFoldout.viewDataKey = $"{property.propertyPath}-foldout-datakey";
            Toggle toggle = hasValueFoldout.Q<Toggle>();
            toggle.AddToClassList(Toggle.alignedFieldUssClassName);
            toggle.style.marginTop = 0;
            toggle.style.marginBottom = 0;
            toggle.style.marginLeft = 0;
            toggle.style.marginRight = 0;
            var checkMark = toggle.Q("unity-checkmark");
            checkMark.style.marginRight = 0;
            root.Add(hasValueFoldout);

            // string fieldName = property.displayName;
            string fieldName = preferredLabel;
            ObjectField hvObjectField = new ObjectField(fieldName);
            hvObjectField.name = "ESO-has-value-objectfield";
            hvObjectField.objectType = type;
            // hvObjectField.bindingPath = property.propertyPath;
            hvObjectField.BindProperty(property);
            hvObjectField.AddToClassList(ObjectField.alignedFieldUssClassName);
            hvObjectField.style.paddingLeft = 2;
            hvObjectField.style.flexGrow = 1;
            hvObjectField.style.flexShrink = 1;
            hvObjectField.style.marginRight = 0;
            // hvObjectField.RegisterValueChangedCallback(ce => UpdateUI());
            // hvObjectField.TrackPropertyValue
            // hvObjectField.RegisterCallback<ChangeEvent<Object>, SerializedProperty>((ce, p) => {
            //     UpdateUI(p);
            // }, property);
            // hvObjectField.RegisterValueChangedCallback(ce => {
            //     // Debug.Log("hv update " + ce.newValue + " me:" + property.propertyPath + " uh:" + fieldProperty.propertyPath);
            //     UpdateUI(ce.newValue != null);
            // });
            Label label = hvObjectField.Q<Label>();
            label.AddToClassList(PropertyField.labelUssClassName);
            label.style.marginRight = 5;
            VisualElement foldoutLabelContainer = toggle.Children().FirstOrDefault();
            foldoutLabelContainer.Add(hvObjectField);

            // AddInspector();

            // no value ui
            noValueHBox = new VisualElement();
            noValueHBox.name = "noValueHBox";
            noValueHBox.style.flexDirection = FlexDirection.Row;
            noValueHBox.style.justifyContent = Justify.SpaceBetween;
            root.Add(noValueHBox);

            ObjectField nvObjectField = new ObjectField(fieldName);
            nvObjectField.name = "ESO-no-value-objectfield";
            // nvObjectField.bindingPath = property.propertyPath;
            nvObjectField.BindProperty(property);
            nvObjectField.style.flexGrow = 1;
            nvObjectField.style.flexShrink = 1;
            nvObjectField.objectType = type;
            nvObjectField.AddToClassList(ObjectField.alignedFieldUssClassName);
            nvObjectField.Q<Label>().AddToClassList(PropertyField.labelUssClassName);
            //? callback not working
            // nvObjectField.RegisterValueChangedCallback(ce => {
            //     Debug.Log($"nv update {ce.previousValue}-{ce.newValue} hv:{ce.newValue != null} me:{fieldProperty.propertyPath} {property.propertyPath}");
            //     // UpdateUI();
            //     UpdateUI(ce.newValue != null);
            // });
            //me:{fieldProperty.propertyPath} 
            var args = new ESoPassArgs() {
                esoDrawer = this,
                property = property,
                root = root,
                hasValueFoldout = hasValueFoldout,
                noValueHBox = noValueHBox,

            };
            // hvObjectField.RegisterCallback<ChangeEvent<Object>, ESoPassArgs>(UpdateFieldUI, args);
            // nvObjectField.RegisterCallback<ChangeEvent<Object>, ESoPassArgs>(UpdateFieldUI, args);
            hvObjectField.RegisterCallback<ChangeEvent<Object>, SerializedProperty>((c, p) => UpdateUI(p), property);
            nvObjectField.RegisterCallback<ChangeEvent<Object>, SerializedProperty>((c, p) => UpdateUI(p), property);
            // nvObjectField.RegisterCallback<ChangeEvent<Object>, SerializedProperty>((ce, p) => {
            //     // Debug.Log($"nv update2 {ce.previousValue}-{ce.newValue} hv:{ce.newValue != null} {property.propertyPath} {p.propertyPath}");
            //     UpdateUI(p);
            // }, property);

            noValueHBox.Add(nvObjectField);

            if (allowCreation) {
                Button addButton = new Button();
                addButton.name = "ESO-CreateButton";
                addButton.style.marginLeft = 4;
                addButton.style.marginRight = 4;
                addButton.style.flexShrink = 1;
                addButton.text = "Create";

                lastUsedAssetPath = GetSelectedAssetPath(property);

                IEnumerable<Type> assignableTypes = type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t));
                if (type.IsAbstract || assignableTypes.Count() > 1) {
                    // todo test!
                    addButton.AddManipulator(new ContextualMenuManipulator(evt => {
                        foreach (var assignableType in assignableTypes) {
                            if (assignableType.IsAbstract) continue;
                            evt.menu.AppendAction(assignableType.Name, action => {
                                property.objectReferenceValue = CreateAssetWithSavePrompt(assignableType as Type, lastUsedAssetPath);
                                property.serializedObject.ApplyModifiedProperties();
                                UpdateUI(true);
                            }, DropdownMenuAction.AlwaysEnabled);
                        }
                    }));
                } else {
                    addButton.clicked += () => {
                        // Debug.Log("create button clicked");
                        property.objectReferenceValue = CreateAssetWithSavePrompt(type, lastUsedAssetPath);
                        property.serializedObject.ApplyModifiedProperties();
                        UpdateUI(true);
                    };
                }
                noValueHBox.Add(addButton);
            }

            UpdateUI(property);
            return root;
        }

        private void AddInspector(SerializedProperty property) {
            if (property == null || property.serializedObject.targetObject == null) {
                ClearValueSO();
                return;
            }
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (property.objectReferenceValue != null) {
                if (property.objectReferenceValue == valueReference) {
                    // already updated
                    return;
                }
                if (valueReference != null) {
                    // clear the old SO
                    ClearValueSO();
                }
                valueReference = property.objectReferenceValue;
                hasValueSO = new SerializedObject(property.objectReferenceValue);
                // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/Editor.cs
                // todo? should be able to use InspectorElement if passed in an Editor wiht OnCreateInspectorGUI overridden to return InspectorElement.FillDefaultInspector()
                // this uses imgui still
                // InspectorElement defaultInspector = new InspectorElement(property.objectReferenceValue);
                // use an InspectorElement to allow certain decorators to find the proper serializedobject?
                // InspectorElement inspectorParent = new InspectorElement();
                // VisualElement defaultInspector = new VisualElement();
                // FillDefaultInspector(defaultInspector, hasValueSO, true, true);
                defaultInspector = new InspectorField(hasValueSO);
                defaultInspector.Bind(hasValueSO);
                hasValueFoldout.contentContainer.Add(defaultInspector);
                // inspectorParent.Add(defaultInspector);
            } else {
                // remove old one, if there was one
                ClearValueSO();
            }
        }

        private void ClearValueSO() {
            if (hasValueSO != null) {
                hasValueSO.Dispose();
                hasValueSO = null;
            }
            if (defaultInspector != null) {
                hasValueFoldout.Clear();
                // hasValueFoldout.contentContainer.Remove(defaultInspector);
                defaultInspector = null;
            }
            valueReference = null;
        }

        // void UpdateUI() {
        //     fieldProperty.serializedObject.Update();
        //     bool hasValue = fieldProperty.propertyType == SerializedPropertyType.ObjectReference && fieldProperty.objectReferenceValue != null;
        //     Debug.Log($"Updating ui on {fieldProperty.propertyPath} hasValue:{hasValue}");
        //     UpdateUI(hasValue);
        // }
        // ! for some reason, the property drawer is reused for some arrays or something and the instance variables get overwritten to the last element in the array propertydrawer instance
        // ! this means when updating ui, it would only work on the last element and throw errors otherwise
        struct ESoPassArgs {
            public SerializedProperty property;
            public ExtendedScriptableObjectDrawer esoDrawer;
            public VisualElement root;
            public Foldout hasValueFoldout;
            public VisualElement noValueHBox;
            public SerializedObject hasValueSO;
            public VisualElement defaultInspector;
            public Object valueReference;
            public string lastUsedAssetPath;

            // public ExtendedScriptableObjectDrawer(ExtendedScriptableObjectDrawer esoDrawer, SerializedProperty property) {
            //     this.esoDrawer = esoDrawer;
            //     this.property = property;
            //     this.root = esoDrawer.root;
            //     // todo 
            // }

            public override string ToString() {
                return "ESOARG"
                + " property:" + property?.propertyPath
                + " esoDrawer:" + esoDrawer?.ToString()
                + " root:" + root?.ToStringBetter()
                + " hasValueFoldout:" + hasValueFoldout?.ToStringBetter()
                + " noValueHBox:" + noValueHBox?.ToStringBetter()
                + " hasValueSO:" + hasValueSO?.ToString()
                + " defaultInspector:" + defaultInspector?.ToString()
                + " valueReference:" + valueReference?.ToString()
                + " lastUsedAssetPath:" + lastUsedAssetPath?.ToString()
                ;
            }
        }

        static void UpdateFieldUI(ChangeEvent<Object> evt, ESoPassArgs userArgs) {
            Debug.Log(userArgs.ToString());
            UpdateUI(userArgs);
        }

        static void UpdateUI(ESoPassArgs args) {
            // args.esoDrawer.UpdateUI(args.property);
            bool hasValue = args.property.propertyType == SerializedPropertyType.ObjectReference && args.property.objectReferenceValue != null;
            Debug.Log($"Updating ui2 on {args.property.propertyPath} hasValue:{hasValue} {args.root.ToStringBetter()} p:{args.root.parent?.ToStringBetter()}");

            args.hasValueFoldout.SetDisplay(hasValue);
            args.noValueHBox.SetDisplay(!hasValue);
            //todo
            args.esoDrawer.UpdateAssetPath();
            args.esoDrawer.AddInspector(args.property);
        }
        void UpdateUI(SerializedProperty property) {
            if (property == null) {
                Debug.LogError("update ui property null!");
                return;
            }
            property.serializedObject.Update();
            bool hasValue = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;
            Debug.Log($"Updating ui on {property.propertyPath} hasValue:{hasValue} {root.ToStringBetter()} p:{root.parent?.ToStringBetter()}");
            UpdateUI(hasValue);
            AddInspector(property);
        }

        private void UpdateUI(bool hasValue) {
            // todo sometimes works, sometimes doesnt
            // hasValueFoldout.SetDisplay(hasValue);
            hasValueFoldout.style.display = hasValue ? DisplayStyle.Flex : DisplayStyle.None;
            noValueHBox.style.display = !hasValue ? DisplayStyle.Flex : DisplayStyle.None;
            // hasValueFoldout.MarkDirtyRepaint();
            // AddInspector();
            // Debug.Log(hasValueFoldout.style.display+" "+hasValue);

            UpdateAssetPath();
        }

        void UpdateAssetPath() {
            Object so = valueReference;
            if (so == null) return;
            lastUsedAssetPath = AssetDatabase.GetAssetPath(so).Replace($"/{so.name}.asset", "");
        }



        /// old IMGUI way

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

    /// <summary>
    /// create a default inspector for a serialized object.
    /// must bind yourself 
    /// because the uitk InspectorElement uses imgui still.
    /// </summary>
    public class InspectorField : VisualElement {
        public static readonly string inspectorFieldClass = "kutil-inspector-field";
        SerializedObject serializedObject;
        public SerializedObject SerializedObject => serializedObject;

        public InspectorField(SerializedObject serializedObject, bool hideScript = true) {
            // this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            this.serializedObject = serializedObject;
            this.AddToClassList(inspectorFieldClass);
            this.AddToClassList(InspectorElement.ussClassName);
            this.AddToClassList(InspectorElement.customInspectorUssClassName);
            this.AddToClassList(InspectorElement.uIEInspectorVariantUssClassName);
            this.AddToClassList(InspectorElement.uIECustomVariantUssClassName);
            this.style.paddingLeft = 0;
            this.style.paddingTop = 0;
            this.style.paddingRight = 0;
            //? need prefab stuff?
            // try {
            // todo custom editor support?
            // Editor.CreateEditor(serializedObject?.targetObject);

            FillDefaultInspector(this, serializedObject, hideScript, true);

            // this shows script field
            // https://github.com/needle-mirror/com.unity.ui/blob/master/Editor/Inspector/InspectorElement.cs
            // InspectorElement.FillDefaultInspector(this, serializedObject, null);

            this.Bind(serializedObject);
            // } catch (System.Exception) {

            //     throw;
            // }
        }

        /// <summary>
        /// create a default inspector for a serialized object. 
        /// only using because the uitk InspectorElement uses imgui still
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serializedObject"></param>
        /// <param name="hideScript"></param>
        /// <param name="hideSDMC"></param>
        public static void FillDefaultInspector(VisualElement container, SerializedObject serializedObject, bool hideScript = false, bool hideSDMC = true) {
            if (serializedObject == null)
                return;

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

                    if (property.propertyPath == "m_Script" && serializedObject.targetObject != null) {
                        field.SetEnabled(false);
                    }

                    container.Add(field);
                }
                while (property.NextVisible(false));
            }
        }

    }
}