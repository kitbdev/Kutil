using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

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
            // isAvailable = false;
            UpdateAvailability();
        }
        private void OnDisable() {
            // disable tool, box collider does it 
            //? not working
            // if (ToolManager.IsActiveTool(this)) {
            //     ToolManager.RestorePreviousTool();
            // }
        }

        // todo this is called every time a selection is made, is reflection too costly? not really
        void UpdateAvailability() {
            isAvailable = false;
            // Debug.Log("btool selection change " + Selection.objects.ToStringFull(null, true));
            // Debug.Log("btool update " + targets?.ToStringFull(null, true));
            // targets

            foreach (var obj in targets) {
                // Debug.Log($"check on {obj.name} b?{obj is Behaviour} t{obj.GetType().Name}");
                isAvailable = IsAvailable(obj);
                if (isAvailable) break;
            }
            ToolManager.RefreshAvailableTools();
            if (!isAvailable) {
                // disable self
                ToolManager.RestorePreviousTool();
            }
        }
        /// <summary>
        /// Does this Object have fields with BoundsEditorToolAttribute
        /// Uses reflection.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsAvailable(UnityEngine.Object obj) {
            if (obj is not Component) return false;
            return IsAvailableCheckSO(obj);
            // return ReflectionHelper.HasAnyFieldsWithAttributeType<BoundsEditorToolAttribute>(obj.GetType());
            // Debug.Log($"is available: {isAvailable} on {obj.name}");
            // todo? make sure those fields are of Bounds or BoundsInt type?
        }

        static bool IsAvailableCheckSO(UnityEngine.Object obj) {
            if (obj is not Component) return false;
            using (SerializedObject so = new SerializedObject(obj)) {
                bool contains = false;
                // var props = new List<SerializedProperty>();
                so.ForEachProperty((prop) => {
                    if (prop.propertyType != SerializedPropertyType.Bounds &&
                        prop.propertyType != SerializedPropertyType.BoundsInt) {
                        // continue
                        return null;
                    }
                    // Debug.Log("checking " + prop.propertyPath);
                    var fieldInfo = prop.GetFieldInfoOnProp();
                    if (fieldInfo != null && fieldInfo.HasAttribute<BoundsEditorToolAttribute>()) {
                        // return fieldInfo;
                        // Debug.Log("found!");
                        // props.Add(prop.Copy());
                        contains = true;
                        return SerializedPropertyExtensions.PropIterFlags.Break;
                    }
                    // skip bounds internal properties
                    return SerializedPropertyExtensions.PropIterFlags.SkipChildren;
                    // return null;
                }, true);
                // var filtered = props.Where(p => p.propertyType == SerializedPropertyType.Bounds ||
                // p.propertyType == SerializedPropertyType.BoundsInt);
                // return props.Count() > 0;
                return contains;
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

            this.property = property;
            propertyField = new PropertyField(property);

            if (property.propertyType != SerializedPropertyType.Bounds && property.propertyType != SerializedPropertyType.BoundsInt) {
                // invalid field type, only works on Bounds and BoundsInt
                return propertyField;
            }

            // todo array support
            if (property.IsInAnyArray() || property.isArray) {
                // arrays not supported
                return propertyField;
            }

            // prevent the user from trying to enable the tool here where it is unavailable for this target
            // ToolManager.
            if (!BoundsEditorTool.IsAvailable(property.serializedObject.targetObject)) {
                // Debug.LogWarning("BoundsEditorTool is not available for " + property.serializedObject.targetObject.name + " " + property.serializedObject.targetObject.GetType().Name);
                return propertyField;
            }


            root = new();
            root.name = "kutil-bounds-editor-tool-drawer-container";
            root.AddToClassList(boundsEditorToolClass);
            string tooltip = "Edit bounds. \nHold alt to pin center, hold shift to scale uniformly";//, hold control to snap relatively
            root.tooltip = tooltip;

            root.Add(propertyField);


            // add button?
            VisualElement buttonAbsContainer = new VisualElement();
            buttonAbsContainer.name = "bounds-editor-button-container";
            buttonAbsContainer.style.position = Position.Absolute;
            buttonAbsContainer.style.left = 0;
            // todo get bounds label position and move under or something
            float topDist = 1.5f;
            if (property.IsElementInArray()) {
                topDist = 2.5f;
            }
            buttonAbsContainer.style.top = EditorGUIUtility.singleLineHeight * topDist;
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

            // context menu
            ContextualMenuManipulator m = new ContextualMenuManipulator(OnContextMenuEvent);
            // propertyField.AddManipulator(m);
            // try to add to existing context handler so menu gets appended
            // VisualElement contextField = propertyField;
            // contextField = propertyField.Q(null, "unity-base-field");
            // if (contextField == null) contextField = propertyField;
            // Debug.Log("c:" + contextField.name);
            root.AddManipulator(m);
            // root.RegisterCallback<ContextualMenuPopulateEvent>(OnContextMenuEvent);


            // ? set min and max per axis in attribute?

            // todo? maybe use this event - AttachToPanelEvent
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
            // root.UnregisterCallback<ContextualMenuPopulateEvent>(OnContextMenuEvent);

            SceneView.duringSceneGui -= SceneGUI;
            ToolManager.activeToolChanged -= OnToolChange;
        }

        void OnContextMenuEvent(ContextualMenuPopulateEvent ce) {
            // Debug.Log("OnContextMenuEvent");

            // todo add copy and paste options

            //https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-Command-Events.html
            // ce.menu.AppendAction("Copy", a => ProcessCmd("Copy"), DropdownMenuAction.AlwaysEnabled);
            // ce.menu.AppendAction("Paste", a => ProcessCmd("Paste"), ValidateCmd("Paste"));
            // using (ContextualMenuPopulateEvent evt = ContextualMenuPopulateEvent.GetPooled(ce, ce.menu, ce.target, null)) {
            //     // evt.target = propertyField;
            //     // send down
            //     propertyField.SendEvent(ce);
            // }
            ce.menu.AppendSeparator();
            ce.menu.AppendAction("Toggle Bounds tool", a => ToggleBoundsTool(), (a) => IsBoundsToolActive() ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            ce.menu.AppendAction("Recenter to (0,0,0)", a => RecenterAndApplyBounds(), DropdownMenuAction.AlwaysEnabled);
            ce.menu.AppendAction("Make Cube", a => MakeCubeApplyBounds(), DropdownMenuAction.AlwaysEnabled);
            ce.menu.AppendAction("Reset", a => ResetAndApplyBounds(), DropdownMenuAction.AlwaysEnabled);
            // ce.menu.AppendAction("PD", a => { ce.PreventDefault(); }, DropdownMenuAction.AlwaysEnabled);

            // ? calling this gives the regular copy paste
            // ce.PreventDefault();
            // ce.menu.InsertAction(0, "test", a => ToggleBoundsTool(), DropdownMenuAction.Status.Normal);

            // ce.StopPropagation();
            // ce.StopImmediatePropagation();
        }

        static bool IsBoundsToolActive() =>
            typeof(BoundsEditorTool).IsAssignableFrom(ToolManager.activeToolType);
        // ToolManager.activeToolType is typeof(BoundsEditorTool);

        static void ToggleBoundsTool() {
            if (IsBoundsToolActive()) {
                ToolManager.RestorePreviousTool();
            } else {
                // ToolManager.RefreshAvailableTools();
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

        // void ProcessCmd(string command) {
        //     using (ExecuteCommandEvent evt = ExecuteCommandEvent.GetPooled(command)) {
        //         evt.target = propertyField;
        //         propertyField.SendEvent(evt);
        //     }
        // }
        // void ValidateCmd(string command) {
        //     using (ValidateCommandEvent evt = ValidateCommandEvent.GetPooled(command)) {
        //         evt.target = propertyField;
        //         propertyField.SendEvent(evt);
        //     }
        // }
        // DropdownMenuAction.Status CanPaste(DropdownMenuAction action) {
        //     if (EditorGUIUtility.systemCopyBuffer == "b") {
        //         return DropdownMenuAction.Status.Normal;
        //     }
        //     return DropdownMenuAction.Status.Disabled;
        // }
        // void OnExecuteCommand(ExecuteCommandEvent evt) {
        //     if (evt.commandName == "Copy") {
        //         if (isBoundsInt) {
        //             EditorGUIUtility.systemCopyBuffer = bounds.AsBoundsIntRounded();
        //         } else {
        //             EditorGUIUtility.systemCopyBuffer = bounds;
        //         }
        //         evt.StopPropagation();
        //     } else if (evt.commandName == "Paste" && !string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer)) {
        //         fruits.Add(EditorGUIUtility.systemCopyBuffer);
        //         m_ListView.RefreshItems();
        //         evt.StopPropagation();
        //     }
        // }

        // void OnValidateCommand(ValidateCommandEvent evt) {
        //     if (evt.commandName == "Copy") {
        //         evt.StopPropagation();
        //     } else if (evt.commandName == "Paste" && !string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer)) {
        //         evt.StopPropagation();
        //     }
        // }

        void MakeCubeApplyBounds() {
            bounds.size = new(bounds.size.x, bounds.size.x, bounds.size.x);
            UpdateValueFromBounds();
        }
        void RecenterAndApplyBounds() {
            bounds.center = Vector3.zero;
            UpdateValueFromBounds();
        }
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
            if (boundsEditorToolAttribute.scale <= 0) {
                boundsEditorToolAttribute.scale = 1f;
            }
            // boundsEditorToolAttribute.scale = 0.5f;
            Vector3 viewScale = boundsEditorToolAttribute.scale * Vector3.one;
            bool useRotScale = boundsEditorToolAttribute.useTransformScaleAndRotation;
            // scale must be 0 for handle to work correctly
            if (useRotScale) {
                transformMatrix = Matrix4x4.TRS(boundsTransform.position, boundsTransform.rotation, Vector3.one);
            } else {
                // transformMatrix = Matrix4x4.TRS(boundsTransform.position, Quaternion.identity, Vector3.one);
                transformMatrix = Matrix4x4.Translate(boundsTransform.position);
            }
            // Debug.Log("scale:" + scale + " " + boundsEditorToolAttribute.scale+" "+transformMatrix.lossyScale);
            if (!toolActive) {
                // draw regular cube when tool not activated
                if (boundsEditorToolAttribute.showBoundsWhenInactive) {
                    UpdateBounds();
                    using (new Handles.DrawingScope(transformMatrix)) {
                        // ? disabled color instead
                        Handles.color = (Handles.UIColliderHandleColor);

                        var handleBounds = TransformBoundsToHandleSpace(bounds, boundsTransform, viewScale, useRotScale);

                        Handles.DrawWireCube(handleBounds.center, handleBounds.size);
                    }
                }
                return;
            }

            UpdateBounds();

            // relative box matrix is center multiplied by transform's matrix with custom postmultiplied lossy scale matrix
            using (new Handles.DrawingScope(transformMatrix)) {

                var handleBounds = TransformBoundsToHandleSpace(bounds, boundsTransform, viewScale, useRotScale);
                boundsHandle.center = handleBounds.center;
                boundsHandle.size = handleBounds.size;

                // can change color or which axes to have handles on
                boundsHandle.SetColor(Handles.UIColliderHandleColor);

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck()) {
                    // dont need undo cause serialized prop handles it
                    // Undo.RecordObject(obj, string.Format("Modify {0} Bounds", ObjectNames.NicifyVariableName(target.GetType().Name)));
                    // Debug.Log("edited " + boundsTransform.name);

                    bounds = TransformHandleToBoundsSpace(new(boundsHandle.center, boundsHandle.size), boundsTransform, viewScale, useRotScale);

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

        protected static Bounds TransformBoundsToHandleSpace(Bounds bounds, Transform transform, Vector3 viewScale, bool useRS) {
            if (useRS) {
                return new Bounds(
                    center: Handles.inverseMatrix * (transform.localToWorldMatrix * Vector3.Scale(bounds.center, viewScale)),
                    // size: Handles.inverseMatrix.MultiplyVector(Vector3.Scale(bounds.size, transform.lossyScale))
                    size: Vector3.Scale(Vector3.Scale(bounds.size, viewScale), transform.lossyScale)
                );
            } else {
                return new Bounds(
                    center: Handles.inverseMatrix * (Vector3.Scale(bounds.center, viewScale)),
                    // size: Handles.inverseMatrix.MultiplyVector(bounds.size)
                    // size: bounds.size
                    size: Vector3.Scale(bounds.size, viewScale)
                );

            }
        }
        protected static Bounds TransformHandleToBoundsSpace(Bounds handleBounds, Transform transform, Vector3 viewScale, bool useRS) {
            viewScale = viewScale.InvertScale();
            if (useRS) {
                // Vector3 size = Handles.matrix.MultiplyVector(handleBounds.size);
                // size = Vector3.Scale(size, Vector3Ext.InvertScale(transform.lossyScale));
                Vector3 size = Vector3.Scale(handleBounds.size, Vector3Ext.InvertScale(transform.lossyScale));
                size = size.Abs();
                size = Vector3.Scale(size, viewScale);
                // Vector3 size2 = Handles.matrix.MultiplyVector(handleBounds.size).Scaled(transform.lossyScale.InvertScale()).Abs();
                return new Bounds(
                    center: Vector3.Scale(transform.localToWorldMatrix.inverse * (Handles.matrix * handleBounds.center), viewScale),
                    size: size
                // size: handleBounds.size.Scaled(transform.lossyScale.InvertScale()).Abs()
                );
            } else {
                return new Bounds(
                    center: Vector3.Scale(Handles.matrix * handleBounds.center, viewScale),
                    // size: Handles.matrix.MultiplyVector(handleBounds.size)
                    // size: handleBounds.size
                    size: Vector3.Scale(handleBounds.size, viewScale)
                );

            }
        }
    }
}