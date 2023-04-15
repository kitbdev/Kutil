using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    /// <summary>
    /// Holds an array of values for a 2d grid
    /// </summary>
    /// <typeparam name="TCellObject">type of data for each cell</typeparam>
    [System.Serializable]
    public class GridMap2D<TCellObject> : IEnumerable<TCellObject> {

        public Grid grid;
        [SerializeField] protected RectInt rect;
        [SerializeField] protected TCellObject[] cells;

        Func<GridMap2D<TCellObject>, Vector2Int, TCellObject> createFunc;
        Action<TCellObject, Vector2Int> destroyAction;

        /// <summary>
        /// called when any value is set. wont handle internal object modifications
        /// </summary>
        public Action onValueSetEvent;

        public RectInt Rect => rect;
        public int area => rect.size.x * rect.size.y;

        public GridMap2D(RectInt rect, Grid grid,
            Func<GridMap2D<TCellObject>, Vector2Int, TCellObject> createFunc = null,
            Action<TCellObject, Vector2Int> destroyAction = null) {
            this.rect = rect;
            // this.offset = rect.position;
            // this.size = rect.size;
            this.grid = grid;
            this.createFunc = createFunc;
            this.destroyAction = destroyAction;
            cells = new TCellObject[area];
            RecreateCells();
        }
        [ContextMenu("RecreateCells")]
        /// <summary>destroys and recreates all cells</summary>
        public void RecreateCells() {
            if (destroyAction != null) {
                ForEach((pos, ival) => {
                    if (ival is IEquatable<TCellObject> && !ival.Equals(default(TCellObject))) {
                        destroyAction(ival, pos);
                    }
                }, true);
            }
            if (cells == null) {
                cells = new TCellObject[area];
            }
            if (createFunc != null) {
                SetForEach((coord, ival) => createFunc(this, coord));
            }
        }
        /// <summary>Destroys all cells</summary>
        public void ClearAllCells() {
            if (destroyAction != null) {
                ForEach((pos, ival) => destroyAction(ival, pos), true);
            }
            // fill cells with nulls (or defaults if struct)
            SetForEach((pos, ival) => default);
        }
        /// <summary>
        /// Offsets the gridmap rect, moving all cells within
        /// </summary>
        /// <param name="offsetBy"></param>
        public void OffsetRect(Vector2Int offsetBy) {
            rect.position += offsetBy;
            // todo? different func to offest and not shift cells?
        }
        /// <summary>
        /// Move and resize the gridmap to match the new rect.
        /// cells will keep their same position, old cells are destroyed and new cells are created
        /// </summary>
        /// <param name="newRect"></param>
        public void Resize(RectInt newRect) {
            RectInt originalRect = rect;
            // Rect origRect = new Rect(offset, size);
            TCellObject[] originalCells = cells;
            rect = newRect;
            cells = new TCellObject[area];

            // create new cells where its bigger or use the existing one
            SetForEach((pos, ival) => {
                if (originalRect.Contains(pos)) {
                    int oldIndex = CoordToGridIndex(pos, originalRect);
                    return originalCells[oldIndex];
                } else if (createFunc != null) {
                    return createFunc.Invoke(this, pos);
                }
                return default;
            });
            // destroy old cells that are now oob
            if (destroyAction != null) {
                for (int i = 0; i < originalCells.Length; i++) {
                    TCellObject oldCell = originalCells[i];
                    // if not in the new cells map, destroy
                    if (!cells.Contains(oldCell)) {
                        // not using index to pos func cause we are in the old map
                        var oldpos = IndexToCoord(i, originalRect);
                        destroyAction.Invoke(oldCell, oldpos);
                    }
                }
            }
        }
        public GridMap2D<TCellObject> CopyConfig() {
            return new GridMap2D<TCellObject>(rect, grid, createFunc, destroyAction);
        }
        /// <summary>
        /// Returns a copy of the cells. use deep copy func if using references
        /// </summary>
        /// <returns></returns>
        public GridMap2D<TCellObject> Copy(Func<TCellObject, TCellObject> deepCopyFunc = null) {
            GridMap2D<TCellObject> copy = CopyConfig();
            copy.SetForEach((pos, ival) => {
                TCellObject cellObject = GetCellAtRaw(pos);
                if (deepCopyFunc != null) return deepCopyFunc.Invoke(cellObject);
                return cellObject;
            });
            return copy;
        }
        public void CopyCellsTo(GridMap2D<TCellObject> newGridCells, Vector2Int offset = default) {
            Rect obounds = new Rect(offset + rect.position, rect.size);
            newGridCells.SetForEach((pos, ival) => {
                if (obounds.Contains(pos)) {
                    Vector2Int opos = pos - offset;
                    return cells[CoordToGridIndex(opos)];
                }
                return ival;
            });
        }

        int CoordToGridIndex(Vector2Int coord) => CoordToGridIndex(coord, rect);
        public static int CoordToGridIndex(Vector2Int coord, RectInt rect) {
            // coord -= rect.position;
            // return coord.x + coord.y * rect.size.x;
            return coord.x - rect.x + (coord.y - rect.y) * rect.width;
        }
        Vector2Int IndexToCoord(int gridIndex) => IndexToCoord(gridIndex, rect);
        public static Vector2Int IndexToCoord(int gridIndex, RectInt rect) {
            // var pos = Vector2Int.zero;
            // pos.y = gridIndex / rect.width;
            // pos.x = gridIndex - pos.y * rect.width;
            // return pos + rect.position;
            return rect.position + new Vector2Int(gridIndex % rect.width, gridIndex / rect.width);
        }
        public Rect GetBounds() => rect.AsRect();
        public bool IsCoordInBounds(Vector2Int coord) => IsCoordInBounds(coord, rect);
        static bool IsCoordInBounds(Vector2Int coord, RectInt rect) {
            coord -= rect.position;
            return (coord.x >= 0 && coord.y >= 0 && coord.x < rect.size.x && coord.y < rect.size.y);
        }
        public bool AreAllCoordsInBounds(IEnumerable<Vector2Int> coords) {
            return coords.All(c => IsCoordInBounds(c));
        }


        /// <summary>Get the cell at the specified coordinate. no boundary check</summary>
        public TCellObject GetCellAtRaw(Vector2Int coord) {
            return cells[CoordToGridIndex(coord)];
        }
        /// <summary>Get the cell at the specified coordinate. checks bounds</summary>
        public TCellObject GetCellAt(Vector2Int coord) {
            if (!IsCoordInBounds(coord)) {
                // invalid position
                Debug.LogWarning($"Invalid coord {coord} size {rect}");
                return default;
            }
            return cells[CoordToGridIndex(coord)];
        }
        /// <summary>Get all of the cells in the gridmap</summary>
        public TCellObject[] GetAllCells() {
            return cells;
        }
        public (Vector2Int, TCellObject)[] GetAllCellsWithPos() {
            (Vector2Int, TCellObject)[] cellWithPos = new (Vector2Int, TCellObject)[area];
            for (int i = 0; i < area; i++) {
                cellWithPos[i] = (IndexToCoord(i), cells[i]);
            }
            return cellWithPos;
        }
        /// <summary>
        /// get the cells in each of the neighbor offset directions
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="neighborDirs"></param>
        /// <returns></returns>
        public IEnumerable<TCellObject> GetCellNeighbors(Vector2Int coord, IEnumerable<Vector2Int> neighborDirs) {
            return neighborDirs.Where(v => IsCoordInBounds(v + coord)).Select(v => GetCellAtRaw(v + coord));
        }
        public List<TCellObject> GetCellsInArea(RectInt coords) {
            // return cells.Where((c, i) => coords.Contains(IndexToCoord(i))).ToArray();
            List<TCellObject> cellObjects = new List<TCellObject>();
            foreach (var coord in coords.allPositionsWithin) {
                // ignores all out of bounds coords
                if (IsCoordInBounds(coord)) {
                    cellObjects.Add(GetCellAtRaw(coord));
                }
            }
            return cellObjects;
        }

        /// <summary>sets every cell in the gridmap to newvalue</summary>
        public void SetAllCells(TCellObject newValue) {
            for (int i = 0; i < area; i++) {
                cells[i] = newValue;
            }
            TriggerSetEvent();
        }
        public void SetCells(TCellObject[] newCells, RectInt area) {
            if (newCells.Length != area.size.x * area.size.y) {
                Debug.LogError($"Cannot SetCells, newcells length {newCells.Length} does not equal size of area {area.size.x * area.size.y}");
                return;
            }
            int i = 0;
            foreach (var coord in area.allPositionsWithin) {
                // ignores all out of bounds coords
                if (IsCoordInBounds(coord)) {
                    SetCellRaw(coord, newCells[i]);
                }
                i++;
            }
            TriggerSetEvent();
        }
        public void SetCells(Vector2Int offset, TCellObject[,] newCells) {
            int w = newCells.GetLength(0);//? switch these
            int h = newCells.GetLength(1);
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    Vector2Int coord = new Vector2Int(offset.x + x, offset.y + y);
                    cells[CoordToGridIndex(coord)] = newCells[x, y];
                }
            }
            TriggerSetEvent();
        }
        /// <summary>
        /// Set the cell at the coord to newValue. dsetroys old cell
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="newValue"></param>
        /// <returns>true coord is in bounds</returns>
        public bool SetCell(Vector2Int coord, TCellObject newValue) {
            if (!IsCoordInBounds(coord)) {
                // invalid position
                Debug.LogWarning($"Invalid position {coord}");
                return false;
            }
            int gridindex = CoordToGridIndex(coord);
            if (destroyAction != null) {
                TCellObject original = cells[gridindex];
                if (original != null) {
                    // Debug.Log($"clearing {original} {coord} nn{original != null} d{destroyAction}");
                    destroyAction.Invoke(original, coord);
                }
            }
            cells[gridindex] = newValue;
            TriggerSetEvent();
            return true;
        }
        public void SetCellRaw(Vector2Int coord, TCellObject newValue) {
            cells[CoordToGridIndex(coord)] = newValue;
            //? TriggerSetEvent();
        }

        public void SetForEach(System.Func<Vector2Int, TCellObject, TCellObject> setFunc) {
            for (int i = 0; i < area; i++) {
                cells[i] = setFunc.Invoke(IndexToCoord(i), cells[i]);
            }
            TriggerSetEvent();
        }
        /// <summary>
        /// iterate over each cell.
        /// return true to break out of loop
        /// </summary>
        /// <param name="action"></param>
        /// <param name="triggerSetEvent"></param>
        public void ForEach(System.Func<Vector2Int, TCellObject, bool> action, bool triggerSetEvent = true) {
            for (int i = 0; i < area; i++) {
                bool earlyOut = action.Invoke(IndexToCoord(i), cells[i]);
                if (earlyOut) {
                    break;
                }
            }
            if (triggerSetEvent) {
                TriggerSetEvent();
            }
        }
        /// <summary>
        /// iterate over each cell.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="triggerSetEvent"></param>
        public void ForEach(System.Action<Vector2Int, TCellObject> action, bool triggerSetEvent = false) {
            // ? try to allow early out
            for (int i = 0; i < area; i++) {
                action.Invoke(IndexToCoord(i), cells[i]);
            }
            if (triggerSetEvent) {
                TriggerSetEvent();
            }
        }
        /// <summary>invoke the OnValueSetEvent</summary>
        public void TriggerSetEvent() {
            onValueSetEvent?.Invoke();
        }

        public void DrawGizmosValues() {
            if (grid == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // values
            // todo only if in view
            for (int i = 0; i < area; i++) {
                string text = cells[i]?.ToString();
                Vector3 pos = grid.GetCellCenterWorld((Vector3Int)IndexToCoord(i));
                Handles.Label(pos, text);
            }
            // grid
            DrawGizmosGrid();
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosGrid() {
            if (grid == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // grid
            for (int y = rect.yMin; y <= rect.yMax; y++) {
                Handles.DrawLine(grid.CellToWorld(new Vector3Int(rect.xMin, y)), grid.CellToWorld(new Vector3Int(rect.xMax, y)));
            }
            for (int x = rect.xMin; x <= rect.xMax; x++) {
                Handles.DrawLine(grid.CellToWorld(new Vector3Int(x, rect.yMin)), grid.CellToWorld(new Vector3Int(x, rect.yMax)));
            }
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosBounds() {
            if (grid == null) return;
#if UNITY_EDITOR

            Vector3[] poses = new Vector3[]{
             grid.CellToWorld(new Vector3Int(rect.xMin, rect.yMin)),
             grid.CellToWorld(new Vector3Int(rect.xMin, rect.yMax)),
             grid.CellToWorld(new Vector3Int(rect.xMax, rect.yMin)),
             grid.CellToWorld(new Vector3Int(rect.xMax, rect.yMax)),
             };
            Handles.DrawLine(poses[0], poses[1]);
            Handles.DrawLine(poses[0], poses[2]);
            Handles.DrawLine(poses[2], poses[3]);
            Handles.DrawLine(poses[1], poses[3]);
#endif
        }


        // public void OnBeforeSerialize() {
        // }

        // public void OnAfterDeserialize() {
        //     // re set the map var
        // }

        public IEnumerable<(Vector2Int, TCellObject)> AllCells() {
            for (int i = 0; i < area; i++) {
                yield return (IndexToCoord(i), cells[i]);
            }
        }
        public IEnumerator<TCellObject> GetEnumerator() {
            return cells.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return cells.GetEnumerator();
        }

        public override string ToString() {
            return $"GridMap({rect}) of {typeof(TCellObject).Name}";
        }
    }
}