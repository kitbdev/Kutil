using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    public static class AssetHelper {

        /// <summary>
        /// Retrieves all Assets of Type in specified folder.
        /// ex GameObjects in "Assets/Data".
        /// Only works in Unity Editor, use Resources otherwise
        /// </summary>
        /// <param name="folder">location of assets, starting with "Asset/"</param>
        /// <typeparam name="T">type of asset, such as GameObject, Texture, or ScriptableObject</typeparam>
        /// <returns></returns>
        public static T[] AutoFindAllAssets<T>(string folder) where T : UnityEngine.Object {
#if UNITY_EDITOR
            // Find all Gameobjects in specified folder
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