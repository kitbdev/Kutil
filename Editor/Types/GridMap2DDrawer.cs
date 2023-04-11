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

        VisualElement root;
        VisualElement cellsContainer;
        PropertyField cellField;
        HelpBox invalidSizeWarning;
        PropertyField[] cellsPropertyFields;
        RectInt gridmapRect;

        SerializedProperty gridProp;
        SerializedProperty rectProp;
        SerializedProperty cellsProp;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            gridProp = property.FindPropertyRelative(nameof(GridMap2D<int>.grid));
            rectProp = property.FindPropertyRelative("rect");
            cellsProp = property.FindPropertyRelative("cells");

            root = new VisualElement();
            var gridField = new PropertyField(gridProp);
            gridField.name = "PropertyField:GridMap2d-Grid";
            var rectField = new PropertyField(rectProp);
            rectField.name = "PropertyField:GridMap2d-Rect";
            root.Add(gridField);
            root.Add(rectField);
            rectField.RegisterValueChangeCallback(OnValueChanged);

            gridmapRect = rectProp.rectIntValue;


            // tmp?
            cellField = new PropertyField(cellsProp);
            root.Add(cellField);
            // cellField.RegisterValueChangeCallback(OnValueChanged);
            cellField.RegisterCallback<SerializedPropertyChangeEvent>(OnValueChanged);

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

            cellsContainer = new VisualElement();
            cellsContainer.AddToClassList(cellsContainerClass);
            cellsContainer.name = "GridMap2D CellContainer";
            cellsContainer.style.flexWrap = Wrap.Wrap;
            cellsContainer.style.flexDirection = FlexDirection.Row;


            cellsContainer.RegisterCallback<AttachToPanelEvent>(OnSetup);
            cellsContainer.RegisterCallback<GeometryChangedEvent>(OnGeoChanged);
            // root.Add(cellsContainer);
            cellsFoldOut.Add(cellsContainer);
            // IEnumerable<SerializedProperty> cellProps = cellsProp.GetAllChildren(true);
            // SerializedProperty cellProp = cellsProp.NextVisible(true);
            // Debug.Log(cellsProp.propertyPath + " cps " + cellProps.ToStringFull(p => p.propertyPath, true));
            // int i = 0;
            invalidSizeWarning = new HelpBox("array size does not match rect!", HelpBoxMessageType.Warning);
            invalidSizeWarning.SetDisplay(false);
            cellsContainer.Add(invalidSizeWarning);


            if (gridmapRect.Area() != cellsProp.arraySize) {
                invalidSizeWarning.SetDisplay(true);
                Debug.LogWarning($"gridmap array size doesnt match rect! r:{gridmapRect.Area()} a:{cellsProp.arraySize}");
            } else {
                FillCellsContainer();
            }

            return root;
        }

        void FillCellsContainer() {
            int maxColumns = 5;
            int maxRows = 10;
            //? only valid array element types?

            bool showAs2D = gridmapRect.width <= maxColumns && gridmapRect.height <= maxRows;

            if (gridmapRect.Area() != cellsProp.arraySize) {
                // todo?
                // RecreateGrid(property);
            }
            if (!showAs2D) {
                cellsContainer.SetDisplay(false);
                return;
            }
            cellsContainer.SetDisplay(true);

            if (gridmapRect.Area() != cellsProp.arraySize) {
                invalidSizeWarning.SetDisplay(true);
                return;
            }
            // Debug.Log($"filling {cellsProp.arraySize} cells!");
            invalidSizeWarning.SetDisplay(false);
            cellsPropertyFields = new PropertyField[cellsProp.arraySize];
            for (int i = 0; i < cellsProp.arraySize; i++) {
                // for (int y = 0; y < gridmapRect.width; y++) {
                // for (int x = 0; x < gridmapRect.height; x++) {
                // Vector2Int gridPos = new Vector2Int(gridmapRect.x + x, gridmapRect.y + y);
                Vector2Int gridPos = GridMap2D<int>.IndexToCoord(i, gridmapRect);
                SerializedProperty eProp = cellsProp.GetArrayElementAtIndex(i);
                var epf = new PropertyField(eProp, "");
                epf.AddToClassList(cellsElementClassName);
                cellsPropertyFields[i] = epf;
                epf.name = $"PropertyField cell {i} ";
                epf.style.maxWidth = 50;
                cellsContainer.Add(epf);
                // i++;
                // }
                epf.BindProperty(eProp);
            }
            // todo cells get added, but arent shown
            cellsContainer.SetDisplay(true);
            root.MarkDirtyRepaint();
            // cellsContainer.style.height = 100;
            UpdateGridSize();
        }

        void ClearCellsContainer() {
            if (cellsPropertyFields == null) return;
            for (int i = cellsPropertyFields.Length - 1; i >= 0; i--) {
                cellsContainer.Remove(cellsPropertyFields[i]);
            }
            cellsPropertyFields = null;
        }

        void OnValueChanged(SerializedPropertyChangeEvent changeEvent) {
            gridmapRect = rectProp.rectIntValue;
            // Debug.Log($"updating size {gridmapRect} {cellsProp.arraySize} cpfs:{cellsPropertyFields?.Length + "" ?? "null"}");
            // only if size changes
            if (cellsPropertyFields == null
                || cellsPropertyFields.Length != cellsProp.arraySize
                || gridmapRect.Area() != cellsPropertyFields.Length) {
                ClearCellsContainer();
                FillCellsContainer();
            }
        }
        void OnSetup(AttachToPanelEvent e) {
            // FixCells();
        }
        private void OnGeoChanged(GeometryChangedEvent evt) {
            UpdateGridSize();
        }

        private void UpdateGridSize() {
            // update grid layout
            if (cellsPropertyFields == null) return;
            int columns = gridmapRect.width;
            if (columns == 0) return;
            float per = 100f / columns;
            float perH = gridmapRect.height == 0 ? 0 : 100f / gridmapRect.height;
            StyleLength width = new StyleLength(new Length(per, LengthUnit.Percent));
            // StyleLength height = new StyleLength(new Length(perH, LengthUnit.Percent));
            for (int i = 0; i < cellsPropertyFields.Length; i++) {
                cellsPropertyFields[i].style.maxWidth = width;
                cellsPropertyFields[i].style.width = width;
                // cellsPropertyFields[i].style.height = height;
            }
        }

        void FixCells() {
            // Debug.Log("fixing cells");
            // for (int i = 0; i < cellsPropertyFields.Length; i++) {
            //     cellsPropertyFields[i].label = "";
            //     Label label = cellsPropertyFields[i].Q<Label>(null, PropertyField.labelUssClassName);
            //     if (label!=null){
            //         label.text = "";
            //     }
            // }
        }



        void RecreateGrid(SerializedProperty property) {
            // cannot cast like that...
            // GridMap2D<dynamic> gridMap2D = property.GetValue<GridMap2D<dynamic>>();
            // Debug.Log("got gmap " + gridMap2D.ToString());
            //? invoke method w/ reflection?
            string methodName = nameof(GridMap2D<int>.RecreateCells);
            UnityEngine.Object targetUObject = property.serializedObject.targetObject;
            // target uboject should be whatever monobehavior holds it...
            object targetObject = targetUObject;
            if (targetObject == null) return;

            Debug.Log($"Recreating grid on {targetUObject.name} {methodName}");
            if (ReflectionHelper.TryGetMemberInfo(ref targetObject, methodName, ReflectionHelper.defFlags, out var memberInfo)) {
                if (memberInfo is System.Reflection.MethodInfo mi) {
                    Undo.RecordObject(targetUObject, $"Call {methodName} method on {property.name}");
                    mi.Invoke(targetObject, new object[0]);

                    EditorUtility.SetDirty(targetUObject);
                    property.serializedObject.ApplyModifiedProperties();
                    return;
                }
            }

            Debug.LogError($"failed to find method {methodName} on {targetUObject.name}");
        }

    }
}