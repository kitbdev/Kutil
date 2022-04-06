using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    public class GetAllSOEditor {
        public static T[] GetAllInstances<T>() where T : ScriptableObject {
#if UNITY_EDITOR
        // http://answers.unity.com/answers/1425776/view.html
        // FindAssets uses tags check documentation for more info
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
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