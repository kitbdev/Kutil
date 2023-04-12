// https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {

    /// <summary>
    /// Serialized dictionary. TKey and TValue must both be serializable
    /// Editable in inspector.
    /// in versions before 2020.1: must inherit like so:
    /// [System.Serializable] public class DictionaryStringVector2 : SerializableDictionary<string, Vector2> {}
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {

#if UNITY_EDITOR
        string warningText {
            get {
                IEnumerable<TKey> allKeys = serializedDict.Select(kv => kv.key);
                HashSet<TKey> keysSet = new(allKeys);
                // Debug.Log($"ak:{allKeys.ToStringFull(null, true)} ks:{keysSet.ToStringFull(null, true)} ki:{allKeys.ToList(). .Except(keysSet).ToStringFull()}");
                if (keysSet.Count != allKeys.Count()) {
                    string invalidKeys = allKeys.RemoveRangeOnce(keysSet).ToStringFull();
                    return $"Uncheck to save! \nDuplicate keys will be removed!: {invalidKeys}";
                }
                return "Uncheck to save Dictionary!";
            }
        }

        // [ShowWarning(nameof(editInEditor), "Uncheck to save dictionary!", showIfTrue = true)]
        [ShowWarning(nameof(editInEditor), nameof(warningText), useTextAsSourceField = true)]
        [Tooltip("Check to edit in inspector. Remember to turn off when finished to save!")]
        //? possible to hide if disabled?
        [SerializeField] bool editInEditor = false;
#endif

        /// <summary>
        /// seriailizable key value pair
        /// </summary>
        [System.Serializable]
        public struct KeyVal {
            public TKey key;
            public TValue value;

            public KeyVal(TKey key, TValue value) {
                this.key = key;
                this.value = value;
            }

            public override string ToString() {
                return $"key:{key} val:'{value}'";// {(isValid ? "valid" : "")}";
            }
            public override bool Equals(object obj) {
                if (obj is KeyVal b) {
                    return key.Equals(b.key) && value.Equals(b.value);
                }
                return false;
            }
            public override int GetHashCode() {
                return key.GetHashCode() ^ 43242 * value.GetHashCode();
            }
            public static bool operator ==(KeyVal a, KeyVal b) => a.key.Equals(b.key) && a.value.Equals(b.value);
            public static bool operator !=(KeyVal a, KeyVal b) => !a.key.Equals(b.key) || !a.value.Equals(b.value);
        }
        [ConditionalHide(nameof(editInEditor), true, readonlyInstead = true)]
        [SerializeField]
        List<KeyVal> serializedDict = new List<KeyVal>();


        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            if (editInEditor) {
                return;
            }
#endif
            // save dict to list
            serializedDict = this.ToList().Select(kvp => new KeyVal(kvp.Key, kvp.Value)).ToList();
        }

        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            if (editInEditor) {
                return;
            }
#endif
            // restore list to dict
            this.Clear();
            for (int i = 0; i < serializedDict.Count; i++) {
                KeyVal kv = serializedDict[i];
                this.TryAdd(kv.key, kv.value);
            }
            // serializedDict = null;
        }
    }
}