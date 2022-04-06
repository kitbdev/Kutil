using UnityEngine;
using UnityEngine.Sprites;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Kutil {
    public class SpriteAtlasRenamerEditor : EditorWindow {

        [MenuItem("Util/SpriteAtlasRenamer")]
        private static void ShowWindow() {
            var window = GetWindow<SpriteAtlasRenamerEditor>();
            window.titleContent = new GUIContent("SpriteAtlasRenamer");
            window.Show();
        }
        public SerializedObject serializedObject;
        // [Multiline]
        public string csvtext;
        public Texture2D selectedTexture;

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            UseSelectedTexture();
        }
        private void OnGUI() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(selectedTexture)));
            SerializedProperty textprop = serializedObject.FindProperty(nameof(csvtext));
            EditorGUILayout.PropertyField(textprop);
            // string r = EditorGUILayout.TextArea(textprop.stringValue,GUILayout.Height(EditorGUIUtility.singleLineHeight*5));
            // EditorGUI.BeginChangeCheck();
            // if (EditorGUI.EndChangeCheck()){
            //     textprop.stringValue = r;
            // }

            if (GUILayout.Button("UpdateNames")) {
                UpdateNames();
            }
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSelectionChange() {
            UseSelectedTexture();
        }
        private void UseSelectedTexture() {
            if (Selection.objects.Length > 1) {
                selectedTexture = null;
            } else {
                selectedTexture = Selection.activeObject as Texture2D;
            }

            if (selectedTexture != null) {
                var assetPath = AssetDatabase.GetAssetPath(selectedTexture);
            } else {
            }

            Repaint();
        }
        void UpdateNames() {
            if (selectedTexture == null) {
                return;
            }
            Debug.Log("Updating names");
            List<string> newnameList = ParseCSV();
            if (newnameList.Count == 0) {
                Debug.Log("csv param is invalid");
                return;
            }
            Debug.Log($"Found {newnameList.Count} names!");
            // get sprites
            string texpath = AssetDatabase.GetAssetPath(selectedTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
            SpriteMetaData[] spritesheet = textureImporter.spritesheet;


            // Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texpath).OfType<Sprite>().ToArray();
            Debug.Log($"Found {spritesheet.Length} sprites!");
            if (newnameList.Count != spritesheet.Length) {
                Debug.Log("length mismatch!");
                return;
            }
            int rncnt = 0;
            for (int i = 0; i < spritesheet.Length; i++) {
                SpriteMetaData spr = spritesheet[i];
                string nn = newnameList[i];
                if (nn != "") {
                    if (spritesheet[i].name == nn) {
                        continue;
                    }
                    // Debug.Log($"nm {nn}");
                    // check unique
                    bool isUnique = true;
                    for (int j = 0; j < i; j++) {
                        if (spritesheet[j].name == nn) {
                            Debug.Log($"Not unique {spr.name} to {nn}!");
                            isUnique = false;
                            break;
                        }
                    }
                    if (isUnique) {
                        Debug.Log($"Renaming {spr.name} to {nn}");
                        // Undo.RecordObject(tex, "rename sprite");
                        spritesheet[i].name = nn;
                        // spr.name = nn;
                        rncnt++;
                    }
                }
            }
            // Undo.RecordObject(tex, "rename sprite");
            // AssetDatabase.ImportAsset(texpath, ImportAssetOptions.ForceUpdate);
            textureImporter.spritesheet = spritesheet;
            if (rncnt > 0) {
                EditorUtility.SetDirty(textureImporter);
            }
            textureImporter.SaveAndReimport();
            // selectedTexture.Apply();
            Debug.Log($"Renamed {rncnt} sprites!");
        }
        List<string> ParseCSV() {
            List<string> list = new List<string>();
            if (csvtext.Length == 0) {
                return list;
            }
            string[] lines = csvtext.Split('\n');// no \n is ever found, convert to commas
            Debug.Log($"{lines.Length} lines");
            int nml = 0;
            foreach (var line in lines) {
                string[] nms = line.Split(',');
                if (nms.Length > 0) {
                    nml = nms.Length;
                }
                foreach (var nm in nms) {
                    list.Add(nm);
                }
            }
            Debug.Log($"{nml} columns");
            return list;
        }
    }
}