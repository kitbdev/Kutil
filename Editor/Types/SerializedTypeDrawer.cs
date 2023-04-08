// using System.Reflection;

namespace Kutil.Editor.PropertyDrawers {
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypeDrawer : ShowAsChildPropertyDrawer {
        public override string childName => nameof(SerializedType.assemblyName);
    }
}