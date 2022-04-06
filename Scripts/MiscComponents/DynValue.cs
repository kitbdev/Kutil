using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kutil {
    /// <summary>
    /// Easy way to set values to text
    /// supports float, int, vector2, vector3, bool, and string
    /// </summary>
    public class DynValue : MonoBehaviour {

        public int precision = -1;
        // public string prefix = "";
        public string suffix = "";
        public TMP_Text uiText;

        private void Awake() {
            if (!uiText) {
                uiText = GetComponent<TMP_Text>();
            }
        }
        string ValToString(float value) {
            string t;
            if (precision == 1) {
                t = $"{value:F1}";
            } else if (precision == 2) {
                t = $"{value:F2}";
            } else if (precision == 3) {
                t = $"{value:F3}";
            } else if (precision == 4) {
                t = $"{value:F4}";
            } else {
                t = $"{value}";
            }
            return t;
        }
        string ValToString(Vector2 value) {
            string t;
            t = $"x:{ValToString(value.x)},y:{ValToString(value.y)}";
            return t;
        }
        string ValToString(Vector3 value) {
            string t;
            t = $"x:{ValToString(value.x)},y:{ValToString(value.y)},z:{ValToString(value.z)}";
            return t;
        }
        string ValToString(bool value) {
            return value ? "true" : "false"; ;
        }
        public void ClearText() {
            uiText.text = "";
        }
        public void SetText(float value) {
            uiText.text = ValToString(value) + suffix;
        }
        public void SetText(Vector2 value) {
            uiText.text = ValToString(value) + suffix;
        }
        public void SetText(Vector3 value) {
            uiText.text = ValToString(value) + suffix;
        }
        public void SetText(bool value) {
            uiText.text = ValToString(value) + suffix;
        }
        public void SetText(string value) {
            uiText.text = value + suffix;
        }
        public void SetText() {
            uiText.text = "Hi" + suffix;
        }
        public void AppendText(string value) {
            uiText.text += value + suffix;
        }
        public void AppendText(float value) {
            uiText.text += ValToString(value) + suffix;
        }
        public void AppendText(Vector2 value) {
            uiText.text += ValToString(value) + suffix;
        }
        public void AppendText(Vector3 value) {
            uiText.text += ValToString(value) + suffix;
        }
        public void AppendText(bool value) {
            uiText.text += ValToString(value) + suffix;
        }
        public void NewLine() {
            uiText.text += "\n";
        }
        public void DebugLog(string value) {
            Debug.Log(value);
        }
    }
}