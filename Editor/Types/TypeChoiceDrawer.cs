// using System.Reflection;

namespace Kutil {
    [UnityEditor.CustomPropertyDrawer(typeof(TypeChoice<>))]
    public class TypeChoiceDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "_selectedType";
    }
}