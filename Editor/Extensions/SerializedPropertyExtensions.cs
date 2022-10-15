using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Kutil {
    // by https://gist.github.com/aholkner/214628a05b15f0bb169660945ac7923b 
    // Unity editor extension providing value get/set methods for SerializedProperty. This simplifies writing PropertyDrawers against non-trivial objects.

    // Provide simple value get/set methods for SerializedProperty.  Can be used with
    // any data types and with arbitrarily deeply-pathed properties.
    public static class SerializedPropertyExtensions {


        public static T GetValueOnPropRefl<T>(this SerializedProperty property, string fieldname = null) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = fieldname == null ? property.propertyPath :
                property.propertyPath.Replace(property.name, fieldname);
            if (ReflectionHelper.TryGetValue<T>(targetObject, path, out var val)) {
                return val;
            }
            return default;
        }
        public static bool TryGetValueOnPropRefl<T>(this SerializedProperty property, string fieldname, out T value) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = fieldname == null ? property.propertyPath :
                property.propertyPath.Replace(property.name, fieldname);
            if (ReflectionHelper.TryGetValue<T>(targetObject, path, out var val)) {
                value = val;
                return true;
            }
            value = default;
            return false;
        }
        public static bool TrySetValueOnPropRefl(this SerializedProperty property,object value, string fieldname = null) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = fieldname == null ? property.propertyPath :
                property.propertyPath.Replace(property.name, fieldname);
            return ReflectionHelper.TrySetValue(value, targetObject, path);
        }


        public static SerializedProperty GetNeighborProperty(this SerializedProperty property, string neighborFieldName) {
            string path = property.propertyPath.Replace(property.name, neighborFieldName);
            SerializedProperty neighborProp = property.serializedObject.FindProperty(path);
            return neighborProp;
        }

        public static T GetValue<T>(this SerializedProperty property) {
            object v = property.GetValue();
            if (v is T typedVal) {
                return typedVal;
            }
            if (v != null) {
                Debug.Assert(false, $"{property.name} is not of type {typeof(T)} and could not be casted from {v.GetType()}");
            }
            return default;
        }
        /// (Extension) Get the value of the serialized property.
        public static object GetValue(this SerializedProperty property) {
            string propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            int i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token))
                value = GetPathComponentValue(value, token);
            return value;
        }

        /// (Extension) Set the value of the serialized property.
        public static void SetValue(this SerializedProperty property, object value) {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

            SetValueNoRecord(property, value);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        /// (Extension) Set the value of the serialized property, but do not record the change.
        /// The change will not be persisted unless you call SetDirty and ApplyModifiedProperties.
        public static void SetValueNoRecord(this SerializedProperty property, object value) {
            string propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;

            int i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token)) {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }
            Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary.  Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
            SetPathComponentValue(container, deferredToken, value);
        }

        // Union type representing either a property name or array element index.  The element
        // index is valid only if propertyName is null.
        struct PropertyPathComponent {
            public string propertyName;
            public int elementIndex;
        }

        static Regex arrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

        // Parse the next path component from a SerializedProperty.propertyPath.  For simple field/property access,
        // this is just tokenizing on '.' and returning each field/property name.  Array/list access is via
        // the pseudo-property "Array.data[N]", so this method parses that and returns just the array/list index N.
        //
        // Call this method repeatedly to access all path components.  For example:
        //
        //      string propertyPath = "quests.Array.data[0].goal";
        //      int i = 0;
        //      NextPropertyPathToken(propertyPath, ref i, out var component);
        //          => component = { propertyName = "quests" };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => component = { elementIndex = 0 };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => component = { propertyName = "goal" };
        //      NextPropertyPathToken(propertyPath, ref i, out var component) 
        //          => returns false
        static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component) {
            component = new PropertyPathComponent();

            if (index >= propertyPath.Length)
                return false;

            var arrayElementMatch = arrayElementRegex.Match(propertyPath, index);
            if (arrayElementMatch.Success) {
                index += arrayElementMatch.Length + 1; // Skip past next '.'
                component.elementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
                return true;
            }

            int dot = propertyPath.IndexOf('.', index);
            if (dot == -1) {
                component.propertyName = propertyPath.Substring(index);
                index = propertyPath.Length;
            } else {
                component.propertyName = propertyPath.Substring(index, dot - index);
                index = dot + 1; // Skip past next '.'
            }

            return true;
        }

        static object GetPathComponentValue(object container, PropertyPathComponent component) {
            if (component.propertyName == null)
                return ((IList)container)[component.elementIndex];
            else
                return GetMemberValue(container, component.propertyName);
        }

        static void SetPathComponentValue(object container, PropertyPathComponent component, object value) {
            if (component.propertyName == null)
                ((IList)container)[component.elementIndex] = value;
            else
                SetMemberValue(container, component.propertyName, value);
        }

        static object GetMemberValue(object container, string name) {
            if (container == null)
                return null;
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; ++i) {
                if (members[i] is FieldInfo field)
                    return field.GetValue(container);
                else if (members[i] is PropertyInfo property)
                    return property.GetValue(container);
            }
            return null;
        }

        static void SetMemberValue(object container, string name, object value) {
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; ++i) {
                if (members[i] is FieldInfo field) {
                    field.SetValue(container, value);
                    return;
                } else if (members[i] is PropertyInfo property) {
                    property.SetValue(container, value);
                    return;
                }
            }
            Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }
    }
}