

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {
    [System.Serializable]
    public class SerializableStack<T> : Stack<T>, ISerializationCallbackReceiver {

        [SerializeField]
        // [SerializeReference]
        private T[] serialized;

        public void OnAfterDeserialize() {
            if (serialized==null) {
                Debug.LogError("Failed to serialize Stack!");
                return;
            }
            this.Clear();
            for (int i = serialized.Length - 1; i >= 0; i--) {
                T item = serialized[i];
                this.Push(item);
            }
        }

        public void OnBeforeSerialize() {
            serialized = ToArray();
        }
    }
}