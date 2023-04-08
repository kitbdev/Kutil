// using System.Reflection;

namespace Kutil.Editor.PropertyDrawers {
    [UnityEditor.CustomPropertyDrawer(typeof(TypeChoice<>))]
    public class TypeChoiceDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "_selectedType";
    }
}