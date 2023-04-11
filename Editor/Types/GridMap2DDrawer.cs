using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;

namespace Kutil.Editor.PropertyDrawers {
    [CustomPropertyDrawer(typeof(GridMap2D<>))]
    public class GridMap2DDrawer : PropertyDrawer {
        public static readonly string cellsContainerClass = "kutil-gridmap2d-cells-container";
        public static readonly string cellsElementClassName = "kutil-gridmap2d-cells-element";
        public static readonly string gridmapClass = "kutil-gridmap-2d";


        class GridMap2DData {
            public VisualElement root;
            public VisualElement cellsContainer;
            public PropertyField cellField;
            public HelpBox invalidSizeWarning;
            public PropertyField[] cellsPropertyFields;
            public RectInt gridmapRect;
            public SerializedProperty property;
            public SerializedProperty gridProp;
            public SerializedProperty rectProp;
            public SerializedProperty cellsProp;
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            GridMap2DData args = new();
            SerializedProperty gridProp = property.FindPropertyRelative(nameof(GridMap2D<int>.grid));
            SerializedProperty rectProp = property.FindPropertyRelative("rect");
            SerializedProperty cellsProp = property.FindPropertyRelative("cells");
            args.property = property;
            args.gridProp = gridProp;
            args.rectProp = rectProp;
            args.cellsProp = cellsProp;


            VisualElement root = new VisualElement();
            args.root = root;
            var gridField = new PropertyField(gridProp);
            gridField.name = "PropertyField:GridMap2d-Grid";
            var rectField = new PropertyField(rectProp);
            rectField.name = "PropertyField:GridMap2d-Rect";
            root.Add(gridField);
            root.Add(rectField);
            rectField.RegisterCallback<SerializedPropertyChangeEvent, GridMap2DData>(OnValueChanged, args);

            args.gridmapRect = rectProp.rectIntValue;


            // tmp?
            args.cellField = new PropertyField(cellsProp);
            root.Add(args.cellField);
            args.cellField.RegisterCallback<SerializedPropertyChangeEvent, GridMap2DData>(OnValueChanged, args);
            // cellField.RegisterValueChangeCallback(OnValueChanged);
            // cellField.RegisterCallback<SerializedPropertyChangeEvent>(OnValueChanged);

            // show 2d array editor
            var cellsFoldOut = new Foldout();
            cellsFoldOut.text = "Cells Grid";
            cellsFoldOut.value = true;
            cellsFoldOut.viewDataKey = $"GridMap2d foldout {property.propertyPath}";
            // remove indent
            VisualElement foldoutContainer = cellsFoldOut.Q(null, Foldout.contentUssClassName);
            // if (foldoutContainer.ValidateExists($"{GetType().Name} ")) return root;
            if (foldoutContainer == null) {
                Debug.LogError($"Gridmap2d {property.propertyPath} invalid Foldout no foldoutContainer!");
                return root;
            }
            foldoutContainer.style.marginLeft = new StyleLength(0f);
            root.Add(cellsFoldOut);

            var cellsContainer = new VisualElement();
            args.cellsContainer = cellsContainer;
            cellsContainer.AddToClassList(cellsContainerClass);
            cellsContainer.name = "GridMap2D CellContainer";
            cellsContainer.style.flexWrap = Wrap.Wrap;
            cellsContainer.style.flexDirection = FlexDirection.Row;


            cellsContainer.RegisterCallback<AttachToPanelEvent, GridMap2DData>(OnSetup, args);
            cellsContainer.RegisterCallback<GeometryChangedEvent, GridMap2DData>(OnGeoChanged, args);
            // root.Add(cellsContainer);
            cellsFoldOut.Add(cellsContainer);
            // IEnumerable<SerializedProperty> cellProps = cellsProp.GetAllChildren(true);
            // SerializedProperty cellProp = cellsProp.NextVisible(true);
            // Debug.Log(cellsProp.propertyPath + " cps " + cellProps.ToStringFull(p => p.propertyPath, true));
            // int i = 0;
            args.invalidSizeWarning = new HelpBox("array size does not match rect!", HelpBoxMessageType.Warning);
            args.invalidSizeWarning.SetDisplay(false);
            cellsContainer.Add(args.invalidSizeWarning);


            if (args.gridmapRect.Area() != cellsProp.arraySize) {
                args.invalidSizeWarning.SetDisplay(true);
                Debug.LogWarning($"gridmap array size doesnt match rect! r:{args.gridmapRect.Area()} a:{cellsProp.arraySize}");
            } else {
                FillCellsContainer(args);
            }

            return root;
        }

