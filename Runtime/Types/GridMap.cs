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
    /// Optional gridcell to use with gridmap
    /// </summary>
    /// <typeparam name="TGridType">This inherited type</typeparam>
    [System.Serializable]
    public abstract class GridCell<TGridType> {

        [SerializeReference]
        GridMap<TGridType> _map;
        [SerializeField, ReadOnly]
        Vector3Int _pos;

        public GridMap<TGridType> map { get => _map; private set => _map = value; }
        public Vector3Int pos { get => _pos; private set => _pos = value; }

        public GridCell(GridMap<TGridType> map, Vector3Int pos) {
            this.map = map;
            this.pos = pos;
        }
        public void TriggerUpdateEvent() {
            map.TriggerUpdateEvent();
        }
        public override string ToString() {
            return $"{pos}";
        }
    }

    // public class GridMap {
    // static stuff
    // }

    [System.Serializable]
    public class GridMap<TGridObject> {

        [SerializeField, ReadOnly] private int _width;//x
        [SerializeField, ReadOnly] private int _length;//z
        [SerializeField, ReadOnly] private int _height;//y
        [SerializeField] private Grid grid;

        [SerializeReference]
        [SerializeField]
        protected TGridObject[] map;

        [SerializeField]
        public Func<GridMap<TGridObject>, Vector3Int, TGridObject> createFunc = null;
        [SerializeField]
        public Action<TGridObject, Vector3Int> destoryAction = null;

        public int width { get => _width; protected set => _width = value; }
        public int length { get => _length; protected set => _length = value; }
        public int height { get => _height; protected set => _height = value; }
        public Vector3Int dimensions => new Vector3Int(width, height, length);

        protected int volume => width * length * height;
        protected int floorArea => width * length;

        /// <summary>
        /// called when a value is changed
        /// map.OnAnyValueChanged += (o, args) => { };
        /// </summary>
        public System.Action OnAnyValueChanged;

        // todo manage chunks?


        public GridMap(Vector3Int dimensions, Grid grid, Func<GridMap<TGridObject>, Vector3Int, TGridObject> createFunc = null,
         Action<TGridObject, Vector3Int> destoryAction = null) {
            this.width = dimensions.x;
            this.height = dimensions.y;
            this.length = dimensions.z;
            this.grid = grid;

            map = new TGridObject[volume];
            this.createFunc = createFunc;
            this.destoryAction = destoryAction;
            RecreateMap();
        }

        public void RecreateMap() {
            if (createFunc != null) {
                SetForEach((pos, ival) => createFunc.Invoke(this, pos));
            } else {
                // dont set, it can be done externally
            }
        }
        public void ClearAll() {
            SetForEach((pos, ival) => default);
            // map is now filled with nulls (well defaults, if struct)
            //? should set size to 0?
        }
        void ResizeMap(Vector3Int newDimensions, Vector3Int originalOffset = default) {
            Vector3Int originalDimensions = dimensions;
            BoundsInt origBounds = new BoundsInt(originalOffset, dimensions);
            TGridObject[] originalMap = map;
            this.width = newDimensions.x;
            this.height = newDimensions.y;
            this.length = newDimensions.z;
            map = new TGridObject[volume];
            if (createFunc != null) {
                SetForEach((pos, ival) => {
                    if (origBounds.Contains(pos)) {
                        Vector3Int opos = pos - originalOffset;
                        int oldIndex = opos.x + opos.z * originalDimensions.x + opos.y * originalDimensions.x * originalDimensions.z;
                        return originalMap[oldIndex];
                    } else {
                        return createFunc.Invoke(this, pos);
                    }
                });
            }
            // destroy old ones that are now oob
            for (int i = 0; i < originalMap.Length; i++) {
                TGridObject item = originalMap[i];
                // if not added to new one
                if (!map.Contains(item)) {
                    // not using index to pos func cause we are in the old map
                    var oldpos = Vector3Int.zero;
                    int index = i;
                    oldpos.y = index / (originalDimensions.x * originalDimensions.z);
                    index -= (oldpos.y * originalDimensions.x * originalDimensions.z);
                    oldpos.z = index / originalDimensions.x;
                    oldpos.x = index % originalDimensions.x;
                    if (destoryAction != null) destoryAction.Invoke(item, oldpos);
                }
            }
        }

        public bool IsPosInBounds(int x, int y, int z) {
            return IsPosInBounds(new Vector3Int(x, y, z));
        }
        public bool IsPosInBounds(Vector3Int pos) {
            return (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && pos.z >= 0 && pos.z < length);
        }

        int ToMapIndex(Vector3Int cellpos) {
            return ToMapIndex(cellpos.x, cellpos.y, cellpos.z);
        }
        int ToMapIndex(int x, int y, int z) {
            if (!IsPosInBounds(x, y, z)) return -1;
            return x + z * width + y * floorArea;
        }
        Vector3Int IndexToCellPos(int mapIndex) {
            var pos = Vector3Int.zero;
            pos.y = mapIndex / (width * length);
            mapIndex -= (pos.y * width * length);
            pos.z = mapIndex / width;
            pos.x = mapIndex % width;
            return pos;
        }

        public TGridObject GetCell(Vector3Int pos) {
            return GetCell(pos.x, pos.y, pos.z);
        }
        public TGridObject GetCell(int x, int y, int z) {
            if (!IsPosInBounds(x, y, z)) {
                // invalid position
                Debug.LogWarning($"Invalid position {x},{y},{z}");
                return default;
            }
            int mapIndex = ToMapIndex(x, y, z);
            return map[mapIndex];
        }
        public TGridObject[] GetAllCells() {
            return map;
        }
        public void SetAllCells(TGridObject newValue) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < length; z++) {
                    for (int x = 0; x < width; x++) {
                        map[ToMapIndex(x, y, z)] = newValue;
                    }
                }
            }
            OnAnyValueChanged?.Invoke();
        }
        // public void SetCells(Vector3Int offset, TGridObject[,,] newCells) {
        // int w = newCells.GetLength(0);//? switch these
        // int h = newCells.GetLength(1);
        // for (int y = 0; y < h; y++) {
        //     for (int x = 0; y < w; x++) {
        //         int gx = offset.x + x;
        //         int gy = offset.y + y;
        //         int gz = offset.z + z;
        //         grid[ToGridIndex(gx, gy, gz)] = newCells[x, y, z];
        //     }
        // }
        // OnAnyValueChanged?.Invoke(this, new EventArgs());
        // }

        public bool SetCell(Vector3Int pos, TGridObject newValue) {
            return SetCell(pos.x, pos.y, pos.z, newValue);
        }
        public bool SetCell(int x, int y, int z, TGridObject newValue) {
            if (!IsPosInBounds(x, y, z)) {
                // invalid position
                Debug.LogWarning($"Invalid position {x},{y},{z}");
                return false;
            }
            if (destoryAction != null) {
                int mpindex = ToMapIndex(x, y, z);
                TGridObject original = map[mpindex];
                Vector3Int pos = new Vector3Int(x, y, z);
                if (original != null) {
                    // Debug.Log($"clearing {original} {pos} nn{original != null} d{destoryAction}");
                    destoryAction.Invoke(original, pos);
                }
            }
            map[ToMapIndex(x, y, z)] = newValue;
            OnAnyValueChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Set 
        /// </summary>
        /// <param name="action">cellpos, original value-> new value</param>
        public void SetForEach(System.Func<Vector3Int, TGridObject, TGridObject> action) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < length; z++) {
                    for (int x = 0; x < width; x++) {
                        int mpindex = ToMapIndex(x, y, z);
                        TGridObject original = map[mpindex];
                        Vector3Int pos = new Vector3Int(x, y, z);
                        if (original != null) {
                            // Debug.Log($"clearing {original} {pos} nn{original != null} d{destoryAction}");
                            destoryAction?.Invoke(original, pos);
                        }
                        TGridObject newGridObject = action.Invoke(pos, original);
                        map[mpindex] = newGridObject;
                    }
                }
            }
            OnAnyValueChanged?.Invoke();
        }
        public void ForEach(System.Action<TGridObject> action, bool triggerUpdate = true) {
            // ? try to allow early out
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < length; z++) {
                    for (int x = 0; x < width; x++) {
                        action.Invoke(map[ToMapIndex(x, y, z)]);
                    }
                }
            }
            if (triggerUpdate) {
                OnAnyValueChanged?.Invoke();
            }
        }
        public void ForEach(IEnumerable<Vector3Int> positions, System.Action<TGridObject> action, bool triggerUpdate = true) {
            foreach (var pos in positions) {
                if (!IsPosInBounds(pos)) {
                    Debug.LogWarning($"Invalid pos {pos} in map foreach!");
                    continue;
                }
                action.Invoke(map[ToMapIndex(pos.x, pos.y, pos.z)]);
            }
            if (triggerUpdate) {
                OnAnyValueChanged?.Invoke();
            }
        }

        public void TriggerUpdateEvent() {
            OnAnyValueChanged?.Invoke();
        }

        public void DrawGizmosValues() {
            if (map == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // values
            // todo only if in view
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < length; z++) {
                    for (int x = 0; x < width; x++) {
                        string text = map[ToMapIndex(x, y, z)]?.ToString();
                        Vector3 pos = grid.CellToWorld(new Vector3Int(x, y, z));
                        Handles.Label(pos, text);
                    }
                }
            }
            // grid
            DrawGizmosGrid();
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosGrid() {
            if (map == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;
            // grid
            // for (int y = 0; y <= width; y++) {
            //     Handles.DrawLine(GetWorldPos(0, y), GetWorldPos(width, y,0));
            // }
            // for (int x = 0; x <= width; x++) {
            //     Handles.DrawLine(GetWorldPos(x, 0), GetWorldPos(x, width,0));
            // }
            // for (int z = 0; z <= width; z++) {
            //     Handles.DrawLine(GetWorldPos(x, 0), GetWorldPos(0, width, z));
            // }
            Handles.color = Color.white;
#endif
        }
        public void DrawGizmosBounds() {
            if (map == null) return;
#if UNITY_EDITOR
            Handles.color = Color.gray;

            Vector3[] cornerPositions = new Vector3[]{
             grid.CellToWorld(new Vector3Int(0,     0,      0)),
             grid.CellToWorld(new Vector3Int(width, 0,      0)),
             grid.CellToWorld(new Vector3Int(0,     height, 0)),
             grid.CellToWorld(new Vector3Int(width, height, 0)),
             grid.CellToWorld(new Vector3Int(0,     0,      length)),
             grid.CellToWorld(new Vector3Int(width, 0,      length)),
             grid.CellToWorld(new Vector3Int(0,     height, length)),
             grid.CellToWorld(new Vector3Int(width, height, length)),
             };
            Handles.DrawLine(cornerPositions[0], cornerPositions[1]);
            Handles.DrawLine(cornerPositions[0], cornerPositions[2]);
            Handles.DrawLine(cornerPositions[2], cornerPositions[3]);
            Handles.DrawLine(cornerPositions[1], cornerPositions[3]);
            // todo
            Handles.color = Color.white;
#endif
        }

    }
}