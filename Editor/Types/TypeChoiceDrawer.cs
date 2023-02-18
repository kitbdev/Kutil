// using System.Reflection;

namespace Kutil.PropertyDrawers {
    [UnityEditor.CustomPropertyDrawer(typeof(TypeChoice<>))]
    public class TypeChoiceDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "_selectedType";
    }
}