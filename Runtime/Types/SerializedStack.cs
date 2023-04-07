

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedStack<>))]
    public class SerializedStackDrawer : ShowAsChildPropertyDrawer {
        // public override string childName => nameof(SerializedStack<int>.serialized);
        public override string childName => "serialized";
    }
#endif

    /// <summary>
    /// Serialized Stack. type T must be serializable.
    /// Editable in inspector.
    /// Only serialized in UnityEditor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class SerializedStack<T> : Stack<T>, ISerializationCallbackReceiver {

        [SerializeField]
        // [SerializeReference]
        private T[] serialized;

        public void OnAfterDeserialize() {
            if (serialized == null) {
                Debug.LogError("Failed to serialize Stack!");
                return;
            }
            this.Clear();
            for (int i = serialized.Length - 1; i >= 0; i--) {
                T item = serialized[i];
                this.Push(item);
            }
            serialized = null;
        }

        public void OnBeforeSerialize() {
            serialized = ToArray();
        }
    }
}