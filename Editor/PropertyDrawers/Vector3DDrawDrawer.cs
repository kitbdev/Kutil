using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    // [CustomPropertyDrawer(typeof(Vector3DDraw))]
    public class Vector3DDrawDrawer: PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }
    }
    // todo generic 3d viewport VE that rotates around a point via mmb and handles camera matrix stuff
    // painter3D that uses the view matrix?
    // ! check out the mesh api first
}