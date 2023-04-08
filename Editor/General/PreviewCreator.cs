

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kutil.Editor {
    public static class PreviewCreator {
    
        // private const string previewPath = "Assets";//"Textures/Previews";
    
        [MenuItem("Kutil/Create Preview...")]
        static void CreatePreview() {
            var transforms = Selection.GetTransforms(SelectionMode.TopLevel);
    
            if (transforms.Length > 0) {
                for (int i = 0; i < transforms.Length; i++) {
                    var t = transforms[i];
                    string goName = t.gameObject.name;
                    string defaultFileName = $"{goName}_preview";
    
                    string pathname = EditorUtility.SaveFilePanelInProject($"Create Preview for {goName}", defaultFileName, "png", $"Create preview for {goName}", "Textures");
                    //t.GetComponent<PreviewProvider>() == null && 
                    if (pathname == null || pathname == "") {
                        // dialog was canceled
                        continue;
                    }
                    string fullpath = Path.Combine(Application.dataPath.Replace("Assets", ""), pathname);
                    // Debug.Log("dp:" + Application.dataPath + " pn:" + pathname + " fp:" + fullpath);
    
                    var previewTexture = AssetPreview.GetAssetPreview(t.gameObject);
                    if (previewTexture == null) {
                        Debug.LogWarning($"Asset preview failed to make preview for {goName}", t.gameObject);
                        continue;
                    }
    
                    Debug.Log($"Creating image at {fullpath}");
                    File.WriteAllBytes(fullpath, previewTexture.EncodeToPNG());
                    AssetDatabase.Refresh();
                    // var provider = t.gameObject.AddComponent<PreviewProvider>();
                    // provider.preview = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + path, typeof(Texture2D));
                }
    
    
            }
        }
    }
}