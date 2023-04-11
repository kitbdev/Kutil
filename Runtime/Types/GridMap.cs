using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    public static class GridMap {

        public static Vector3Int[] v3dir6 = new Vector3Int[6]{
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.forward,
            Vector3Int.back,
        };
        /// <summary>
        /// FloodFill algorithm
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="checkBreakFunc">runs on every coord. return true to end flood</param>
        /// <param name="neighborFunc">check pos -> neighbor positions to add. Must be restrictive!</param>
        /// <param name="sortComparer">null or use to sort the frontier after adding neighbors</param>
        public static void FloodFor(Vector3Int startPos,
        Func<Vector3Int, bool> checkBreakFunc,
        Func<Vector3Int, IEnumerable<Vector3Int>> neighborFunc,
        Action failAction = null,
        IComparer<Vector3Int> sortComparer = null
        // bool debug = false
        // out List<Vector3Int> allCheckedPos = null
        ) {
            List<Vector3Int> frontier = new();
            List<Vector3Int> checkedPos = new();

            frontier.Add(startPos);
            bool failed = true;
            while (frontier.Count > 0) {
                var checking = frontier[0];
                frontier.RemoveAt(0);
                checkedPos.Add(checking);
                if (checkBreakFunc.Invoke(checking)) {
                    failed = false;
                    break;
                }
                frontier.AddRange(neighborFunc.Invoke(checking));
                // todo dont use to list? its slow I think
                frontier = frontier.Except(checkedPos).ToList();
                if (sortComparer != null) {
                    frontier.Sort(sortComparer);
                }
            }
            // allCheckedPos = checkedPos;
            if (failed) {
                failAction?.Invoke();
            }
        }
    }
    // public interface IGridMap<TCellObject> {
    //     Grid grid { get; set; }
    //     BoundsInt Bounds { get; }
    //     TCellObject GetCellAt(Vector3Int coord);
    //     TCellObject[] GetAllCells();
    //     // void SetCell(Vector3Int coord, TCellObject newValue);
    // }
    /// <summary>
    /// Holds an array of values for a 3d grid
    /// </summary>
    /// <typeparam name="TCellObject">type of data for each cell</typeparam>
    [System.Serializable]
    public class GridMap<TCellObject> {
        // todo? use jagged array (faster, no mul needed to convert to index)

        [SerializeField] public Grid grid;
        [SerializeField] protected BoundsInt bounds = new BoundsInt();

        // [SerializeReference]
        [SerializeField]
        protected TCellObject[] cells;

        [SerializeField]
        public Func<GridMap<TCellObject>, Vector3Int, TCellObject> createFunc = null;
        [SerializeField]
        public Action<TCellObject, Vector3Int> destroyAction = null;

        /// <summary>
        /// called when any value is set. wont handle internal object modifications
        /// </summary>
        public System.Action onValueSetEvent;

        protected int Volume => bounds.size.x * bounds.size.z * bounds.size.y;

        /// <summary>exclusive bounds containing the cells</summary>
        public BoundsInt Bounds => bounds;


        public GridMap(BoundsInt bounds, Grid grid,
            Func<GridMap<TCellObject>, Vector3Int, TCellObject> createFunc = null,
            Action<TCellObject, Vector3Int> destoryAction = null) {
            this.bounds = bounds;
            this.grid = grid;

            cells = new TCellObject[Volume];
            this.createFunc = createFunc;
            this.destroyAction = destoryAction;
            RecreateMap();
        }

        public void RecreateMap() {
            // Debug.Log("GridMap create " + cells + " " + Volume + " " + bounds);
            if (destroyAction != null && cells != null) {
                ForEach((coord, ival) => {
                    if (ival is IEquatable<TCellObject> && !ival.Equals(default(TCellObject))) {
                        destroyAction(ival, coord);
                    }
                }, true);
            }
            // if (cells == null) {
            cells = new TCellObject[Volume];
            // }
            if (createFunc != null) {
                SetForEach((coord, ival) => createFunc.Invoke(this, coord));
                // else dont set, it can be done externally
            }
        }
        public void ClearAllCells() {
            if (destroyAction != null) {
                ForEach((coord, ival) => destroyAction(ival, coord), true);
            }
            // fill cells with nulls (or defaults if struct)
            SetForEach((coord, ival) => default);
        }
        /// <summary>
        /// Offsets the gridmap bounds, moving all cells within
        /// </summary>
        /// <param name="offsetBy"></param>
        public void OffsetBounds(Vector3Int offsetBy) {
            bounds.position += offsetBy;
        }
        /// <summary>
        /// Move and resize the gridmap to match the new rect.
        /// cells will keep their same position, old cells are destroyed and new cells are created
        /// </summary>
        /// <param name="newRect"></param>
        void ResizeMap(BoundsInt newBounds) {
            BoundsInt originalBounds = bounds;
            TCellObject[] originalCells = cells;
            bounds = newBounds;
            cells = new TCellObject[Volume];

            // create new cells where its bigger or use the existing one
            SetForEach((pos, ival) => {
                if (originalBounds.Contains(pos)) {
                    int oldIndex = CoordToGridIndex(pos, originalBounds);
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
                        var oldpos = IndexToCoord(i, originalBounds);
                        destroyAction.Invoke(oldCell, oldpos);
                    }
                }
            }
        }
        public GridMap<TCellObject> CopyConfig() {
            return new GridMap<TCellObject>(bounds, grid, createFunc, destroyAction);
        }
        /// <summary>
        /// Returns a copy of the cells. use deep copy func if using references
        /// </summary>
        /// <returns></returns>
        public GridMap<TCellObject> Copy(Func<TCellObject, TCellObject> deepCopyFunc = null) {
            GridMap<TCellObject> copy = CopyConfig();
            copy.SetForEach((pos, ival) => {
                TCellObject cellObject = GetCellAtRaw(pos);
                if (deepCopyFunc != null) return deepCopyFunc.Invoke(cellObject);
                return cellObject;
            });
            return copy;
        }
        public void CopyCellsTo(GridMap<TCellObject> newGridCells, Vector3Int offset = default) {
            Bounds obounds = new Bounds(offset + bounds.position, bounds.size);
            newGridCells.SetForEach((pos, ival) => {
                if (obounds.Contains(pos)) {
                    Vector3Int opos = pos - offset;
                    return cells[CoordToGridIndex(opos)];
                }
                return ival;
            });
        }

        public bool IsCoordInBounds(Vector3Int coord) => IsCoordInBounds(coord, bounds);
        static bool IsCoordInBounds(Vector3Int coord, BoundsInt bounds) {
            coord -= bounds.position;
            return (coord.x >= 0 && coord.x < bounds.size.x && coord.y >= 0 && coord.y < bounds.size.y && coord.z >= 0 && coord.z < bounds.size.z);
        }

        int CoordToGridIndex(Vector3Int coord) => CoordToGridIndex(coord, bounds);
        static int CoordToGridIndex(Vector3Int coord, BoundsInt bounds) {
            // assumes in bounds
            // coord -= bounds.position;
            // return coord.x + coord.z * bounds.size.x + coord.y * bounds.size.x * bounds.size.z;
            return coord.x - bounds.x + (coord.z - bounds.z) * bounds.size.x + (coord.y - bounds.y) * bounds.size.x * bounds.size.z;
        }
        Vector3Int IndexToCoord(int gridIndex) => IndexToCoord(gridIndex, bounds);
        static Vector3Int IndexToCoord(int gridIndex, BoundsInt bounds) {
            var coord = Vector3Int.zero;
            coord.y = gridIndex / (bounds.size.x * bounds.size.z);
            gridIndex -= (coord.y * bounds.size.x * bounds.size.z);
            coord.z = gridIndex / bounds.size.x;
            coord.x = gridIndex % bounds.size.x;
            return coord + bounds.position;
        }



        /// <summary>Get the cell at the specified coordinate. no boundary check</summary>
        public TCellObject GetCellAtRaw(Vector3Int coord) {
            return cells[CoordToGridIndex(coord)];
        }
        TCellObject GetCellAtRaw(int index) {
            return cells[index];
        }
        /// <summary>Get the cell at the specified coordinate. checks bounds</summary>
        public TCellObject GetCellAt(Vector3Int coord) {
            if (!IsCoordInBounds(coord)) {
                // invalid position
                Debug.LogWarning($"Invalid coord {coord} size {bounds}");
                return default;
            }
            return cells[CoordToGridIndex(coord)];
        }
        /// <summary>Get all of the cells in the gridmap</summary>
        public TCellObject[] GetAllCells() {
            return cells;
        }
        /// <summary>
        /// get the cells in each of the neighbor offset directions
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="neighborDirs"></param>
        /// <returns></returns>
        public IEnumerable<TCellObject> GetCellNeighbors(Vector3Int coord, IEnumerable<Vector3Int> neighborDirs) {
            return neighborDirs.Where(v => IsCoordInBounds(v + coord)).Select(v => GetCellAtRaw(v + coord));
        }
        /// <summary>
        /// Gets all cells wihtin the boundsint area.
        /// guaruntees size and order
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public TCellObject[] GetCellsInAreaEnc(BoundsInt area) {
            TCellObject[] cellObjects = new TCellObject[area.Volume()];
            // if (!bounds.ContainsBounds(area)) {
            //     Debug.LogError("Area not in bounds " + area + " " + bounds);
            //     return null;
            // }
            for (int y = 0; y < area.size.y; y++) {
                for (int z = 0; z < area.size.z; z++) {
                    for (int x = 0; x < area.size.x; x++) {
                        Vector3Int gmCoord = new Vector3Int(x, y, z) + area.position;
                        if (IsCoordInBounds(gmCoord)) {
                            cellObjects[CoordToGridIndex(gmCoord, area)] = GetCellAtRaw(gmCoord);
                            // otherwise leave default
                        }
                    }
                }
            }
            return cellObjects;
        }
        public List<TCellObject> GetCellsInArea(BoundsInt area) {
            // return cells.Where((c, i) => coords.Contains(IndexToCoord(i))).ToArray();
            List<TCellObject> cellObjects = new List<TCellObject>(area.Volume());
            foreach (var coord in area.allPositionsWithin) {
                // ignores all out of bounds coords
                if (IsCoordInBounds(coord)) {
                    cellObjects.Add(GetCellAtRaw(coord));
                }
            }
            return cellObjects;
        }
        public List<Vector3Int> GetCellCoordsWhere(System.Func<Vector3Int, TCellObject, bool> condition) {
            List<Vector3Int> conditionCells = new();
            for (int i = 0; i < Volume; i++) {
                Vector3Int coord = IndexToCoord(i);
                if (condition.Invoke(coord, cells[i])) {
                    conditionCells.Add(coord);
                }
            }
            return conditionCells;
        }


        public void SetAllCellsRaw(TCellObject[] newValues, BoundsInt newBounds) {
            bounds = newBounds;
            cells = newValues;
        }
        /// <summary>sets every cell in the gridmap to newvalue</summary>
        public void SetAllCells(TCellObject newValue) {
            for (int i = 0; i < Volume; i++) {
                cells[i] = newValue;
            }
            TriggerSetEvent();
        }
        public void SetCells(TCellObject[] newCells, BoundsInt area) {
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
        /// <summary>
        /// Set the cell at the coord to newValue. dsetroys old cell
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="newValue"></param>
        /// <returns>true coord is in bounds</returns>
        public bool SetCell(Vector3Int coord, TCellObject newValue) {
            if (!IsCoordInBounds(coord)) {
                // invalid position
                Debug.LogWarning($"Invalid position {coord}");
                return false;
            }
            int gridindex = CoordToGridIndex(coord);
            if (destroyAction != null) {
                TCellObject original = cells[gridindex];
                if (original != null) {
                    // Debug.Log($"clearing {original} {coord} nn{original != null} d{destoryAction}");
                    destroyAction.Invoke(original, coord);
                }
            }
            cells[gridindex] = newValue;
            TriggerSetEvent();
            return true;
        }
        public void SetCellRaw(Vector3Int coord, TCellObject newValue) {
            cells[CoordToGridIndex(coord)] = newValue;
            //? TriggerSetEvent();
        }

        public void SetForEach(System.Func<Vector3Int, TCellObject, TCellObject> setFunc) {
            for (int i = 0; i < Volume; i++) {
                cells[i] = setFunc.Invoke(IndexToCoord(i), cells[i]);
            }
            TriggerSetEvent();
        }
        public void ForEach(System.Action<Vector3Int, TCellObject> action, bool triggerSetEvent = true) {
            // ? try to allow early out
            for (int i = 0; i < Volume; i++) {
                action.Invoke(IndexToCoord(i), cells[i]);
            }
            if (triggerSetEvent) {
                TriggerSetEvent();
            }
        }
        public void ForEach(IEnumerable<Vector3Int> coords, System.Action<TCellObject> action, bool triggerUpdate = true) {
            foreach (var coord in coords) {
                if (!IsCoordInBounds(coord)) {
                    Debug.LogWarning($"Invalid coord {coord} in map foreach!");
                    continue;
                }
                action.Invoke(cells[CoordToGridIndex(coord)]);
            }
            if (triggerUpdate) {
                TriggerSetEvent();
            }
        }

        public void TriggerSetEvent() {
            onValueSetEvent?.Invoke();
        }


        /// <summary>
        /// Searches neighbor cells in 3 dimensions within bounds until a condition is reached
        /// </summary>
        /// <param name="startPos">position to start the search</param>
        /// <param name="finishSearchingFunc">runs on every cell checked. return true to stop searching</param>
        /// <param name="validNeighborFunc">should this neighbor be checked?</param>
        /// <param name="failAction">what to do if our target was never found</param>
        /// <param name="sortNeighborComparer">use this to sort all neighbors before each check</param>
        public void SearchNeighbors(Vector3Int startPos,
               Func<Vector3Int, bool> finishSearchingFunc,
               Func<Vector3Int, TCellObject, bool> validNeighborFunc = null,
               Action failAction = null,
               IComparer<Vector3Int> sortNeighborComparer = null) {
            GridMap.FloodFor(startPos,
                             finishSearchingFunc,
                             p => GridMap.v3dir6.Select(d => d + p).Where(p => IsCoordInBounds(p)).Where(p => validNeighborFunc?.Invoke(p, GetCellAtRaw(p)) ?? true),
                             failAction,
                             sortNeighborComparer);
        }


        public void DrawGizmosValues() {
            if (cells == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // values
            // todo only if in view
            for (int y = 0; y < bounds.size.y; y++) {
                for (int z = 0; z < bounds.size.z; z++) {
                    for (int x = 0; x < bounds.size.x; x++) {
                        Vector3Int cellPos = new Vector3Int(x, y, z);
                        string text = cells[CoordToGridIndex(cellPos)]?.ToString();
                        Vector3 coord = grid.CellToWorld(cellPos);
                        Handles.Label(coord, text);
                    }
                }
            }
            // grid
            DrawGizmosGrid();
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosGrid() {
            if (cells == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // grid
            // for (int y = 0; y <= bounds.size.x; y++) {
            //     Handles.DrawLine(GetWorldCoord(0, y), GetWorldCoord(bounds.size.x, y,0));
            // }
            // for (int x = 0; x <= bounds.size.x; x++) {
            //     Handles.DrawLine(GetWorldCoord(x, 0), GetWorldCoord(x, bounds.size.x,0));
            // }
            // for (int z = 0; z <= bounds.size.x; z++) {
            //     Handles.DrawLine(GetWorldCoord(x, 0), GetWorldCoord(0, bounds.size.x, z));
            // }
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosBounds() {
            if (cells == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;

            bounds.DrawBoundsHandles();

            Handles.color = Color.white;
#endif
        }

        public override string ToString() {
            return $"GridMap({bounds}) of {typeof(TCellObject).Name}s";
        }
    }

    /// <summary>
    /// Optional gridcell to use with gridmap
    /// </summary>
    /// <typeparam name="TGridType">This inherited type</typeparam>
    [System.Serializable]
    public abstract class DefGridCellBase<TGridType> {

        [SerializeReference]
        GridMap<TGridType> _map;
        [SerializeField, ReadOnly]
        Vector3Int _coord;

        public GridMap<TGridType> map { get => _map; private set => _map = value; }
        public Vector3Int coord { get => _coord; private set => _coord = value; }

        public DefGridCellBase(GridMap<TGridType> map, Vector3Int coord) {
            this.map = map;
            this.coord = coord;
        }
        public void TriggerSetEvent() {
            map.TriggerSetEvent();
        }
        public override string ToString() {
            return $"{coord}";
        }
    }
}