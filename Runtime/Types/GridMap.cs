using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;
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

        // cant be serialized because circular dependency
        [NonSerialized]
        GridMap<TGridType> _map;
        [SerializeField, ReadOnly]
        Vector2Int _pos;

        public GridMap<TGridType> map { get => _map; private set => _map = value; }
        public Vector2Int pos { get => _pos; private set => _pos = value; }
        public Vector2Int worldPos => map?.originPos2 + pos ?? Vector2Int.zero;

        public GridCell(GridMap<TGridType> map, Vector2Int pos) {
            this.map = map;
            this.pos = pos;
        }
        public void DeserializeSetMap(GridMap<TGridType> map) {
            this.map = map;
        }
        public void TriggerUpdateEvent() {
            map.TriggerUpdateEvent();
        }
        public override string ToString() {
            Vector2Int wpos = pos;
            if (map != null) {
                int index = map.ToGridIndex(pos.x, pos.y);
                return $"Cell{index}{worldPos}({pos})";
            } else {
                return $"Lost Cell({pos})";
            }
            // Debug.Log(map.originPos2);
        }
    }

    public static class GridMap {
        // static stuff
        public static Vector2Int ToCellPos(Vector3 worldPos, float gridSize = 1) {
            return ToCellPos(worldPos, Vector3.zero, gridSize);
        }
        public static Vector2Int ToCellPos(Vector3 worldPos, Vector3 originPos, float gridSize = 1) {
            var localPos = worldPos - originPos;
            Vector2Int cellPos = new Vector2Int(
                        Mathf.FloorToInt(localPos.x / gridSize),
                        Mathf.FloorToInt(localPos.z / gridSize)
                    );
            return cellPos;
        }
        public static Vector3 ToWorldPos(Vector2Int cellpos, float gridSize = 1) {
            Vector3 wpos = new Vector3(cellpos.x, 0, cellpos.y) * gridSize;
            return wpos;
        }
        // public static Vector2Int CellLocalToCellWorld(Vector2Int cellpos, float gridSize = 1) {
        //     Vector3 wpos = new Vector3(cellpos.x, 0, cellpos.y) * gridSize;
        //     return wpos;
        // }
    }

    [System.Serializable]
    public class GridMap<TGridObject> : ISerializationCallbackReceiver {

        [SerializeField, ReadOnly] private int _width;
        [SerializeField, ReadOnly] private int _height;
        [SerializeField] private float _gridSize = 1;
        [SerializeField] private Vector3 _originPos = Vector3.zero;

        [SerializeField] protected TGridObject[] grid;

        public int width { get => _width; protected set => _width = value; }
        public int height { get => _height; protected set => _height = value; }

        public float gridSize { get => _gridSize; set => _gridSize = value; }
        public Vector3 originPos { get => _originPos; set => _originPos = value; }
        public Vector2Int originPos2 => new Vector2Int(Mathf.FloorToInt(_originPos.x), Mathf.FloorToInt(_originPos.z));

        protected int volume => width * height;

        /// <summary>
        /// called when a value is changed
        /// map.OnAnyValueChanged += (o, args) => { };
        /// </summary>
        public event EventHandler OnAnyValueChanged;
        // public event EventHandler<OnGridValueChangedArgs> OnValueChanged;
        // public class OnGridValueChangedArgs {
        //     Vector2Int pos;
        // }

        // public GridMap() {
        //     grid = new TGridObject[volume];
        // }
        // public GridMap(int width, int height, float gridSize, Vector3 originPos) {
        //     this.width = width;
        //     this.height = height;
        //     this.gridSize = gridSize;
        //     this.originPos = originPos;
        //     grid = new TGridObject[volume];
        // }
        public GridMap(int width, int height, Func<GridMap<TGridObject>, Vector2Int, TGridObject> createFunc = null, float gridSize = 1f) {
            this.width = width;
            this.height = height;
            this.gridSize = gridSize;
            grid = new TGridObject[volume];
            if (createFunc != null) {
                SetForEach((pos, ival) => createFunc.Invoke(this, pos));
            } else {
                // if (typeof(GridCell<TGridObject>).IsAssignableFrom(typeof(TGridObject))){
                // ForEach((pos, ival) => new GridCell<TGridObject>(this, pos));
                // (typeof(TGridObject) as GridCell<TGridObject> TCell)
                // ForEach((pos, ival) => new TGridObject(this, pos));
                // ! need a create func
                // }else {
                // ForEach((pos, ival) => default);
                // }
            }
        }

        public Vector3 GetWorldPos(Vector2Int pos) {
            return GetWorldPos(pos.x, pos.y);
        }
        public Vector3 GetWorldPos(int x, int y) {
            return new Vector3(x, 0, y) * gridSize + originPos;
        }
        public Vector2Int GetPos(Vector3 worldPos) {
            var localPos = worldPos - originPos;
            return new Vector2Int(
                Mathf.FloorToInt(localPos.x / gridSize),
                Mathf.FloorToInt(localPos.z / gridSize)
            );
        }
        public bool IsPosInBounds(int x, int y) {
            return (x >= 0 && y >= 0 && x < width && y < height);
        }
        public bool IsPosInBounds(Vector2Int pos) {
            return IsPosInBounds(pos.x, pos.y);
        }

        public int ToGridIndex(int x, int y) {
            return x + y * width;
        }
        Vector2Int ToXY(int gridIndex) {
            var pos = Vector2Int.zero;
            pos.y = gridIndex / width;
            pos.x = gridIndex - pos.y;
            return pos;
        }

        public TGridObject GetCell(Vector2Int pos) {
            return GetCell(pos.x, pos.y);
        }
        /// <summary>
        /// No safety checks
        /// </summary>
        public TGridObject GetCellRaw(int x, int y) {
            return grid[ToGridIndex(x, y)];
        }
        public TGridObject GetCell(int x, int y) {
            if (!IsPosInBounds(x, y)) {
                // invalid position
                Debug.LogWarning($"Invalid position {x},{y}");
                return default;
            }
            return grid[ToGridIndex(x, y)];
        }
        public TGridObject[] GetAllCells() {
            return grid;
        }
        public void SetAllCells(TGridObject newValue) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    grid[ToGridIndex(x, y)] = newValue;
                }
            }
            OnAnyValueChanged?.Invoke(this, new EventArgs());
        }
        public void SetCells(Vector2Int offset, TGridObject[,] newCells) {
            int w = newCells.GetLength(0);//? switch these
            int h = newCells.GetLength(1);
            for (int y = 0; y < h; y++) {
                for (int x = 0; y < w; x++) {
                    int gx = offset.x + x;
                    int gy = offset.y + y;
                    grid[ToGridIndex(gx, gy)] = newCells[x, y];
                }
            }
            OnAnyValueChanged?.Invoke(this, new EventArgs());
        }

        public bool SetCell(Vector2Int pos, TGridObject newValue) {
            return SetCell(pos.x, pos.y, newValue);
        }
        public bool SetCell(int x, int y, TGridObject newValue) {
            if (!IsPosInBounds(x, y)) {
                // invalid position
                Debug.LogWarning($"Invalid position {x},{y}");
                return false;
            }
            grid[ToGridIndex(x, y)] = newValue;
            OnAnyValueChanged?.Invoke(this, new EventArgs());
            return true;
        }

        public void SetForEach(System.Func<Vector2Int, TGridObject, TGridObject> action) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    grid[ToGridIndex(x, y)] = action.Invoke(new Vector2Int(x, y), grid[ToGridIndex(x, y)]);
                }
            }
            OnAnyValueChanged?.Invoke(this, new EventArgs());
        }
        public void ForEach(System.Action<TGridObject> action, bool triggerUpdate = true) {
            // ? try to allow early out
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    action.Invoke(grid[ToGridIndex(x, y)]);
                }
            }
            if (triggerUpdate) {
                OnAnyValueChanged?.Invoke(this, new EventArgs());
            }
        }
        public void ForEach(IEnumerable<Vector2Int> positions, System.Action<TGridObject> action, bool triggerUpdate = true) {
            if (positions == null) return;
            foreach (var pos in positions) {
                if (!IsPosInBounds(pos)) {
                    Debug.LogWarning($"Invalid pos {pos} in map foreach!");
                    continue;
                }
                action.Invoke(grid[ToGridIndex(pos.x, pos.y)]);
            }
            if (triggerUpdate) {
                OnAnyValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public void TriggerUpdateEvent() {
            OnAnyValueChanged?.Invoke(this, new EventArgs());
        }

        public void DrawGizmosValues() {
            if (grid == null) return;
#if UNITY_EDITOR
        Handles.color = Color.gray;
        // values
        // todo only if in view
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                string text = grid[ToGridIndex(x, y)]?.ToString();
                Vector3 pos = GetWorldPos(x, y);
                Handles.Label(pos + new Vector3(gridSize, 0, gridSize) / 2f, text);
            }
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
        for (int y = 0; y <= height; y++) {
            Handles.DrawLine(GetWorldPos(0, y), GetWorldPos(width, y));
        }
        for (int x = 0; x <= width; x++) {
            Handles.DrawLine(GetWorldPos(x, 0), GetWorldPos(x, height));
        }
        Handles.color = Color.white;
#endif
        }
        public void DrawGizmosBounds() {
            if (grid == null) return;
#if UNITY_EDITOR

        Vector3[] poses = new Vector3[]{
             GetWorldPos(0, 0),
             GetWorldPos(width, 0),
             GetWorldPos(0, height),
             GetWorldPos(width, height),
             };
        Handles.DrawLine(poses[0], poses[1]);
        Handles.DrawLine(poses[0], poses[2]);
        Handles.DrawLine(poses[2], poses[3]);
        Handles.DrawLine(poses[1], poses[3]);
#endif
        }

        public void OnBeforeSerialize() {
            // throw new NotImplementedException();
        }

        public void OnAfterDeserialize() {
            if (grid == null) return;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    TGridObject gridObject = grid[ToGridIndex(x, y)];
                    if (gridObject is GridCell<TGridObject> gridCell) {
                        gridCell.DeserializeSetMap(this);
                    }
                }
            }
        }
    }
}