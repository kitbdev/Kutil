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
    [CustomPropertyDrawer(typeof(PostFieldDecoratorAttribute))]
    public class PostFieldDecoratorDrawer : DecoratorDrawer {

        static readonly string unityDecoratorContainerClass = "unity-decorator-drawers-container";
        public static readonly string decoratorClass = "kutil-post-field-decorator";
        public static readonly string decoratorPostContainerClass = "kutil-post-decorator-drawer-container";

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
            postFieldDecorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            PropertyField propertyField = postFieldDecorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"{GetType().Name} failed to find containing property! {postFieldDecorator.name}");
                return;
            }
            // Debug.Log("MoveDecorators once "+propertyField.name);
            MoveDecorators(propertyField);
        }

        // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
        private void MoveDecorators(VisualElement root) {
            if (root == null) {
                Debug.LogError("MoveDecorators null");
                return;
            }
            VisualElement decoratorContainer = postFieldDecorator.parent;
            if (decoratorContainer == null) {
                Debug.LogError($"{GetType().Name} root {root.name} {postFieldDecorator.name} missing decorator container!");
                return;
            }
            if (decoratorContainer.childCount <= 1) {
                // no other decorators
                return;
            }
            int myDecIndex = decoratorContainer.IndexOf(postFieldDecorator);
            if (myDecIndex < 0 || myDecIndex >= decoratorContainer.childCount - 1) {
                // no need to move 
                // Debug.Log($"{myDecIndex} / {decoratorContainer.childCount}");
            } else {
                VisualElement newDecoratorContainer = new VisualElement();
                newDecoratorContainer.AddToClassList(unityDecoratorContainerClass);
                newDecoratorContainer.AddToClassList(decoratorPostContainerClass);
                decoratorContainer.parent.Add(newDecoratorContainer);
                // take the last elements
                var decoratorsToSteal = decoratorContainer.Children().Skip(myDecIndex + 1);
                foreach (var dec in decoratorsToSteal) {
                    decoratorContainer.Remove(dec);
                    newDecoratorContainer.Add(dec);
                }
            }
        }
    }
}