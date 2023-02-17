using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    // [CustomPropertyDrawer(typeof(Vector3DDraw))]
    public class Vector3DDrawDrawer: PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }
    }
}