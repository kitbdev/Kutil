using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Kutil.PropertyDrawers {
    // todo make this the default?
    [CustomPropertyDrawer(typeof(ReadOnlyArrayAttribute))]
    public class ReadOnlyArrayDrawer : DecoratorDrawer {

        public static readonly string readonlyClass = "kutil-readonly-array";

        public override VisualElement CreatePropertyGUI() {
            var root = new VisualElement();
            root.name = "ReadOnlyArray";
            root.AddToClassList(readonlyClass);

            root.RegisterCallback<GeometryChangedEvent>(ce => {
                PropertyField propertyField = root.GetFirstAncestorOfType<PropertyField>();
                if (propertyField == null) {
                    Debug.LogError($"ReadOnlyArray failed to find property! {root.name}");
                    return;
                }
                ReadOnlyDrawer.PropDisable(propertyField);
            });
            return root;
        }

    }
}