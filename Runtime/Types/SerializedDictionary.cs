// https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {

    //?
    // [System.Serializable]
    // public class SerializedDictonaryRendering<TKey, TValue> : UnityEngine.Rendering.SerializedDictionary<TKey, TValue> {
    // }

    // [System.Serializable]
    // public class SerializedDictionary1<TKey, TValue> : Dictionary<TKey, TValue> {
    //     // see SerializedDictionaryDrawer
    // }

    /// <summary>
    /// Serialized dictionary. TKey and TValue must both be serializable
    /// Editable in inspector.
    /// Only serialized in UnityEditor.
    /// in versions before 2020.1: must inherit like so:
    /// [System.Serializable] public class DictionaryStringVector2 : SerializableDictionary<string, Vector2> {}
    /// (I think)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {

        // todo clean up

        [SerializeField]
        [HideInInspector]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        [HideInInspector]
        private List<TValue> values = new List<TValue>();

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
        [Tooltip("Remember to turn off when finished or may not be able to update in scripts!")]
        //? possible to hide if disabled?
        [SerializeField] bool editInEditor = false;
        // show in editor
        [System.Serializable]
        public struct KeyVal {
            public TKey key;
            public TValue value;
            // [HideInInspector]
            // public bool isValid;

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
        public List<KeyVal> serializedDict = new List<KeyVal>();

        // KeyValuePair<TKey, TValue> a;

        // private void OnValidate() {
        //     Debug.Log("Validate");
        //     Clear();
        //     foreach (var kv in displayList) {
        //         Add(kv.key, kv.value);
        //     }
        //     //  = displayList.ToDictionary(kv=>kv.key,kv=>kv.value);
        //     // keys = displayList.Select(k => k.key).ToList();
        //     // values = displayList.Select(k => k.value).ToList();
        // }
#endif


        //         public void OnBeforeSerialize() {
        // #if UNITY_EDITOR
        //             // save dict to list
        //             serializedDict = this.ToList().Select(kvp => new KeyVal(kvp.Key, kvp.Value)).ToList();
        // #endif
        //         }

        //         public void OnAfterDeserialize() {
        // #if UNITY_EDITOR
        //             // restore list to dict
        //             Clear();
        //             for (int i = 0; i < serializedDict.Count; i++) {
        //                 KeyVal kv = serializedDict[i];
        //                 TryAdd(kv.key, kv.value);
        //             }
        //             // serializedDict = null;
        // #endif
        //         }


        // save the dictionary to lists
        public void OnBeforeSerialize() {
            // Debug.Log("OnBeforeSerialize");
#if UNITY_EDITOR
            if (editInEditor) {
                // update actual dict with values from inspector
                Clear();
                for (int i = 0; i < serializedDict.Count; i++) {
                    KeyVal kv = serializedDict[i];
                    // kv.isValid = 
                    TryAdd(kv.key, kv.value);
                    // displayList[i] = kv;
                }
            }
#endif
            // update serialization
            keys.Clear();
            values.Clear();
            if (typeof(TKey).IsSubclassOf(typeof(UnityEngine.Object)) || typeof(TKey) == typeof(UnityEngine.Object)) {
                // avoid copying UnityEngine.Objects that have been destroyed in the event that they're used as a key
                foreach (var element in this.Where(element => element.Key != null)) {
                    keys.Add(element.Key);
                    values.Add(element.Value);
                }
            } else {
                foreach (var element in this) {
                    keys.Add(element.Key);
                    values.Add(element.Value);
                }
            }
#if UNITY_EDITOR
            if (!editInEditor) {
                KeyVal[] newList = keys.Zip(values, (k, v) => new KeyVal() { key = k, value = v }).ToArray();
                serializedDict = newList.ToList();
            }
#endif
        }

        // load dictionary from lists
        public void OnAfterDeserialize() {
            // Debug.Log("OnAfterDeserialize");
            this.Clear();

            if (keys.Count != values.Count) {
                throw new System.Exception($"there are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable. ({typeof(TKey)},{typeof(TValue)})");
            }

            for (int i = 0; i < keys.Count; i++) {
                this.Add(keys[i], values[i]);
            }

#if UNITY_EDITOR
            // update inspector after deserialize (like from scripts?)
            // KeyVal[] oldList = displayList.ToArray();
            if (!editInEditor) {
                KeyVal[] newList = keys.Zip(values, (k, v) => new KeyVal() { key = k, value = v }).ToArray();
                serializedDict = newList.ToList();
            }

            // displayList = displayList.Where(kv => !kv.isValid).ToList();
            // displayList.AddRange(collection);
            // collection.Where(kv=>kv.)
            // rules
            // old list has priority where the key is invalid
            // otherwise new list has priority
            // todo really need to seperate updates from editor and updates from script
            // todo since we cant know when updated by script, need a custom editor drawer with list

            // displayList.Clear();
            // int lmax = Mathf.Max(newList.Length, oldList.Length);
            // for (int i = 0, j = 0; i < lmax && j < lmax; i++, j++) {
            //     if (i >= oldList.Length) {
            //         Debug.Log($"adding news {j}");
            //         // newlist has more, add remaining and break
            //         displayList.AddRange(newList.Skip(j - 1));
            //         break;
            //     }
            //     KeyVal oldkv = oldList[i];
            //     if (j >= newList.Length) {
            //         // existing list has more?
            //         // remove all valid ones
            //         // displayList = oldList.Skip(i).Where(kv => keys.Contains(kv.key)).ToList();
            //         // add remaining invalid ones
            //         IEnumerable<KeyVal> remaining = oldList.Skip(i);//.Where(kv => !kv.isValid);
            //         Debug.Log($"adding olds {i} {j} {remaining.Count()}");

            //         displayList.AddRange(remaining);
            //         break;
            //     }
            //     bool invalid = keys.Contains(oldkv.key);// || oldList.Count(kv => kv.key.Equals(oldkv.key)) > 1;
            //     Debug.Log($"checking {oldkv} {i} {j}");
            //     if (!invalid) {
            //         Debug.Log($"adding old");
            //         // adds the old one back if invalid
            //         displayList.Add(oldkv);
            //         // these are ignored by dict and keep position
            //         j--;
            //         continue;
            //     }
            //     if (j >= newList.Length) continue;

            //     KeyVal newkv = newList[j];
            //     Debug.Log($"adding new {newkv}");
            //     // replaces old one
            //     displayList.Add(newkv);
            // }
            // Debug.Log($"{lmax} was {oldList.Length} new {newList.Length} now {displayList.Count}");
#endif
        }
    }
}