using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace Kutil {

    // [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer {

        private const string sDictName = nameof(SerializedDictionary<int, int>.serializedDict);
        public static readonly string serializedDictClass = "kutil-serialized-dict";


        // todo warnings
        // todo custom binding onvaluechange to properly set the right field

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty sDictProp = property.FindPropertyRelative(sDictName);
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

            var root = new VisualElement();
            root.AddToClassList(serializedDictClass);

            // ListView listView = new ListView(list,);
            // listView.AddToClassList();
            // PropertyField listView = new PropertyField(sDictProp);
            // root.Add(listView);


            return root;
        }

    }
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>.KeyVal))]
    public class SerializedDictionaryKeyValDrawer : PropertyDrawer {
        public static readonly string serializedDictKeyValClass = "kutil-serialized-dict-keyval";
        private const string sDictKeyName = nameof(SerializedDictionary<int, int>.KeyVal.key);
        private const string sDictValueName = nameof(SerializedDictionary<int, int>.KeyVal.value);


        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return CreateListElement(property);
        }

        VisualElement CreateListElement(SerializedProperty elementProperty) {
            SerializedProperty keyProp = elementProperty.FindPropertyRelative(sDictKeyName);
            SerializedProperty valProp = elementProperty.FindPropertyRelative(sDictValueName);

            var containerEl = new VisualElement();
            containerEl.name = $"SDictKV:{elementProperty.displayName}";
            containerEl.AddToClassList(serializedDictKeyValClass);

            var keyPropField = new PropertyField(keyProp, elementProperty.displayName);//elementProperty.displayName
            var valPropField = new PropertyField(valProp, " ");
            keyPropField.name = $"{elementProperty.displayName}-Key";
            valPropField.name = $"{elementProperty.displayName}-Value";
            keyPropField.style.flexGrow = 1;
            valPropField.style.flexGrow = 1;
            containerEl.Add(keyPropField);
            containerEl.Add(valPropField);

            containerEl.style.flexDirection = FlexDirection.Row;
            containerEl.style.justifyContent = Justify.FlexStart;
            return containerEl;
        }
    }
}