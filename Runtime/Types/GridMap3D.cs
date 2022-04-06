// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using System.Runtime.Serialization;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// /// <summary>
// /// Optional gridcell to use with gridmap
// /// </summary>
// /// <typeparam name="TGridType">This inherited type</typeparam>
// [System.Serializable]
// public abstract class GridCell3D<TGridType> {

//     // cant be serialized because circular dependency
//     [NonSerialized]
//     GridMap3D<TGridType> _map;
//     [SerializeField, ReadOnly]
//     Vector2Int _pos;

//     public GridMap3D<TGridType> map { get => _map; private set => _map = value; }
//     public Vector3Int pos { get => _pos; private set => _pos = value; }

//     public GridCell(GridMap3D<TGridType> map, Vector3Int pos) {
//         this.map = map;
//         this.pos = pos;
//     }
//     public void DeserializeSetMap(GridMap3D<TGridType> map) {
//         this.map = map;
//     }
//     public void TriggerUpdateEvent() {
//         map.TriggerUpdateEvent();
//     }
//     public override string ToString() {
//         return $"{pos}";
//     }
// }

// // public class GridMap {
// // static stuff
// // }

// [System.Serializable]
// public class GridMap3D<TGridObject> {

//     [SerializeField, ReadOnly] private int _width;
//     // [SerializeField, ReadOnly] private int _height;
//     [SerializeField] private float _gridSize = 1;
//     [SerializeField] private Vector3 _originPos = Vector3.zero;

//     [SerializeField] protected TGridObject[] grid;

//     public int width { get => _width; protected set => _width = value; }
//     // public int height { get => _height; protected set => _height = value; }
//     // public int length;//?

//     public float gridSize { get => _gridSize; set => _gridSize = value; }
//     public Vector3 originPos { get => _originPos; set => _originPos = value; }

//     protected int volume => width * width * width;

//     /// <summary>
//     /// called when a value is changed
//     /// map.OnAnyValueChanged += (o, args) => { };
//     /// </summary>
//     public event EventHandler OnAnyValueChanged;

//     // public GridMap() {
//     //     grid = new TGridObject[volume];
//     // }

//     // todo manage chunks


//     public GridMap(int width, Func<GridMap3D<TGridObject>, Vector3Int, TGridObject> createFunc = null, float gridSize = 1f) {
//         this.width = width;
//         // this.height = height;
//         this.gridSize = gridSize;
//         grid = new TGridObject[volume];
//         if (createFunc != null) {
//             SetForEach((pos, ival) => createFunc.Invoke(this, pos));
//         } else {
//             // if (typeof(GridCell<TGridObject>).IsAssignableFrom(typeof(TGridObject))){
//             // ForEach((pos, ival) => new GridCell<TGridObject>(this, pos));
//             // (typeof(TGridObject) as GridCell<TGridObject> TCell)
//             // ForEach((pos, ival) => new TGridObject(this, pos));
//             // ! need a create func
//             // }else {
//             // ForEach((pos, ival) => default);
//             // }
//         }
//     }

//     public Vector3 GetWorldPos(Vector3Int pos) {
//         return GetWorldPos(pos.x, pos.y, pos.z);
//     }
//     public Vector3 GetWorldPos(int x, int y, int z) {
//         return new Vector3(x, 0, y) * gridSize + originPos;
//     }
//     public Vector3Int GetPos(Vector3 worldPos) {
//         var localPos = worldPos - originPos;
//         return new Vector3Int(
//             Mathf.FloorToInt(localPos.x / gridSize),
//             Mathf.FloorToInt(localPos.z / gridSize)
//         );
//     }
//     public bool IsPosInBounds(int x, int y, int z) {
//         return (x >= 0 && x < width && y >= 0 && y < width && z >= 0 && z < width);
//     }
//     public bool IsPosInBounds(Vector3Int pos) {
//         return IsPosInBounds(pos.x, pos.y, pos.z);
//     }

//     int ToGridIndex(int x, int y, int z) {
//         return x + y * width * width + z * width;
//     }
//     Vector3Int IndexToPos(int gridIndex) {
//         var pos = Vector3Int.zero;
//         pos.z = gridIndex / width;
//         pos.x = gridIndex - pos.z;
//         // pos.y = gridIndex - pos.y;//todo
//         return pos;
//     }

//     public TGridObject GetCell(Vector3Int pos) {
//         return GetCell(pos.x, pos.y, pos.z);
//     }
//     public TGridObject GetCell(int x, int y, int z) {
//         if (!IsPosInBounds(x, y, z)) {
//             // invalid position
//             Debug.LogWarning($"Invalid position {x},{y},{z}");
//             return default;
//         }
//         return grid[ToGridIndex(x, y, z)];
//     }
//     public TGridObject[] GetAllCells() {
//         return grid;
//     }
//     public void SetAllCells(TGridObject newValue) {
//         for (int y = 0; y < width; y++) {
//             for (int z = 0; z < width; z++) {
//                 for (int x = 0; x < width; x++) {
//                     grid[ToGridIndex(x, y)] = newValue;
//                 }
//             }
//         }
//         OnAnyValueChanged?.Invoke(this, new EventArgs());
//     }
//     public void SetCells(Vector3Int offset, TGridObject[,,] newCells) {
//         // int w = newCells.GetLength(0);//? switch these
//         // int h = newCells.GetLength(1);
//         // for (int y = 0; y < h; y++) {
//         //     for (int x = 0; y < w; x++) {
//         //         int gx = offset.x + x;
//         //         int gy = offset.y + y;
//         //         int gz = offset.z + z;
//         //         grid[ToGridIndex(gx, gy, gz)] = newCells[x, y, z];
//         //     }
//         // }
//         // OnAnyValueChanged?.Invoke(this, new EventArgs());
//     }

