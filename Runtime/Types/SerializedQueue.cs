using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Kutil {
    #if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedQueue<>))]
    public class SerializedQueueDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "serializedQueue";
    }
    #endif
    /// <summary>
    /// Serialized Queue. type T must be serializable.
    /// Editable in inspector.
    /// Only serialized in UnityEditor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class SerializedQueue<T> : Queue<T>, ISerializationCallbackReceiver {
    #if UNITY_EDITOR
        [SerializeField]
        private T[] serializedQueue;
    #endif
    
        // todo when editing in editor, doesnt work properly
        public void OnBeforeSerialize() {
    #if UNITY_EDITOR
            serializedQueue = this.ToArray();
    #endif
        }
    
        public void OnAfterDeserialize() {
    #if UNITY_EDITOR
            if (serializedQueue == null) {
                Debug.LogError("Failed to serialize Queue!");
                return;
            }
            this.Clear();
            for (int i = 0; i < serializedQueue.Length; i++) {
                this.Enqueue(serializedQueue[i]);
            }
            serializedQueue = null;
    #endif
        }
    
    }
}