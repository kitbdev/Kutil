namespace Kutil {
    [UnityEditor.CustomPropertyDrawer(typeof(Direction))]
    public class DirectionDrawer : ShowAsChildPropertyDrawer {
        public override string childName => "dir";
    }
}