//     public bool SetCell(Vector3Int pos, TGridObject newValue) {
//         return SetCell(pos.x, pos.y, pos.z, newValue);
//     }
//     public bool SetCell(int x, int y, int z, TGridObject newValue) {
//         if (!IsPosInBounds(x, y, z)) {
//             // invalid position
//             Debug.LogWarning($"Invalid position {x},{y},{z}");
//             return false;
//         }
//         grid[ToGridIndex(x, y, z)] = newValue;
//         OnAnyValueChanged?.Invoke(this, new EventArgs());
//         return true;
//     }

//     public void SetForEach(System.Func<Vector3Int, TGridObject, TGridObject> action) {
//         for (int y = 0; y < width; y++) {
//             for (int z = 0; z < width; z++) {
//                 for (int x = 0; x < width; x++) {
//                     grid[ToGridIndex(x, y, z)] = action.Invoke(new Vector3Int(x, y, z), grid[ToGridIndex(x, y, z)]);
//                 }
//             }
//         }
//         OnAnyValueChanged?.Invoke(this, new EventArgs());
//     }
//     public void ForEach(System.Action<TGridObject> action, bool triggerUpdate = true) {
//         // ? try to allow early out
//         for (int y = 0; y < width; y++) {
//             for (int z = 0; z < width; z++) {
//                 for (int x = 0; x < width; x++) {
//                     action.Invoke(grid[ToGridIndex(x, y, z)]);
//                 }
//             }
//         }
//         if (triggerUpdate) {
//             OnAnyValueChanged?.Invoke(this, new EventArgs());
//         }
//     }
//     public void ForEach(IEnumerable<Vector3Int> positions, System.Action<TGridObject> action, bool triggerUpdate = true) {
//         foreach (var pos in positions) {
//             if (!IsPosInBounds(pos)) {
//                 Debug.LogWarning($"Invalid pos {pos} in map foreach!");
//                 continue;
//             }
//             action.Invoke(grid[ToGridIndex(pos.x, pos.y, pos.z)]);
//         }
//         if (triggerUpdate) {
//             OnAnyValueChanged?.Invoke(this, new EventArgs());
//         }
//     }

//     public void TriggerUpdateEvent() {
//         OnAnyValueChanged?.Invoke(this, new EventArgs());
//     }

//     public void DrawGizmosValues() {
//         if (grid == null) return;
// #if UNITY_EDITOR
//         Handles.color = Color.gray;
//         // values
//         // todo only if in view
//         for (int y = 0; y < width; y++) {
//             for (int z = 0; z < width; z++) {
//             for (int x = 0; x < width; x++) {
//                 string text = grid[ToGridIndex(x, y,z)]?.ToString();
//                 Vector3 pos = GetWorldPos(x, y,z);
//                 Handles.Label(pos + new Vector3(gridSize, 0, gridSize) / 2f, text);
//             }
//             }
//         }
//         // grid
//         DrawGizmosGrid();
//         Handles.color = Color.white;
// #endif
//     }
//     public void DrawGizmosGrid() {
//         if (grid == null) return;
// #if UNITY_EDITOR
//         Handles.color = Color.gray;
//         // grid
//         // for (int y = 0; y <= width; y++) {
//         //     Handles.DrawLine(GetWorldPos(0, y), GetWorldPos(width, y,0));
//         // }
//         // for (int x = 0; x <= width; x++) {
//         //     Handles.DrawLine(GetWorldPos(x, 0), GetWorldPos(x, width,0));
//         // }
//         // for (int z = 0; z <= width; z++) {
//         //     Handles.DrawLine(GetWorldPos(x, 0), GetWorldPos(0, width, z));
//         // }
//         Handles.color = Color.white;
// #endif
//     }
//     public void DrawGizmosBounds() {
//         if (grid == null) return;
// #if UNITY_EDITOR
//         Handles.color = Color.gray;

//         Vector3[] poses = new Vector3[]{
//              GetWorldPos(0, 0, 0),
//              GetWorldPos(width, 0, 0),
//              GetWorldPos(0, width, 0),
//              GetWorldPos(width, width, 0),
//              GetWorldPos(0, 0, width),
//              GetWorldPos(width, 0, width),
//              GetWorldPos(0, width, width),
//              GetWorldPos(width, width, width),
//              };
//         Handles.DrawLine(poses[0], poses[1]);
//         Handles.DrawLine(poses[0], poses[2]);
//         Handles.DrawLine(poses[2], poses[3]);
//         Handles.DrawLine(poses[1], poses[3]);
//         // todo
//         Handles.color = Color.white;
// #endif
//     }

//     [OnDeserialized]
//     private void OnDeserialized(object o) {
//         for (int y = 0; y < width; y++) {
//             for (int z = 0; z < width; z++) {
//                 for (int x = 0; x < width; x++) {
//                     TGridObject gridObject = grid[ToGridIndex(x, y)];
//                     if (gridObject is GridCell<TGridObject> gridCell) {
//                         gridCell.DeserializeSetMap(this);
//                     }
//                 }
//             }
//         }
//     }
// }