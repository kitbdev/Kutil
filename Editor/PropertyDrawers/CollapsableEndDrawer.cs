using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(CollapsableEndAttribute))]
    public class CollapsableEndDrawer : DecoratorDrawer {

        CollapsableEndAttribute collapsableEndAttribute => (CollapsableEndAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            VisualElement collapsableEnd = new VisualElement();
            collapsableEnd.name = "collapsable-end-decorator";
            collapsableEnd.AddToClassList(CollapsableDrawer.collapsableEndClass);
            return collapsableEnd;
        }
    }
}