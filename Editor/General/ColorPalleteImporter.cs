using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using Kutil.PropertyDrawers;

namespace Kutil {
    public class ColorPalleteImporter : EditorWindow {

        [Multiline]
        public string field;
        [ContextMenuItem("Clear label", nameof(Clear))]
        public ScriptableObject palette;

        Label outputLabel;

        [HideInInspector]
        public SerializedObject serializedObject;

        [MenuItem("Winter Storm Witch/ColorPalleteImporter")]
        private static void ShowWindow() {
            var window = GetWindow<ColorPalleteImporter>();
            window.titleContent = new GUIContent("ColorPalleteImporter");
            window.Show();
        }

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            var root = new ScrollView();
            rootVisualElement.Add(root);
            root.style.flexGrow = 1;
            root.style.display = DisplayStyle.Flex;
            root.name = "color-pallete-importer";

            // var container = new VisualElement();
            // container.style.minHeight = 80;

            // PropertyField propertyField = new PropertyField();
            // root.Add(propertyField);
            // WorkaroundUIToolkitMissingDefaultInspector.FillDefaultInspector(container, serializedObject, false);
            InspectorField inspectorField = new InspectorField(serializedObject);
            root.Add(inspectorField);

            Button button = new Button();
            button.text = "import";
            button.clicked += () => {
                // bool isOpen = AssetDatabase.IsOpenForEdit(palette, out var msg);
                // Debug.Log($"{palette.name} open:{isOpen} msg:{msg}");
                // if (!isOpen) {
                //     return;
                // }
                // string palettePath = AssetDatabase.GetAssetPath(palette);
                // string v1 = AssetDatabase.LoadAssetAtPath<string>(palettePath);

                string[] allLines = field.Split("\n");
                IEnumerable<string> linesTrimmed = allLines.Select(l => l.Trim()).Where(l => !l.StartsWith(';') && l.Length != 0);
                int numColors = linesTrimmed.Count();
                string outputText = "Generated:\n";
                foreach (var line in linesTrimmed) {
                    // ARGB hexadecimal
                    string colorhex = "#" + line.Trim();
                    if (!ColorUtility.TryParseHtmlString(colorhex, out Color color)) {
                        Debug.LogError("Invalid color " + colorhex);
                        continue;
                    }
                    color = new Color(color.g, color.b, color.a, color.r);

                    outputText += FormatColor(color) + "\n";
                }
                outputLabel.text = outputText;
                Debug.Log($"importing {numColors} colors");
            };
            root.Add(button);

            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
            root.Add(scrollView);
            scrollView.style.flexGrow = 0;

            outputLabel = new Label("click import");
            outputLabel.selection.isSelectable = true;
            scrollView.Add(outputLabel);

            root.Bind(serializedObject);
        }
        private void OnDisable() {
            serializedObject.Dispose();
        }

        [ContextMenu("Clear")]
        void Clear() {
            outputLabel.text = "...";
        }

        string FormatColor(Color color) {
            /*
            format 
            - m_Name: 
                m_Color: {r: 1, g: 1, b: 1, a: 1}
            */
            return $"- m_Name:\n  m_Color: {{r: {color.r}, g: {color.g}, b: {color.b}, a: {color.a}}}";
        }
    }
}