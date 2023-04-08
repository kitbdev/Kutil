using UnityEngine;
using UnityEngine.Sprites;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Kutil.Editor {
    public class SpriteAtlasRenamerEditor : EditorWindow {

        [MenuItem("Kutil/SpriteAtlasRenamer")]
        private static void ShowWindow() {
            var window = GetWindow<SpriteAtlasRenamerEditor>();
            window.titleContent = new GUIContent("SpriteAtlasRenamer");
            window.Show();
        }
        public SerializedObject serializedObject;
        [Multiline]
        public string csvtext;
        public Texture2D selectedTexture;
        public string prefix = "";
        public string suffix = "";

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            UseSelectedTexture();
        }
        private void OnGUI() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(selectedTexture)));
            SerializedProperty textprop = serializedObject.FindProperty(nameof(csvtext));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(prefix)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(suffix)));
            EditorGUILayout.PropertyField(textprop);
            // string r = EditorGUILayout.TextArea(textprop.stringValue,GUILayout.Height(EditorGUIUtility.singleLineHeight*5));
            // EditorGUI.BeginChangeCheck();
            // if (EditorGUI.EndChangeCheck()){
            //     textprop.stringValue = r;
            // }

            if (GUILayout.Button("UpdateNames")) {
                // UpdateNames();
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
        // void UpdateNames() {
        //     if (selectedTexture == null) {
        //         return;
        //     }
        //     Debug.Log("Updating names");
        //     List<string> newnameList = ParseCSV();
        //     // if (newnameList.Count == 0) {
        //     //     Debug.Log("csv param is invalid");
        //     //     return;
        //     // } 
        //     Debug.Log($"Found {newnameList.Count} names!");
        //     // get sprites
        //     string texpath = AssetDatabase.GetAssetPath(selectedTexture);
        //     TextureImporter textureImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
        //     textureImporter.isReadable = true;

        //     // todo TextureImporter.spritesheet' is obsolete: 'Support for accessing sprite meta data through spritesheet has been removed. Please use the UnityEditor.U2D.Sprites.ISpriteEditorDataProvider interface instead.'
        //     SpriteMetaData[] spritesheet = textureImporter.spritesheet;
            


        //     // Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texpath).OfType<Sprite>().ToArray();
        //     Debug.Log($"Found {spritesheet.Length} sprites!");
        //     if (newnameList.Count != spritesheet.Length && newnameList.Count > 0) {
        //         Debug.Log("length mismatch!");
        //         return;
        //     }
        //     int numRenamed = 0;
        //     for (int i = 0; i < spritesheet.Length; i++) {
        //         SpriteMetaData spr = spritesheet[i];
        //         string newname;
        //         if (newnameList.Count > 0) {
        //             newname = prefix + newnameList[i] + suffix;
        //         } else {
        //             newname = prefix + spr.name + suffix;
        //         }
        //         if (newname != "") {
        //             if (spritesheet[i].name == newname) {
        //                 continue;
        //             }
        //             // Debug.Log($"nm {nn}");
        //             // check unique
        //             bool isUnique = true;
        //             for (int j = 0; j < i; j++) {
        //                 if (spritesheet[j].name == newname) {
        //                     Debug.Log($"Not unique {spr.name} to {newname}!");
        //                     isUnique = false;
        //                     break;
        //                 }
        //             }
        //             if (isUnique) {
        //                 Debug.Log($"Renaming {spr.name} to {newname}");
        //                 // Undo.RecordObject(tex, "rename sprite");
        //                 // spr.name = newname;
        //                 // spritesheet[i] = spr;
        //                 spritesheet[i].rect = spr.rect;
        //                 spritesheet[i].name = newname;
        //                 // spr.name = nn;
        //                 numRenamed++;
        //             }
        //         }
        //     }
        //     // Undo.RecordObject(tex, "rename sprite");
        //     // AssetDatabase.ImportAsset(texpath, ImportAssetOptions.ForceUpdate);
        //     if (numRenamed > 0) {
        //         // Undo.RecordObject(textureImporter, $"renamed {numRenamed} sprites");
        //         // textureImporter.spritesheet = spritesheet;//todo
        //         // }
        //         // if (numRenamed > 0) {
        //         EditorUtility.SetDirty(textureImporter);
        //         textureImporter.SaveAndReimport();
        //         // AssetDatabase.ImportAsset(texpath, ImportAssetOptions.ForceUpdate);
        //         Debug.Log($"Renamed {numRenamed} sprites!");
        //     } else {
        //         Debug.Log("no sprites to rename");
        //     }
        //     // selectedTexture.Apply();
        // }
        List<string> ParseCSV() {
            List<string> list = new List<string>();
            if (csvtext.Length == 0) {
                return list;
            }
            string[] lines = csvtext.Split('\n');// no \n is ever found, convert to commas
            // Debug.Log($"{lines.Length} lines");
            int nml = 0;
            foreach (var line in lines) {
                string[] nms;
                if (line.Contains(',')) {
                    nms = line.Split(',');
                } else {
                    nms = line.Split('\t');
                }
                if (nms.Length > 0) {
                    nml = nms.Length;
                }
                foreach (var nm in nms) {
                    list.Add(nm);
                }
            }
            // Debug.Log($"{nml} columns");
            return list;
        }
    }
}