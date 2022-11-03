using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Kutil;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    /// <summary>
    /// Holds an array of values for a grid
    /// </summary>
    /// <typeparam name="TCellObject">type of data for each cell</typeparam>
    [System.Serializable]
    public class GridMap2D<TCellObject> : IEnumerable<TCellObject> {

        [SerializeField, ReadOnly] protected RectInt rect;
        public Grid grid;
        [SerializeField] protected TCellObject[] cells;

        [SerializeField] Func<GridMap2D<TCellObject>, Vector2Int, TCellObject> createFunc;
        [SerializeField] Action<TCellObject, Vector2Int> destroyAction;

        /// <summary>
        /// called when any value is set. wont handle internal object modifications
        /// </summary>
        public event Action OnValueSetEvent;

        public RectInt Rect => rect;
        protected int area => rect.size.x * rect.size.y;

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

        public void RecreateCells() {
            if (destroyAction != null) {
                ForEach((pos, ival) => {
                    if (ival is IEquatable<TCellObject> && !ival.Equals(default(TCellObject))) {
                        destroyAction(ival, pos);
                    }
                }, true);
            }
            if (createFunc != null) {
                SetForEach((coord, ival) => createFunc(this, coord));
            }
        }
        public void ClearAllCells() {
            if (destroyAction != null) {
                ForEach((pos, ival) => destroyAction(ival, pos), true);
            }
            // fill cells with nulls (or defaults if struct)
            SetForEach((pos, ival) => default);
        }

        public void OffsetRect(Vector2Int offsetBy) {
            rect.position += offsetBy;
            // todo shift cells?
        }
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
        static int CoordToGridIndex(Vector2Int coord, RectInt rect) {
            coord -= rect.position;
            return coord.x + coord.y * rect.size.x;
        }
        Vector2Int IndexToCoord(int gridIndex) => IndexToCoord(gridIndex, rect);
        static Vector2Int IndexToCoord(int gridIndex, RectInt rect) {
            var pos = Vector2Int.zero;
            pos.y = gridIndex / rect.x;
            pos.x = gridIndex - pos.y;
            return pos + rect.position;
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


        /// <summary>
        /// No bounds check
        /// </summary>
        public TCellObject GetCellAtRaw(Vector2Int coord) {
            return cells[CoordToGridIndex(coord)];
        }
        public TCellObject GetCellAt(Vector2Int coord) {
            if (!IsCoordInBounds(coord)) {
                // invalid position
                Debug.LogWarning($"Invalid coord {coord} size {rect}");
                return default;
            }
            return cells[CoordToGridIndex(coord)];
        }
        public TCellObject[] GetAllCells() {
            return cells;
        }
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


        public void SetAllCells(TCellObject newValue) {
            for (int i = 0; i < area; i++) {
                cells[i] = newValue;
            }
            OnValueSetEvent?.Invoke();
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
            OnValueSetEvent?.Invoke();
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
            OnValueSetEvent?.Invoke();
        }

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
            OnValueSetEvent?.Invoke();
            return true;
        }
        public void SetCellRaw(Vector2Int coord, TCellObject newValue) {
            cells[CoordToGridIndex(coord)] = newValue;
            //? OnValueSetEvent?.Invoke();
        }

        public void SetForEach(System.Func<Vector2Int, TCellObject, TCellObject> setFunc) {
            for (int i = 0; i < area; i++) {
                cells[i] = setFunc.Invoke(IndexToCoord(i), cells[i]);
            }
            OnValueSetEvent?.Invoke();
        }
        public void ForEach(System.Action<Vector2Int, TCellObject> action, bool triggerSetEvent = true) {
            // ? try to allow early out
            for (int i = 0; i < area; i++) {
                action.Invoke(IndexToCoord(i), cells[i]);
            }
            if (triggerSetEvent) {
                OnValueSetEvent?.Invoke();
            }
        }
        public void TriggerSetEvent() {
            OnValueSetEvent?.Invoke();
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

        public IEnumerator<TCellObject> GetEnumerator() {
            return cells.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return cells.GetEnumerator();
        }

        public override string ToString() {
            return $"GridCells({rect}) of {typeof(TCellObject).Name}";
        }
    }
}