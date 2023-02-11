using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    /// <summary>
    /// SO utilities for Editor
    /// </summary>
    public class GetAllSOEditor {
        /// <summary>
        /// Returns all instances of a SO of type T.
        /// filter: 'name', 'l:label'
        /// folders: ex "Assets/Data"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="folders"></param>
        /// <returns>array of ScriptableObjects</returns>
        public static T[] GetAllInstances<T>(string filter = "", string[] folders = null) where T : ScriptableObject {
#if UNITY_EDITOR
            // http://answers.unity.com/answers/1425776/view.html
            // FindAssets uses tags check documentation for more info
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} " + filter, folders);
            T[] a = new T[guids.Length];
            // probably could get optimized 
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return a;
#endif
        }
    }
}