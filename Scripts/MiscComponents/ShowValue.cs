using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kutil {
    public class ShowValue : MonoBehaviour {
        [SerializeField] ValueRef<ValueRef.AnyType> showRef;
        [SerializeField] string prefix = "";
        [SerializeField] string suffix = "";
        [SerializeField, Min(0)] int floatPrecision = 2;
        [SerializeField] string boolTrueString = "true";
        [SerializeField] string boolFalseString = "false";

        [SerializeField] float updatePollInterval = -1;
        float updatePollLastTime = 0;

        [SerializeField] protected Text text;
        [SerializeField] protected TMP_Text text_tmp;

        private void Awake() {
            if (!text) text = GetComponent<Text>();
            if (!text_tmp) text_tmp = GetComponent<TMP_Text>();
        }
        private void OnEnable() {
            UpdateText();
        }
        private void Update() {
            if (updatePollInterval > 0) {
                if (Time.unscaledTime > updatePollLastTime + updatePollInterval) {
                    updatePollLastTime = Time.unscaledTime;
                    UpdateText(true);
                }
            }
        }
        public void UpdateText(bool mustChange = false) {
            if (showRef.TryGetAnyValue(out var refval))
            // if (mustChange ? showRef.TryGetValueChanged(out var refval) : showRef.TryGetValue(out refval))
            {
                SetText(ValToString(refval));
            }
        }
        public void SetText(string t) {
            if (text) {
                text.text = t;
            }
            if (text_tmp) {
                text_tmp.text = t;
            }
        }
        string ValToString(object value) {
            string s = prefix;
            if (value is float valf) {
                s += FloatPrecision(valf);
            } else if (value is bool valb) {
                s += valb ? boolTrueString : boolFalseString;
            } else {
                s += value.ToString();
            }
            s += suffix;
            return s;
        }
        string FloatPrecision(float value) {
            string t;
            if (floatPrecision == 1) {
                t = $"{value:F1}";
            } else if (floatPrecision == 2) {
                t = $"{value:F2}";
            } else if (floatPrecision == 3) {
                t = $"{value:F3}";
            } else if (floatPrecision == 4) {
                t = $"{value:F4}";
            } else {
                t = $"{value}";
            }
            return t;
        }
    }
}