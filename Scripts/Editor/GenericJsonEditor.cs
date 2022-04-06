
#if true
using System.IO;
using System.Linq;
using System.Globalization;

using UnityEngine;
using UnityEditor;
using System;


// Adds a nice editor to edit JSON files as well as a simple text editor incase
// the editor doesn't support the types you need. It works with strings, floats
// ints and bools at the moment.
// 
// * Requires the latest version of JSON.net compatible with Unity

namespace Kutil {
    using UnityEngine;
    using UnityEditor;

    public class GenericJsonEditor : EditorWindow {

        [MenuItem("Kutil/GenericJson")]
        private static void ShowWindow() {
            var window = GetWindow<GenericJsonEditor>();
            window.titleContent = new GUIContent("GenericJson");
            window.Show();
        }

        private void OnGUI() {
            
        }
        private void LoadFromJson() {
            // if (!string.IsNullOrWhiteSpace(Path) && File.Exists(Path)) {
                // rawText = File.ReadAllText(Path);
                // jsonObject = JsonConvert.DeserializeObject<JObject>(rawText);
            // }
        }

        private void WriteToJson() {
            // if (jsonObject != null) {
            //     if (!wasTextMode)
            //         rawText = jsonObject.ToString();

            //     File.WriteAllText(Path, rawText);
            // }
        }

        // private string GetUniqueName(JObject jObject, string orignalName) {
        //     string uniqueName = orignalName;
        //     int suffix = 0;
        //     while (jObject[uniqueName] != null && suffix < 100) {
        //         suffix++;
        //         if (suffix >= 100) {
        //             Debug.LogError("Stop calling all your fields the same thing! Isn't it confusing?");
        //         }
        //         uniqueName = string.Format("{0} {1}", orignalName, suffix.ToString());
        //     }
        //     return uniqueName;
        // }

        // [MenuItem("Assets/Create/JSON File", priority = 81)]
        // public static void CreateNewJsonFile() {
        //     string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //     if (path == "")
        //         path = "Assets";
        //     else if (System.IO.Path.GetExtension(path) != "")
        //         path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

        //     path = System.IO.Path.Combine(path, "New JSON File.json");

        //     JObject jObject = new JObject();
        //     File.WriteAllText(path, jObject.ToString());
        //     AssetDatabase.Refresh();
        // }
    }
}
#endif