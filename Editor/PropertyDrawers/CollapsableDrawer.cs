using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil.PropertyDrawers {
    /// <summary>
    /// create a foldout from the attribute to the next CollapsableAttribute or the next decorator(header or space)
    /// </summary>
    [CustomPropertyDrawer(typeof(CollapsableAttribute))]
    public class CollapsableDrawer : DecoratorDrawer {

        // ? can we find these elsewhere
        static readonly string unityDecoratorContainerClass = "unity-decorator-drawers-container";
        static readonly string unityHeaderDecoratorClass = "unity-header-drawer__label";
        static readonly string unitySpaceDecoratorClass = "unity-space-drawer";

        public static readonly string collapsableDecoratorClass = "kutil-collapsable-drawer-decorator";
        public static readonly string collapsableBaseClass = "kutil-collapsable-drawer";
        public static readonly string collapsableFoldoutClass = "kutil-collapsable-drawer-foldout";
        public static readonly string collapsableClass = "kutil-collapsable-property";
        public static readonly string collapsableEndClass = "kutil-collapsable-end-marker";

        VisualElement collapsableDecorator;


        // todo horizontal layout option?


        CollapsableAttribute collapsable => (CollapsableAttribute)attribute;

        public override VisualElement CreatePropertyGUI() {
            collapsableDecorator = new VisualElement();
            collapsableDecorator.name = "collapsable-decorator";
            collapsableDecorator.AddToClassList(collapsableDecoratorClass);

            collapsableDecorator.RegisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            return collapsableDecorator;
        }
        private void OnDecGeoChange(GeometryChangedEvent changedEvent) {
            collapsableDecorator.UnregisterCallback<GeometryChangedEvent>(OnDecGeoChange);
            PropertyField propertyField = collapsableDecorator.GetFirstAncestorOfType<PropertyField>();
            if (propertyField == null) {
                Debug.LogError($"CollapsableDrawer failed to find containing property! {collapsableDecorator.name}");
                return;
            }
            // Debug.Log("collapsable once "+propertyField.name);
            CreateCollapsable(propertyField);
        }

        // ! note this modifies the inspector's visual tree hierarchy. hopefully it doesnt cause any problems
        private void CreateCollapsable(VisualElement root) {
            if (root == null) {
                Debug.LogError("CreateCollapsable null");
                return;
            }
            if (root.ClassListContains(collapsableClass)) {
                Debug.LogError($"CreateCollapsable root {root.name} is already collapsable!");
                return;
            }
            root.AddToClassList(collapsableClass);
            // Debug.Log($"creating collapsable for {root.name}");
            // after layout
            VisualElement collapsableBase = new VisualElement();
            collapsableBase.name = $"{root.name}__collapsable";
            collapsableBase.AddToClassList(collapsableBaseClass);

            VisualElement decoratorContainer = collapsableDecorator.parent;
            if (decoratorContainer == null) {
                Debug.LogError($"CreateCollapsable root {root.name} {collapsableDecorator.name} missing decorator container!");
                return;
            }
            if (decoratorContainer.childCount > 1) {
                // there are other decorators than just us
                int collapsableDecIndex = decoratorContainer.IndexOf(collapsableDecorator);
                if (collapsableDecIndex <= 0) {//} || collapsableDecIndex>=decoratorContainer.childCount) {
                    // no need to add our own decorator
                } else {
                    VisualElement newDecoratorContainer = new VisualElement();
                    newDecoratorContainer.AddToClassList(unityDecoratorContainerClass);
                    collapsableBase.Add(newDecoratorContainer);
                    // take the first n elements
                    var decoratorsToSteal = decoratorContainer.Children().Take(collapsableDecIndex);
                    //? move later? 
                    foreach (var dec in decoratorsToSteal) {
                        decoratorContainer.Remove(dec);
                        newDecoratorContainer.Add(dec);
                    }
                }
            }

            // insert a Foldout
            Foldout foldout = new Foldout();
            collapsableBase.Add(foldout);
            // foldout.name = $"{root.name}__collapsable-foldout";
            // this needs to be set to create the label element
            foldout.text = "foldout init...";
            // value will be overwritten if viewdatakey is set and found, to keep state
            foldout.value = !collapsable.startCollapsed;

            // styling
            // Label label = foldout.Q<Label>();//null, Foldout.textUssClassName
            Label label = foldout.Q<Label>(null, Foldout.textUssClassName);
            if (label == null) {
                Debug.Log(foldout.Children().ToStringFull(c => c.name));
                Debug.LogError($"Collapsable drawer {root.name} invalid Foldout no label!");
                return;
            }
            if (collapsable.text != null) {
                foldout.text = collapsable.text;
                label.enableRichText = collapsable.useRichText;
                if (!collapsable.useRichText) {
                    // make look like a header by default
                    // label.AddToClassList(unityHeaderDecoratorClass);
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
            }
            if (collapsable.hideFoloutTriangle) {
                // remove checkmark triangle
                VisualElement foldoutCheckmark = foldout.Q(null, Foldout.checkmarkUssClassName);
                if (foldoutCheckmark == null) {
                    Debug.LogError($"Collapsable drawer {root.name} invalid Foldout no foldoutCheckmark!");
                    return;
                }
                foldoutCheckmark.style.display = DisplayStyle.None;
                // fix label layout, make it even and not negative
                label.style.paddingLeft = 2;
                label.style.marginLeft = 0;
            }
            if (collapsable.dontIndent) {
                // remove indent
                VisualElement foldoutContainer = foldout.Q(null, Foldout.contentUssClassName);
                if (foldoutContainer == null) {
                    Debug.LogError($"Collapsable drawer {root.name} invalid Foldout no foldoutContainer!");
                    return;
                }
                foldoutContainer.style.marginLeft = new StyleLength(0f);
            }
            foldout.AddToClassList(collapsableFoldoutClass);


            // get parent
            // parent should be Inspector element (unless nested?)
            InspectorElement inspectorElement = root.GetFirstAncestorOfType<InspectorElement>();
            if (inspectorElement == null) {
                Debug.LogError("cannot find inspector element");
                return;
            }

            // todo nestable?
            VisualElement parent = inspectorElement;
            VisualElement cPropVE = root;
            while (cPropVE.parent != parent) {
                cPropVE = cPropVE.parent;
            }

            // get all top level children to move into the foldout
            VisualElement[] childs = parent.Children()
                    .SkipWhile((el) => el != cPropVE)
                    .TakeWhile((el, i) => {
                        // return true to include this element
                        if (i == 0) return true;

                        // todo end only if before field?
                        bool isEndMarker = el.Q(null, collapsableEndClass) != null;
                        if (isEndMarker) return false;

                        // dont allow if is another collapsable
                        bool isCollapsable = el.Q(null, collapsableClass) != null
                                            || el.Q(null, collapsableBaseClass) != null
                                            || el.Q(null, collapsableDecoratorClass) != null;
                        if (isCollapsable) return false;

                        // todo test all cases
                        bool isHeaderDec = el.Q(null, unityHeaderDecoratorClass) != null;
                        if (!collapsable.includeHeaders && isHeaderDec) return false;

                        // bool takeHeaderDec = collapsable.includeHeaders || isHeaderDec;
                        bool isSpaceDec = el.Q(null, unitySpaceDecoratorClass) != null;
                        if (!collapsable.includeSpaces && isSpaceDec) return false;

                        // bool takeSpaceDec = collapsable.includeSpaces || isSpaceDec;
                        bool isDec = el.Q(null, unityDecoratorContainerClass) != null;
                        bool isOtherDec = isDec && !isSpaceDec && !isHeaderDec;
                        if (!collapsable.includeOtherDecorators && isOtherDec) return false;
                        // bool takeOtherDec = isSpaceDec || isHeaderDec || (collapsable.includeOtherDecorators || isDec);
                        // bool takeDec = takeSpaceDec && takeHeaderDec && takeOtherDec;
                        // ? any other end markers?
                        // return takeDec;
                        return true;
                    })
                    .ToArray();

            // Debug.Log($"par:{parent.name} c:{cPropVE.name} involved:{childs.ToStringFull(ve => ve.name, true)}");
            // return;
            if (childs.Count() == 0) return;
            int placeIndex = parent.IndexOf(childs.First());
            parent.Insert(placeIndex, collapsableBase);
            collapsableBase.name = $"Collapsable_{childs.First().name}_to_{childs.Last().name}";
            // set viewdata uniquely to make foldout remember folded state
            // https://forum.unity.com/threads/can-someone-explain-the-view-data-key-and-its-uses.855145/#post-5638936
            foldout.viewDataKey = $"{collapsableBaseClass}_{inspectorElement.name}_{childs.First().name}";
            // Debug.Log(foldout.viewDataKey);

            // move to foldout
            foreach (var child in childs) {
                parent.Remove(child);
                foldout.Add(child);
            }
        }
    }
}
/*
test

    [Header("test")]
    [Collapsable("Walking")]
    public int t1;
    [Collapsable("Walking")]
    [Header("test")]
    public int t2;
    [Space]
    [Collapsable("Walking")]
    public int t3;
    [Collapsable("Walking")]
    [Space]
    public int t4;
    [Space]
    [Collapsable("Walking")]
    [Space]
    public int t5;
*/
/* old imgui version
[CustomPropertyDrawer(typeof(FoldStartAttribute))]
    public class FoldStartDrawer : PropertyDrawer {
        // todo keep state?
        bool foldOpen = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            FoldStartAttribute foldStart = (FoldStartAttribute)attribute;
            GUIContent header = foldStart.header == null ? label : new GUIContent(foldStart.header);
            foldOpen = EditorGUI.BeginFoldoutHeaderGroup(position, foldOpen, header);
            position.height -= EditorGUIUtility.singleLineHeight;
            position.y += EditorGUIUtility.singleLineHeight;
            if (foldOpen) {
                EditorGUI.PropertyField(position, property, label);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (foldOpen) height += base.GetPropertyHeight(property, label);
            return height;
        }
    }
 /// <summary>
    /// Ends a foldout section.
    /// Must be paired with a FoldStart!
    /// </summary>
    [CustomPropertyDrawer(typeof(FoldEndAttribute))]
    public class FoldEndDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndFoldoutHeaderGroup();
        }
    }

*/