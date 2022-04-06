using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    [System.Serializable]
    public class CustomDropDownData {
        [System.Serializable]
        public struct Data {
            // [SerializeReference]
            public object value;
            public string name;
        }
        public Data[] data;
        public Func<object, string> preFormatValueFunc;
        public Func<string, string> formatSelectedValueFunc;
        public Func<string, string> formatListFunc;
        public Action<object> onSelectCallback;
        public bool includeNullChoice;
        public string noElementsText;
        public string errorText;

        // public CustomDropDownData() { }

        /// <summary>
        /// Create CustomDropDownData from values
        /// </summary>
        /// <param name="dataValues">list of values</param>
        /// <param name="displayNames">optional display names of values</param>
        /// <param name="preFormatValueFunc">optional func to convert values into strings</param>
        /// <param name="formatListFunc">optional func to format entire display list</param>
        /// <param name="formatSelectedValueFunc">optional func to format the selected value</param>
        /// <param name="onSelectCallback">optional callback after the value is set</param>
        /// <param name="includeNullChoice">include a null choice at beginning? (default=false)</param>
        /// <param name="noElementsText">optional string used if choices has no elements</param>
        /// <param name="errorText">optional string used if an error is encountered finding the choices</param>
        /// <typeparam name="T">type of data</typeparam>
        /// <returns>CustomDropDownData</returns>
        public static CustomDropDownData Create<T>(
            IEnumerable<T> dataValues,
            IEnumerable<string> displayNames = null,
            Func<T, string> preFormatValueFunc = null,
            Func<string, string> formatListFunc = null,
            Func<string, string> formatSelectedValueFunc = null,
            Action<T> onSelectCallback = null,
            bool includeNullChoice = false,
            string noElementsText = null,
            string errorText = null
        ) {
            CustomDropDownData customDropDownData = new CustomDropDownData() {
                formatSelectedValueFunc = formatSelectedValueFunc,
                formatListFunc = formatListFunc,
                includeNullChoice = includeNullChoice,
                noElementsText = noElementsText,
                errorText = errorText,
            };
            // UnityEngine.Debug.Log($"new Cdd {typeof(T)} cast to obj");

            if (preFormatValueFunc != null) {
                customDropDownData.preFormatValueFunc = new Func<object, string>(o => preFormatValueFunc((T)o));
            }
            if (onSelectCallback != null) {
                customDropDownData.onSelectCallback = new Action<object>(o => onSelectCallback((T)o));
            }
            customDropDownData.data = dataValues?
                .Select(v => (object)v)
                .Zip(displayNames ?? dataValues.Select(v => {
                    return customDropDownData.preFormatValueFunc != null ?
                        customDropDownData.preFormatValueFunc(v) :
                        v.ToString();
                }),
                (o, s) => new Data() { value = o, name = s })
                .ToArray() ?? new Data[0];
            // List<T> ts = dataValues.ToList();
            // T[] vals = ts.ToArray();
            // string[] datanamesarr = displayNames?.ToArray();
            // List<Data> datalist = new List<Data>();
            // for (int i = 0; i < vals.Length; i++) {
            //     object dv = vals[i];
            //     string name = displayNames != null ? datanamesarr[i] : dv.ToString();
            //     datalist.Add(new Data() {
            //         value = dv, name = name
            //     });
            // }
            return customDropDownData;
        }

    }
}