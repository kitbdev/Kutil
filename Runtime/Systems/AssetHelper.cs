using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    public static class AssetHelper {
        public static T[] AutoFindAllAssets<T>(string folder) where T : UnityEngine.Object {
            // ex "Assets/Data"
#if UNITY_EDITOR
        // Find all Gameobjects that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
        string[] guids2 = UnityEditor.AssetDatabase.FindAssets("", new[] { folder });
        List<T> loadAssets = new List<T>();
        foreach (string guid2 in guids2) {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid2);
            // Debug.Log("Loading " + path);
            loadAssets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path));
        }
        return loadAssets.Where(p => p != null).ToArray();
#else
            return null;
#endif
        }
    }
}