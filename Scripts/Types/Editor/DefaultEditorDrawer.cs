using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

#if INSPECTOR_DEFAULT_UIE_OVERRIDE
/// <summary>
/// This is necessary for UIElement Property drawers to function in non-custom Inspectors.
/// In the future, this should become obsolete.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(Object), true, isFallback = true)]
public class DefaultEditorDrawer : UnityEditor.Editor {
    public override VisualElement CreateInspectorGUI() {
        var root = new VisualElement(); 
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        // var property = serializedObject.GetIterator();
        // if (property.NextVisible(true)) // Expand first child.
        // {
        //     do {
        //         var field = new PropertyField(property) { name = "PropertyField:" + property.propertyPath };

        //         if (property.propertyPath == "m_Script" && serializedObject.targetObject != null) {
        //             field.SetEnabled(false);
        //         }

        //         root.Add(field);
        //     } while (property.NextVisible(false));
        // }
        return root;
    }
}
#endif