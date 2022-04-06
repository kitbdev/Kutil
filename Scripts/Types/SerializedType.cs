// using System.Reflection;
using UnityEngine;
using System;

namespace Kutil {
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypeDrawer : ShowAsChildPropertyDrawer {
        public override string childName => nameof(SerializedType.assemblyName);
    }
#endif
    /// <summary>
    /// A serializable System.Type
    /// </summary>
    [Serializable]
    public class SerializedType : ISerializationCallbackReceiver {

        [SerializeField, ReadOnly]
        internal string assemblyName;
        public Type type;

        public SerializedType() {
            this.type = null;
        }
        public SerializedType(Type type) {
            this.type = type;
        }

        public void OnBeforeSerialize() {
            assemblyName = type?.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize() {
            type = Type.GetType(assemblyName);
        }

        public override string ToString() {
            return type == null ? "None" : type.FullName;
        }

        public override int GetHashCode() {
            return type != null ? type.GetHashCode() : 0;
        }
        public bool Equals(SerializedType other) {
            if (type == null && other?.type == null) return true;
            if (type == null || other?.type == null) return false;
            return type.Equals(other.type);
        }
        public override bool Equals(System.Object obj) {
            if (obj is SerializedType st) {
                return this.Equals(st);
            }
            return false;
        }

        public static bool operator ==(SerializedType a, SerializedType b) {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializedType a, SerializedType b) => !(a == b);
        public static implicit operator Type(SerializedType st) => st.type;
        public static implicit operator SerializedType(Type t) => new SerializedType(t);

        // todo? store as bytes instead?
        // any other data needed? generic, etc
        // public byte[] data;
        // public static System.Type Read(BinaryReader aReader) {
        //         var paramCount = aReader.ReadByte();
        //         if (paramCount == 0xFF)
        //             return null;
        //         var typeName = aReader.ReadString();
        //         var type = System.Type.GetType(typeName);
        //         if (type == null)
        //             throw new System.Exception("Can't find type; '" + typeName + "'");
        //         if (type.IsGenericTypeDefinition && paramCount > 0) {
        //             var p = new System.Type[paramCount];
        //             for (int i = 0; i < paramCount; i++) {
        //                 p[i] = Read(aReader);
        //             }
        //             type = type.MakeGenericType(p);
        //         }
        //         return type;
        //     }

        //     public static void Write(BinaryWriter aWriter, System.Type aType) {
        //         if (aType == null) {
        //             aWriter.Write((byte)0xFF);
        //             return;
        //         }
        //         if (aType.IsGenericType) {
        //             var t = aType.GetGenericTypeDefinition();
        //             var p = aType.GetGenericArguments();
        //             aWriter.Write((byte)p.Length);
        //             aWriter.Write(t.AssemblyQualifiedName);
        //             for (int i = 0; i < p.Length; i++) {
        //                 Write(aWriter, p[i]);
        //             }
        //             return;
        //         }
        //         aWriter.Write((byte)0);
        //         aWriter.Write(aType.AssemblyQualifiedName);
        //     }


        //     public void OnBeforeSerialize() {
        //         using (var stream = new MemoryStream())
        //         using (var w = new BinaryWriter(stream)) {
        //             Write(w, type);
        //             data = stream.ToArray();
        //         }
        //     }

        //     public void OnAfterDeserialize() {
        //         using (var stream = new MemoryStream(data))
        //         using (var r = new BinaryReader(stream)) {
        //             type = Read(r);
        //         }
        //     }
    }
}