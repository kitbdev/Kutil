using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    
    // ref https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/PhysicsEditor/BoxColliderEditor.cs
    //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/EditModeTools/PrimitiveColliderTool.cs
    
    // non global editor tool, availability is determined when needed
    [EditorTool("Bounds Editor Tool", typeof(Component))]
    public class BoundsEditorTool : EditorTool {
    
        bool isAvailable = true;
        bool isBoundsInt = false;
    
        // d_EditCollider is what boxtool uses. d_RectTool is also box editing like
        public override GUIContent toolbarIcon =>
            new GUIContent(EditorGUIUtility.IconContent("d_EditCollider").image, "Bounds Tool");
    
        public override bool IsAvailable() => isAvailable;
        public override bool gridSnapEnabled => !isBoundsInt;
    
    
        // since this is a non-global tool, onenable is called everytime a different component is selected
        private void OnEnable() {
            // Debug.Log("btool onenable");
            UpdateAvailability();
        }
    
        void UpdateAvailability() {
            isAvailable = false;
            // Debug.Log("btool selection change " + Selection.objects.ToStringFull(null, true));
            // Debug.Log("btool update " + targets?.ToStringFull(null, true));
            // targets
            foreach (var obj in targets) {
                // Debug.Log($"check on {obj.name} b?{obj is Behaviour} t{obj.GetType().Name}");
                if (obj is not Component) continue;
                isAvailable = ReflectionHelper.HasAnyFieldsWithAttributeType<BoundsEditorToolAttribute>(obj.GetType());
                // Debug.Log($"is available: {isAvailable} on {obj.name}");
                if (isAvailable) break;
            }
            ToolManager.RefreshAvailableTools();
            if (!isAvailable) {
                // disable self
                ToolManager.RestorePreviousTool();
            }
        }
    
        // do actual tool in BoundsEditorToolDrawer SceneView.duringSceneGui instead
    }
    
    
    [CustomPropertyDrawer(typeof(BoundsEditorToolAttribute))]
    public class BoundsEditorToolDrawer : PropertyDrawer {
    
        public static readonly string boundsEditorToolClass = "kutil-bounds-tool";
        public static readonly string boundsEditorToolButtonClass = "kutil-bounds-tool-button";
    
        InspectorElement inspectorElement;
        VisualElement root;
        PropertyField propertyField;
        Toggle editButton;
    
        SerializedProperty property;
    
    
        bool toolEnabled;
        bool toolActive;
        Bounds bounds;
        Transform boundsTransform;
    
        readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();
    
    
        bool isBoundsInt => property.propertyType == SerializedPropertyType.BoundsInt;
        BoundsEditorToolAttribute boundsEditorToolAttribute => (BoundsEditorToolAttribute)attribute;
    
    
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            toolActive = false;
            boundsTransform = null;
    
            root = new();
            root.name = "kutil-bounds-editor-tool-drawer-container";
            root.AddToClassList(boundsEditorToolClass);
            string tooltip = "Edit bounds. \nHold alt to pin center, hold shift to scale uniformly";//, hold control to snap relatively
            root.tooltip = tooltip;
    
            this.property = property;
            propertyField = new PropertyField(property);
            root.Add(propertyField);
    
            // add button?
            VisualElement buttonAbsContainer = new VisualElement();
            buttonAbsContainer.name = "bounds-editor-button-container";
            buttonAbsContainer.style.position = Position.Absolute;
            buttonAbsContainer.style.left = 0;
            buttonAbsContainer.style.top = EditorGUIUtility.singleLineHeight * 1.5f;
            buttonAbsContainer.style.flexDirection = FlexDirection.Row;
            buttonAbsContainer.style.justifyContent = Justify.SpaceBetween;
            root.Add(buttonAbsContainer);
    
            if (boundsEditorToolAttribute.showEditButton) {
                editButton = new Toggle();
                editButton.AddToClassList(boundsEditorToolButtonClass);
                // make it look like a button
                editButton.AddToClassList(Button.ussClassName);
                // hide checkmark
                editButton.Q(null, Toggle.checkmarkUssClassName).style.display = DisplayStyle.None;
                // button.AddToClassList(ToolbarToggle.noLabelVariantUssClassName);
    
                // Image iconImage = new();
                Texture2D icon = EditorGUIUtility.FindTexture("d_EditCollider");
                editButton.style.backgroundImage = Background.FromTexture2D(icon);
                // button.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                editButton.style.backgroundSize = new BackgroundSize(icon.width, icon.height);
                // button.style.backgroundColor = 
                editButton.style.unitySliceBottom = 1;
                editButton.style.minWidth = icon.width;
                editButton.style.minHeight = icon.height;
                editButton.style.width = icon.width + 8 + 4;
                editButton.style.height = icon.height + 8;
                // button.style.width = EditorStyles.iconButton.fixedWidth;
                // button.style.height = EditorStyles.iconButton.fixedHeight;
                // EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.
                editButton.style.marginBottom = 0;
    
                editButton.tooltip = tooltip;
                // button.viewDataKey = root.name;
                editButton.RegisterValueChangedCallback((ce) => {
                    if (ce.newValue != toolActive) {
                        ToggleBoundsTool();
                    }
                    editButton.value = toolActive;
                });
                // button.clicked += ToggleBoundsTool;
    
                buttonAbsContainer.Add(editButton);
            }
    
            if (boundsEditorToolAttribute.showResetButton) {
                Button resetButton = new Button();
                // resetButton.AddToClassList(ToolbarToggle.ussClassName);
                // resetButton.AddToClassList(ToolbarToggle.noLabelVariantUssClassName);
                resetButton.text = "R";
                // resetButton.style.backgroundImage
                resetButton.tooltip = "Reset the bounds to (0,0,0) (1,1,1)";
                resetButton.clicked += ResetAndApplyBounds;
                buttonAbsContainer.Add(resetButton);
            }
    
            // ? set min and max per axis in attribute?
    
            root.RegisterCallback<GeometryChangedEvent>(SetupField);
            return root;
        }
        private void SetupField(GeometryChangedEvent evt) {
            root.UnregisterCallback<GeometryChangedEvent>(SetupField);
    
            // get inspector element to register an onvalidate callback
            inspectorElement = propertyField.GetFirstAncestorOfType<InspectorElement>();
            if (inspectorElement == null) {
                Debug.LogError($"{GetType().Name} - inspectorElement missing!");
                return;
            }
            // this properly responds to all changes
            // inspectorElement.RegisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
            root.RegisterCallback<DetachFromPanelEvent>(OnDetach);
    
            // i think this just adds the button in the inspector
            // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GUI/Tools/EditorToolGUI.cs
            // EditorGUILayout.EditorToolbarForTarget() 
            // no alternative? UnityEditor.UIElements.Toolbar a=new();
    
            SceneView.duringSceneGui += SceneGUI;
            ToolManager.activeToolChanged += OnToolChange;
    
            OnToolChange();
        }
    
        void OnDetach(DetachFromPanelEvent detachFromPanelEvent) {
            // inspectorElement.UnregisterCallback<SerializedPropertyChangeEvent>(OnUpdate);
    
            SceneView.duringSceneGui -= SceneGUI;
            ToolManager.activeToolChanged -= OnToolChange;
        }
    
        static bool IsBoundsToolActive() =>
            typeof(BoundsEditorTool).IsAssignableFrom(ToolManager.activeToolType);
        // ToolManager.activeToolType is typeof(BoundsEditorTool);
    
        static void ToggleBoundsTool() {
            if (IsBoundsToolActive()) {
                ToolManager.RestorePreviousTool();
            } else {
                ToolManager.SetActiveTool<BoundsEditorTool>();
            }
        }
    
        // [MenuItem("CONTEXT/Component/Validate Refs")]
        // [MenuItem("CONTEXT/ResetBounds")]
        // static void ResetBounds(MenuCommand command){
            // get the relevent field
            // probably wont work per field
            // command.context
            // something with uitoolkit maybe? 
        //     //?
        // }
        void ResetAndApplyBounds() {
            if (isBoundsInt) {
                bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one).AsBounds();
            } else {
                bounds = new Bounds(Vector3.zero, Vector3.one * 2);
            }
            // todo reset to object bounds?
            // boundsTransform.gameObject.
            UpdateValueFromBounds();
        }
    
        void OnToolChange() {
            bool wasActive = toolActive;
            toolActive = IsBoundsToolActive();
    
            if (editButton != null) {
                // boundsEditorToolAttribute.showEditButton is false
                editButton.SetValueWithoutNotify(toolActive);
            }
    
            if (property.propertyType != SerializedPropertyType.Bounds && property.propertyType != SerializedPropertyType.BoundsInt) {
                // invalid field type, only works on Bounds and BoundsInt
                toolEnabled = false;
                return;
            }
    
            toolEnabled = true;
    
            if (property.serializedObject.targetObject is GameObject go) {
                boundsTransform = go.transform;
            } else if (property.serializedObject.targetObject is Component c) {
                boundsTransform = c.transform;
            }
    
            if (toolActive) {
                // activate
                // Debug.Log("BoundsEditorToolDrawer activate");
                // Debug.Log(property.serializedObject.targetObject);
            }
            if (!toolActive && wasActive) {
                // deactivate
                // boundsTransform = null;
            }
        }
    
        void SceneGUI(SceneView sceneView) {
            if (!toolEnabled) return;
    
            // Debug.Log($"BoundsEditorToolDrawer scenegui active:{toolActive} t:{boundsTransform}");
    
            if (boundsTransform == null || Mathf.Approximately(boundsTransform.lossyScale.sqrMagnitude, 0f)) return;
    
            Matrix4x4 transformMatrix;
            if (boundsEditorToolAttribute.useTransformScaleAndRotation) {
                transformMatrix = Matrix4x4.TRS(boundsTransform.position, boundsTransform.rotation, Vector3.one);
            } else {
                transformMatrix = Matrix4x4.identity;
            }
            if (!toolActive) {
                // draw regular cube when tool not activated
                if (boundsEditorToolAttribute.showBoundsWhenInactive) {
                    UpdateBounds();
                    using (new Handles.DrawingScope(transformMatrix)) {
                        // ? disabled color instead
                        Handles.color = (Handles.UIColliderHandleColor);
    
                        Vector3 center;
                        Vector3 size;
                        if (boundsEditorToolAttribute.useTransformScaleAndRotation) {
                            center = TransformColliderCenterToHandleSpace(boundsTransform, bounds.center);
                            size = Vector3.Scale(bounds.size, boundsTransform.lossyScale);
                        } else {
                            center = bounds.center + boundsTransform.position;
                            size = bounds.size;
                        }
    
                        Handles.DrawWireCube(center, size);
                    }
                }
                return;
            }
    
            UpdateBounds();
    
            // relative box matrix is center multiplied by transform's matrix with custom postmultiplied lossy scale matrix
            using (new Handles.DrawingScope(transformMatrix)) {
    
                if (boundsEditorToolAttribute.useTransformScaleAndRotation) {
                    boundsHandle.center = TransformColliderCenterToHandleSpace(boundsTransform, bounds.center);
                    boundsHandle.size = Vector3.Scale(bounds.size, boundsTransform.lossyScale);
                } else {
                    // boundsHandle.center = Handles.inverseMatrix * bounds.center;
                    boundsHandle.center = bounds.center + boundsTransform.position;
                    boundsHandle.size = bounds.size;
                }
    
                // can change color or which axes to have handles on
                boundsHandle.SetColor(Handles.UIColliderHandleColor);
    
                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    // dont need undo cause serialized prop handles it
                    // Undo.RecordObject(obj, string.Format("Modify {0} Bounds", ObjectNames.NicifyVariableName(target.GetType().Name)));
                    // Debug.Log("edited " + boundsTransform.name);
    
                    if (boundsEditorToolAttribute.useTransformScaleAndRotation) {
                        bounds.center = TransformHandleCenterToColliderSpace(boundsTransform, boundsHandle.center);
                        Vector3 size = Vector3.Scale(boundsHandle.size, InvertScaleVector(boundsTransform.lossyScale));
                        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
                        bounds.size = size;
                    } else {
                        // bounds.center = Handles.matrix * boundsHandle.center;
                        bounds.center = boundsHandle.center - boundsTransform.position;
                        bounds.size = boundsHandle.size;
                    }
    
                    UpdateValueFromBounds();
                }
            }
        }
    
        void UpdateBounds() {
            // update bounds
            if (property.propertyType == SerializedPropertyType.Bounds) {
                bounds = property.boundsValue;
            } else if (property.propertyType == SerializedPropertyType.BoundsInt) {
                bounds = property.boundsIntValue.AsBounds();
            }
        }
        void UpdateValueFromBounds() {
            property.serializedObject.Update();
            if (property.propertyType == SerializedPropertyType.Bounds) {
                property.boundsValue = bounds;
            } else if (property.propertyType == SerializedPropertyType.BoundsInt) {
                property.boundsIntValue = bounds.AsBoundsIntRounded();
            }
            property.serializedObject.ApplyModifiedProperties();
        }
    
        // utils
        protected static Vector3 TransformColliderCenterToHandleSpace(Transform colliderTransform, Vector3 colliderCenter) {
            return Handles.inverseMatrix * (colliderTransform.localToWorldMatrix * colliderCenter);
        }
    
        protected static Vector3 TransformHandleCenterToColliderSpace(Transform colliderTransform, Vector3 handleCenter) {
            return colliderTransform.localToWorldMatrix.inverse * (Handles.matrix * handleCenter);
        }
        protected static Vector3 InvertScaleVector(Vector3 scaleVector) {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];
    
            return scaleVector;
        }
    }
}