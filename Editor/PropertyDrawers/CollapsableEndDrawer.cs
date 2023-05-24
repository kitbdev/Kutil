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

        public CollapsableEndAttribute collapsableEndAttribute => (CollapsableEndAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            VisualElement collapsableEnd = new VisualElement();
            collapsableEnd.name = "collapsable-end-decorator";
            if (collapsableEndAttribute.connectedFieldName != null) {
                collapsableEnd.AddToClassList(CollapsableDrawer.collapsableEndSingleClass);
                collapsableEnd.userData = collapsableEndAttribute.connectedFieldName;
            } else {
                collapsableEnd.AddToClassList(CollapsableDrawer.collapsableEndClass);
            }
            return collapsableEnd;
        }
    }
}