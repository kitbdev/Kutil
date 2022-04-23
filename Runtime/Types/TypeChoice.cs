// using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Kutil {

    /// <summary>
    /// Holds a Type that implements or inherits a base type. Basically a dynamic enum for types.
    /// The type can be selected in the editor, otherwise cannot be changed
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeChoice<T> {

        // todo? seperate no self option
        [SerializeField]
        [CustomDropDown(nameof(dropDownChoice))]
        private SerializedType _selectedType = new SerializedType();
        private bool _onlyIncludeConcreteTypes = true;

        [NonSerialized]
        protected IEnumerable<Type> choicesTypes;// cache

        public Action<SerializedType> onSelectCallback;
        public Func<string, string> formatSelectedValueFunc;
        public Func<string, string> formatListFunc;//s => {
                                                   //     Type curType = GetTypeFor(s);
                                                   //     if (curType != null && curType.BaseType != null) {
                                                   //         return $"{s} : ({curType.BaseType.Name})";
                                                   //     }
                                                   //     return s;
                                                   // };

        public bool onlyIncludeConcreteTypes {
            get => _onlyIncludeConcreteTypes; set {
                if (_onlyIncludeConcreteTypes != value) {
                    ClearCache();
                }
                _onlyIncludeConcreteTypes = value;
            }
        }
        public SerializedType selectedType { get => _selectedType; protected set => _selectedType = value; }
        public Type selectedRawType => selectedType;

        private CustomDropDownData dropDownChoice {
            get {
                UpdateCache();
                return CustomDropDownData.Create<SerializedType>(
                    choicesTypes.Select(t => (SerializedType)t),
                    // null
                    // choices,
                    preFormatValueFunc: o => ((Type)o)?.Name ?? "None",
                    formatListFunc: formatListFunc,
                    onSelectCallback: onSelectCallback,
                    formatSelectedValueFunc: formatSelectedValueFunc,
                    noElementsText: "No inherited or implemented types found!"
                );
            }
        }

        public Type GetBaseType() {
            return typeof(T);
        }

        public TypeChoice() {
            onlyIncludeConcreteTypes = true;
        }
        public TypeChoice(Type setType, bool onlyIncludeConcreteTypes = true,
                Action<SerializedType> onSelectCallback = null,
                Func<string, string> formatSelectedValueFunc = null,
                Func<string, string> formatListFunc = null) {
            this._selectedType = setType;
            this.onlyIncludeConcreteTypes = onlyIncludeConcreteTypes;
            this.onSelectCallback = onSelectCallback;
            this.formatSelectedValueFunc = formatSelectedValueFunc;
            this.formatListFunc = formatListFunc;
        }

        public T CreateInstance() {
            Type selType = _selectedType;
            if (selType != null && !selType.IsAbstract && !selType.IsInterface) {
                return (T)Activator.CreateInstance(selType);
            }
            Debug.LogError($"Cannot create {typeof(T).Name} instance of {selType}!");
            return default;
        }
        public bool TryCreateInstance(out T instance) {
            Type selType = _selectedType;
            if (selType != null && !selType.IsAbstract) {
                instance = (T)Activator.CreateInstance(selType);
                return true;
            }
            instance = default;
            return false;
        }

        protected void ClearCache() {
            choicesTypes = null;
            _selectedType.type = null;
        }
        protected void UpdateCache() {
            choicesTypes ??= GetAllAssignableTypes(typeof(T), onlyIncludeConcreteTypes);
        }
        public IEnumerable<Type> GetAllChoiceTypes() {
            UpdateCache();
            return choicesTypes;
        }

        public override string ToString() {
            return $"TypeChoice<{typeof(T).Name}> {_selectedType.type?.Name}";
        }

        public static IEnumerable<Type> GetAllAssignableTypes(Type type, bool onlyConcrete = true) {
            // Debug.Log($"GetAllAssignableTypes for {type} conc:{onlyConcrete}");
            IEnumerable<Type> enumerable = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => type.IsAssignableFrom(p));
            if (onlyConcrete) {
                enumerable = enumerable.Where(p => !p.IsAbstract && !p.IsInterface);
            }
            return enumerable;
        }
        private static Type GetTypeFor(string typeName) {
            var choicesTypes = GetAllAssignableTypes(typeof(T));
            int index = choicesTypes.Select(t => t.Name).ToList().IndexOf(typeName);
            if (index >= 0) {
                return choicesTypes.ElementAt(index);
            }
            return null;
        }
        public static implicit operator TypeChoice<T>(Type type) => new TypeChoice<T>(type);
        public static explicit operator Type(TypeChoice<T> typechoice) => typechoice._selectedType;
    }
}