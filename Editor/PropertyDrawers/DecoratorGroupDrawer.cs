using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.Editor.PropertyDrawers {
    [CustomPropertyDrawer(typeof(DecoratorGroupEndAttribute))]
    public class DecoratorGroupEndDrawer : DecoratorDrawer {
        public override VisualElement CreatePropertyGUI() {
            var decorator = new VisualElement();
            decorator.AddToClassList(DecoratorGroupDrawer.decoratorEndClass);
            return decorator;
        }
    }
    [CustomPropertyDrawer(typeof(DecoratorGroupAttribute))]
    public class DecoratorGroupDrawer : DecoratorDrawer {
        public static readonly string decoratorClass = "kutil-decorator-group";
        public static readonly string decoratorEndClass = "kutil-decorator-drawer-group-end";

        VisualElement decorator;

        DecoratorGroupAttribute decoratorGroupAttribute => (DecoratorGroupAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            decorator = new VisualElement();
            decorator.name = "decorator-group";
            decorator.AddToClassList(DecoratorGroupDrawer.decoratorClass);
            decorator.RegisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            return decorator;
        }
        private void OnDecGeoChange(GeometryChangedEvent changedEvent) {
            decorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            MoveDecorators();
        }
        private void MoveDecorators() {
            VisualElement decoratorContainer = decorator.parent;
            if (decoratorContainer == null) {
                Debug.LogError($"{GetType().Name} {decorator.name} missing decorator container!");
                return;
            }
            if (decoratorContainer.childCount <= 1) {
                // no other decorators
                return;
            }
            int myDecIndex = decoratorContainer.IndexOf(decorator);
            if (myDecIndex < 0 || myDecIndex >= decoratorContainer.childCount - 1) {
                // decorator at end
                // no need to do anything
                // Debug.Log($"{myDecIndex} / {decoratorContainer.childCount}");
                return;
            }

            decorator.style.flexDirection = decoratorGroupAttribute.flexDirection;
            decorator.style.justifyContent = decoratorGroupAttribute.justifyContent;

            // move all decorators from this one to endgroup
            VisualElement endDecorator = decoratorContainer.Children().LastOrDefault(dec => dec.ClassListContains(decoratorEndClass));

            // take the containing elements
            var decoratorsToMove = decoratorContainer.Children().Skip(myDecIndex + 1);
            if (endDecorator != null) {
                decoratorsToMove = decoratorsToMove.TakeWhile(dec => dec != endDecorator).Append(endDecorator);
            }
            // Debug.Log("moving " + decoratorsToMove.ToStringFull(d => d.name, true));
            foreach (var dec in decoratorsToMove) {
                dec.style.flexGrow = decoratorGroupAttribute.elementsflexGrow;
            }
            foreach (var dec in decoratorsToMove.ToList()) {
                // decoratorContainer.Remove(dec);
                decorator.Add(dec);
                // Debug.Log("moved " + dec.name);
            }
        }
    }
}