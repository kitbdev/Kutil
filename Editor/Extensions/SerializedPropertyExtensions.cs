using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Kutil.PropertyDrawers;
using System.Collections.Generic;

namespace Kutil {
    // originally from https://gist.github.com/aholkner/214628a05b15f0bb169660945ac7923b 
    // Unity editor extension providing value get/set methods for SerializedProperty. This simplifies writing PropertyDrawers against non-trivial objects.

    /// <summary>
    /// Provide simple value get/set methods for SerializedProperty.  Can be used with
    /// any data types and with arbitrarily deeply-pathed properties.
    /// </summary>
    public static class SerializedPropertyExtensions {

        /// <summary>
        /// Get the serialized property from a Decorator drawer.
        /// Uses reflection, so cache if possible.
        /// must be called after geochanged.
        /// </summary>
        /// <param name="rootElement"></param>
        /// <returns>the serialized property of the related property field</returns>
        public static SerializedProperty GetBindedPropertyFromDecorator(VisualElement rootElement) {
            PropertyField propertyField = rootElement.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"GetBindedPropertyFromDecorator mustbe called from a decorator root after GeometryChanged! no propertyField for '{rootElement.name}'");
                return null;
            }
            return GetBindedPropertyFromPropertyField(propertyField);
        }
        public static SerializedProperty GetBindedPropertyFromPropertyField(PropertyField propertyField, bool req = true) {
            if (propertyField == null) {
                if (req) Debug.LogError($"GetBindedPropertyFromDecorator property field is null!");
                return null;
            }
            var rootElement = propertyField;
            // try to get on inspector
            InspectorElement inspectorElement = propertyField.GetFirstAncestorOfType<InspectorElement>();
            if (inspectorElement == null) {
                if (req) Debug.LogError($"GetBindedPropertyFromDecorator {rootElement.name} inspectorElement null");
                return null;
            }
            // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/UIElements/Inspector/InspectorElement.cs
            if (ReflectionHelper.TryGetValue<SerializedObject>(inspectorElement, "boundObject", out SerializedObject so)) {
                SerializedProperty serializedPropertyI = so.FindProperty(propertyField.bindingPath);
                if (serializedPropertyI != null) {
                    return serializedPropertyI;
                }
            }

            // try to get on custom inspectorfield 
            InspectorField inspectorField = propertyField.GetFirstAncestorOfType<InspectorField>();
            if (inspectorField != null) {
                SerializedProperty serializedPropertyIF = inspectorField.SerializedObject.FindProperty(propertyField.bindingPath);
                if (serializedPropertyIF != null) {
                    return serializedPropertyIF;
                }
            }

            // try to get on editor
            VisualElement editorElement = inspectorElement.parent;
            if (editorElement == null) {
                Debug.LogError($"GetBindedPropertyFromDecorator {rootElement.name} {inspectorElement.name} editorElement null");
                return null;
            }
            // EditorElement is internal, so get the editor via reflection
            if (!ReflectionHelper.TryGetValue<Editor>(editorElement, "editor", out Editor editor)) {
                Debug.LogError($"GetBindedPropertyFromDecorator {rootElement.name} {editorElement.name} editor null");
                return null;
            }

            SerializedObject serializedObject = editor.serializedObject;
            if (serializedObject == null) {
                if (req) Debug.LogError($"GetBindedSPropFromDecorator {rootElement.name} {editorElement.name} serializedObject null");
                return null;
            }
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyField.bindingPath);
            if (serializedProperty == null) {
                if (req) Debug.LogError($"GetBindedSPropFromDecorator {rootElement.name} {editor} {propertyField.bindingPath} serializedProperty null");
                return null;
            }
            return serializedProperty;
        }
        /// <summary>
        /// Get the editor from a field inside the editor, such as a field or decorator
        /// Uses reflection, so cache if possible.
        /// must be called after geochanged.
        /// </summary>
        /// <param name="fieldElement"></param>
        public static Editor GetEditorFromField(VisualElement fieldElement) {
            InspectorElement inspectorElement = fieldElement.GetFirstAncestorOfType<InspectorElement>();
            if (inspectorElement == null) {
                Debug.LogError($"GetEditorFromDecorator {fieldElement.name} inspectorElement null");
                return null;
            }
            VisualElement editorElement = inspectorElement.parent;
            if (editorElement == null) {
                Debug.LogError($"GetEditorFromDecorator {fieldElement.name} {inspectorElement.name} editorElement null");
                return null;
            }
            // EditorElement is internal, so get the editor via reflection
            if (!ReflectionHelper.TryGetValue<Editor>(editorElement, "editor", out Editor editor)) {
                Debug.LogError($"GetEditorFromDecorator {fieldElement.name} {editorElement.name} editor null");
                return null;
            }
            return editor;
        }

        // flags instead?
        [System.Flags]
        public enum PropIterFlags {
            None = 0,
            Break = 1,
            SkipChildren = 2,
        }

        /// <summary>
        /// invokes func on each property
        /// </summary>
        /// <param name="so"></param>
        /// <param name="func"></param>
        public static void ForEachProperty(this SerializedObject serializedObject, System.Func<SerializedProperty, PropIterFlags?> func, bool enterChildren = false) {
            if (serializedObject == null) return;
            SerializedProperty property = serializedObject.GetIterator();
            // Expand first child.
            if (property.NextVisible(true)) {
                ForEachProperty(property, func, enterChildren);
            }
        }
        /// <summary>
        /// Iterates over all
        /// May want to copy the property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="func"></param>
        /// <param name="enterChildren"></param>
        public static void ForEachProperty(this SerializedProperty property, System.Func<SerializedProperty, PropIterFlags?> func, bool enterChildren = false) {
            if (property == null || func == null) return;
            bool shouldEnterChildren;
            do {
                shouldEnterChildren = enterChildren;
                PropIterFlags? pir = func(property.Copy());
                if (pir != null) {
                    if ((pir & PropIterFlags.Break) > 0) break;
                    shouldEnterChildren = (pir & PropIterFlags.SkipChildren) == 0;
                }

            }
            while (property.NextVisible(shouldEnterChildren));
        }

        public static IEnumerator GetEnumeratorWithChildren(this SerializedProperty property) {
            if (property == null) yield break;
            do {
                yield return property;
            } while (property.NextVisible(true));
        }

        public static IEnumerable<SerializedProperty> GetAllChildren(this SerializedObject serializedObject, bool enterChildren = false) {
            if (serializedObject == null) return new SerializedProperty[0];
            SerializedProperty property = serializedObject.GetIterator();
            // Expand first child.
            if (property.NextVisible(true)) {
                return GetAllChildren(property, enterChildren);
            }
            return new SerializedProperty[0];
        }
        public static IEnumerable<SerializedProperty> GetAllChildren(this SerializedProperty property, bool enterChildren = false) {
            if (property == null) return new SerializedProperty[0];
            var list = new List<SerializedProperty>();
            do {
                list.Add(property.Copy());
            } while (property.NextVisible(enterChildren));
            return list;
        }

        // public static string GetElementName(this SerializedProperty property) {
        // this is literally what .displayName does
        //     if (property.IsElement(out var i)) {
        //         return $"Element {i}";
        //     }
        //     return property.displayName;
        // }

        //? check this
        public static bool IsInAnyArray(this SerializedProperty property) => property.propertyPath.Contains("]");
        public static bool IsElementInArray(this SerializedProperty property) => property.propertyPath.EndsWith("]");
        public static bool IsElementInArray(this SerializedProperty property, out int index) {
            if (property.propertyPath.EndsWith("]")) {
                int v = property.propertyPath.LastIndexOf("[");
                int l = property.propertyPath.Length - v;
                string indexStr = property.propertyPath.Substring(v);
                // Debug.Log(indexStr);
                if (!int.TryParse(indexStr, out index)) {
                    // failed
                    index = -1;
                }
                return true;
            }
            index = -1;
            return false;
        }


        private static string GetPathRelative(SerializedProperty property, string relativeFieldname) {
            if (relativeFieldname == null) return property.propertyPath;
            //? replace from last
            // todo .. to go up?
            return property.propertyPath.Replace(property.name, relativeFieldname);
        }

        public static SerializedProperty GetNeighborProperty(this SerializedProperty property, string neighborFieldName) {
            string path = property.propertyPath.Replace(property.name, neighborFieldName);
            SerializedProperty neighborProp = property.serializedObject.FindProperty(path);
            return neighborProp;
        }


        public static FieldInfo GetFieldInfoOnProp(this SerializedProperty property, string relativeFieldname = null) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = GetPathRelative(property, relativeFieldname);
            object target = targetObject;
            if (ReflectionHelper.TryGetMemberInfo(ref target, path, ReflectionHelper.defFlags, out var memberInfo)) {
                if (memberInfo is FieldInfo fieldInfo) {
                    // fieldInfo.Attributes
                    return fieldInfo;
                }
            }
            return null;
        }


        public static T GetValueOnPropRefl<T>(this SerializedProperty property, string relativeFieldname = null) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = GetPathRelative(property, relativeFieldname);
            if (ReflectionHelper.TryGetValue<T>(targetObject, path, out var val)) {
                return val;
            }
            return default;
        }
        public static bool TryGetValueOnPropRefl<T>(this SerializedProperty property, string fieldname, out T value) {
            try {
                if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null || fieldname == null) {
                    value = default;
                    return false;
                }
            } catch (System.ArgumentNullException e) {
                // sometimes serialized property can be unset and not register as null...
                //ArgumentNullException: Value cannot be null.
                // Parameter name: _unity_self
                Debug.LogError("TryGetValueOnPropRefl property failed: " + e.Message);
                value = default;
                return false;
            }
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = GetPathRelative(property, fieldname);
            if (ReflectionHelper.TryGetValue<T>(targetObject, path, out var val)) {
                value = val;
                return true;
            }
            value = default;
            return false;
        }
        public static bool TrySetValueOnPropRefl(this SerializedProperty property, object value, string relativeFieldname = null) {
            if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null) {
                value = default;
                return false;
            }
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = GetPathRelative(property, relativeFieldname);
            return ReflectionHelper.TrySetValue(value, targetObject, path);
        }



        /// <summary>
        /// (Extension) Get the value of the serialized property.
        /// </summary>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// (Extension) Get the value of the serialized property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetValue(this SerializedProperty property) {
            string propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            int i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token))
                value = GetPathComponentValue(value, token);
            return value;
        }

        /// <summary>
        /// (Extension) Set the value of the serialized property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetValue(this SerializedProperty property, object value) {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

            SetValueNoRecord(property, value);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// (Extension) Set the value of the serialized property, but do not record the change.
        /// The change will not be persisted unless you call SetDirty and ApplyModifiedProperties.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
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