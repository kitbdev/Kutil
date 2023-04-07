using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using System;

namespace Kutil {

    // [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer {

        private const string sDictName = nameof(SerializedDictionary<int, int>.serializedDict);
        public static readonly string serializedDictClass = "kutil-serialized-dict";

        VisualElement root;
        PropertyField propField;

        SerializedProperty property;
        SerializedProperty sDictProp;

        // todo warnings
        // todo custom binding onvaluechange to properly set the right field

        // todo keep values that are unable to be added to dictionary?

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            this.property = property;
            sDictProp = property.FindPropertyRelative(sDictName);
            if (sDictProp == null || !sDictProp.isArray) {
                Debug.LogError($"Serialized dict {property.propertyPath} invalid serialized dict prop");
                return null;
            }
            // var sdictRawVal = sDictProp.GetValue();
            // if (sdictRawVal == null || sdictRawVal is not IList list) {
            //     Debug.LogError($"Serialized dict {property.propertyPath} invalid serialized dict prop not list! val: {sdictRawVal?.GetType().Name} {sdictRawVal?.ToString() ?? "null"}.");
            //     return null;
            // }

            // property.GetArrayElementAtIndex()

            root = new VisualElement();
            root.AddToClassList(serializedDictClass);

            // ListView listView = new ListView(list,);
            // listView.AddToClassList();
            propField = new PropertyField(property);
            root.Add(propField);
            root.RegisterCallback<AttachToPanelEvent>(OnAttach);

            return root;
        }

        private void OnAttach(AttachToPanelEvent evt) {
            root.UnregisterCallback<AttachToPanelEvent>(OnAttach);
            if (property == null) return;
            // setup here
            ListView listView = propField.Q<ListView>();
            if (listView == null) {
                Debug.LogError($"SerializedDictionaryDrawer failed to find listview on {propField.name}");
                return;
            }
            Debug.Log("listview " + listView?.name);

            // todo
            // listView.makeItem
            // listView.bindItem = (ve, i) => {
            // ve. sDictProp.GetArrayElementAtIndex(i));
            // ve.Bind(sDictProp.GetArrayElementAtIndex(i));
            // };
            // listView.itemsAdded += (l) => {
            //     Debug.Log("item added! " + l.ToStringFull(null, true));
            //     foreach (var li in l) {
            //         object v = listView.itemsSource[li];
            //         Debug.Log($"val: {v?.GetType().Name} {v?.ToString() ?? "null"}");
            //     }
            // };
        }

        void OnUpdate() {
            // check if all elements in the list are valid
            // keys are unique
            List<uint> contents = new();
            for (int i = 0; i < sDictProp.arraySize; i++) {
                SerializedProperty kvProp = sDictProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = kvProp.FindPropertyRelative(SerializedDictionaryKeyValDrawer.sDictKeyName);
                if (contents.Contains(keyProp.contentHash)) {
                    // set both keys to show warning... somehow
                }
                contents.Add(keyProp.contentHash);
            }

        }


    }
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>.KeyVal))]
    public class SerializedDictionaryKeyValDrawer : PropertyDrawer {
        public static readonly string serializedDictKeyValClass = "kutil-serialized-dict-keyval";
        internal const string sDictKeyName = nameof(SerializedDictionary<int, int>.KeyVal.key);
        internal const string sDictValueName = nameof(SerializedDictionary<int, int>.KeyVal.value);

        VisualElement root;
        PropertyField keyPropField;
        PropertyField valPropField;
        HelpBox warningMsg;

        // InspectorElement inspectorElement;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty keyProp = property.FindPropertyRelative(sDictKeyName);
            SerializedProperty valProp = property.FindPropertyRelative(sDictValueName);

            root = new VisualElement();
            root.name = $"SDictKV:{property.displayName}";
            root.AddToClassList(serializedDictKeyValClass);

            // todo validation and show msg
            warningMsg = new HelpBox("Invalid Key!", HelpBoxMessageType.Warning);
            root.Add(warningMsg);
            warningMsg.SetDisplay(false);


            var container = new VisualElement();
            container.name = $"SDictKV container {property.displayName}";
            root.Add(container);

            keyPropField = new PropertyField(keyProp, property.displayName);//elementProperty.displayName
            valPropField = new PropertyField(valProp, " ");
            keyPropField.name = $"{property.displayName}-Key";
            valPropField.name = $"{property.displayName}-Value";
            keyPropField.style.flexGrow = 1;
            valPropField.style.flexGrow = 1;
            container.Add(keyPropField);
            container.Add(valPropField);

            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.FlexStart;

            // root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            // root.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            return root;
        }

        // private void OnAttach(AttachToPanelEvent evt) {
        //     root.UnregisterCallback<AttachToPanelEvent>(OnAttach);
        //     inspectorElement = root.GetFirstAncestorOfType<InspectorElement>();
        //     if (inspectorElement == null) {
        //         // Debug.LogWarning("serialized dict failed to find inspector!");
        //         return;
        //     }
        //     inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);

        // }

        // private void OnDetach(DetachFromPanelEvent evt) {
        //     root.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        //     if (inspectorElement == null) {
        //         return;
        //     }
        //     inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
        // }

        // private void OnUpdate(SerializedPropertyChangeEvent evt) {
        //     // todo do in parent instead?

        // }


        void SetIsValid(bool isValid) {
            warningMsg.SetDisplay(!isValid);
            // todo get reason?
            // warningMsg.text = "Invalid key ";
        }
    }
}