// using System.Reflection;

namespace Kutil {
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypeDrawer : ShowAsChildPropertyDrawer {
        public override string childName => nameof(SerializedType.assemblyName);
    }
}