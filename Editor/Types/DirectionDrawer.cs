namespace Kutil.Editor.PropertyDrawers {
    [UnityEditor.CustomPropertyDrawer(typeof(Direction))]
    public class DirectionDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "dir";
    }
}