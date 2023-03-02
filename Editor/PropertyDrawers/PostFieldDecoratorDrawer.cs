using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(PostFieldDecoratorAttribute))]
    public class PostFieldDecoratorDrawer : DecoratorDrawer {

        public static readonly string decoratorClass = "kutil-post-field-decorator";

        VisualElement postFieldDecorator;

        PostFieldDecoratorAttribute postFieldDecoratorAttribute => (PostFieldDecoratorAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            postFieldDecorator = new VisualElement();
            postFieldDecorator.name = "post-field-decorator";
            postFieldDecorator.AddToClassList(decoratorClass);

            postFieldDecorator.RegisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            return postFieldDecorator;
        }
        private void OnDecGeoChange(GeometryChangedEvent changedEvent) {
            PropertyField propertyField = postFieldDecorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"PostFieldDecoratorDrawer failed to find containing property! {postFieldDecorator.name}");
                return;
            }
            // Debug.Log("MoveDecorators once "+propertyField.name);
            MoveDecorators(propertyField);
            postFieldDecorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
        }

        // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
        private void MoveDecorators(VisualElement root) {
            if (root == null) {
                Debug.LogError("MoveDecorators null");
                return;
            }
            // todo
        }
    }
}