        void FillCellsContainer(GridMap2DData args) {
            int maxColumns = 5;
            int maxRows = 10;
            //? only valid array element types?

            bool showAs2D = args.gridmapRect.width <= maxColumns && args.gridmapRect.height <= maxRows;

            if (args.gridmapRect.Area() != args.cellsProp.arraySize) {
                // todo?
                // RecreateGrid(property);
            }
            if (!showAs2D) {
                args.cellsContainer.SetDisplay(false);
                return;
            }
            args.cellsContainer.SetDisplay(true);

            if (args.gridmapRect.Area() != args.cellsProp.arraySize) {
                args.invalidSizeWarning.SetDisplay(true);
                return;
            }
            // Debug.Log($"filling {cellsProp.arraySize} cells!");
            args.invalidSizeWarning.SetDisplay(false);
            args.cellsPropertyFields = new PropertyField[args.cellsProp.arraySize];
            for (int i = 0; i < args.cellsProp.arraySize; i++) {
                Vector2Int gridPos = GridMap2D<int>.IndexToCoord(i, args.gridmapRect);
                SerializedProperty eProp = args.cellsProp.GetArrayElementAtIndex(i);
                // todo instead of propertyfields directly, have square cells that show the prop field on click
                var epf = new PropertyField(eProp, "");
                epf.AddToClassList(cellsElementClassName);
                args.cellsPropertyFields[i] = epf;
                epf.name = $"PropertyField cell {i} ";
                epf.style.maxWidth = 50;
                args.cellsContainer.Add(epf);
                epf.BindProperty(eProp);
            }
            // args.cellsContainer.SetDisplay(true);
            // args.root.MarkDirtyRepaint();
            // cellsContainer.style.height = 100;
            UpdateGridSize(args);
        }

        void ClearCellsContainer(GridMap2DData args) {
            if (args.cellsPropertyFields == null) return;
            // for (int i = cellsPropertyFields.Length - 1; i >= 0; i--) {
            //     cellsContainer.Remove(cellsPropertyFields[i]);
            // }
            args.cellsContainer.Clear();
            args.cellsPropertyFields = null;
        }

        void OnValueChanged(SerializedPropertyChangeEvent changeEvent, GridMap2DData args) {
            args.gridmapRect = args.rectProp.rectIntValue;
            // Debug.Log($"updating size {gridmapRect} {cellsProp.arraySize} cpfs:{cellsPropertyFields?.Length + "" ?? "null"}");
            // only if size changes
            if (args.cellsPropertyFields == null
                || args.cellsPropertyFields.Length != args.cellsProp.arraySize
                || args.gridmapRect.Area() != args.cellsPropertyFields.Length) {
                ClearCellsContainer(args);
                FillCellsContainer(args);
            }
        }
        void OnSetup(AttachToPanelEvent e, GridMap2DData args) {
            // FixCells(args);
        }
        private void OnGeoChanged(GeometryChangedEvent evt, GridMap2DData args) {
            UpdateGridSize(args);
            FixCells(args);

        }

        private void UpdateGridSize(GridMap2DData args) {
            // update grid layout
            if (args.cellsPropertyFields == null) return;
            int columns = args.gridmapRect.width;
            if (columns == 0) return;
            float per = 100f / columns;
            float perH = args.gridmapRect.height == 0 ? 0 : 100f / args.gridmapRect.height;
            StyleLength width = new StyleLength(new Length(per, LengthUnit.Percent));
            // StyleLength height = new StyleLength(new Length(perH, LengthUnit.Percent));
            for (int i = 0; i < args.cellsPropertyFields.Length; i++) {
                args.cellsPropertyFields[i].style.maxWidth = width;
                args.cellsPropertyFields[i].style.width = width;
                // cellsPropertyFields[i].style.height = height;
            }
        }

        void FixCells(GridMap2DData args) {
            // Debug.Log("fixing cells "+args.cellsPropertyFields.Length);
            for (int i = 0; i < args.cellsPropertyFields.Length; i++) {
                args.cellsPropertyFields[i].label = "";
                Label label = args.cellsPropertyFields[i].Q<Label>(null, PropertyField.labelUssClassName);
                if (label == null) {
                    continue;
                }
                PropertyField pf = label.GetFirstAncestorOfType<PropertyField>();
                if (pf != null && pf == args.cellsPropertyFields[i]) {
                    label.text = "";
                    label.RemoveFromHierarchy();
                }
            }
        }



        void RecreateGrid(GridMap2DData args) {
            // cannot cast like that...
            // GridMap2D<dynamic> gridMap2D = property.GetValue<GridMap2D<dynamic>>();
            // Debug.Log("got gmap " + gridMap2D.ToString());
            //? invoke method w/ reflection?
            string methodName = nameof(GridMap2D<int>.RecreateCells);
            UnityEngine.Object targetUObject = args.property.serializedObject.targetObject;
            // target uboject should be whatever monobehavior holds it...
            object targetObject = targetUObject;
            if (targetObject == null) return;

            Debug.Log($"Recreating grid on {targetUObject.name} {methodName}");
            if (ReflectionHelper.TryGetMemberInfo(ref targetObject, methodName, ReflectionHelper.defFlags, out var memberInfo)) {
                if (memberInfo is System.Reflection.MethodInfo mi) {
                    Undo.RecordObject(targetUObject, $"Call {methodName} method on {args.property.name}");
                    mi.Invoke(targetObject, new object[0]);

                    EditorUtility.SetDirty(targetUObject);
                    args.property.serializedObject.ApplyModifiedProperties();
                    return;
                }
            }

            Debug.LogError($"failed to find method {methodName} on {targetUObject.name}");
        }

    }
}