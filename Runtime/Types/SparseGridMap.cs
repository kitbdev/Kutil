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
    /// Stores a dict of some object indexed by Vector3Int position, and maintains bounds around them
    /// </summary>
    /// <typeparam name="TCellObject"></typeparam>
    [System.Serializable]
    public class SparseGridMap<TCellObject>{// where TCellObject : class
        //: IEnumerable<TCellObject> 
        // literally just a dict with bounds calculation and maybe some util functions

        public Grid grid;
        [SerializeField] SerializableDictionary<Vector3Int, TCellObject> cells;
        [SerializeField] BoundsInt bounds = new BoundsInt();
        public TCellObject defCellValue;

        public event Action OnBoundsUpdateEvent;

        /// <summary>
        /// bounds that fully encapsulate all cells. Inflated by one to give volume even if there is only one cell
        /// </summary>
        public BoundsInt Bounds => bounds;
        public Dictionary<Vector3Int, TCellObject> CellsDict => cells;

        public SparseGridMap() {
            this.defCellValue = default;
            cells = new SerializableDictionary<Vector3Int, TCellObject>();
            RecalculateBounds();
        }
        public SparseGridMap(Grid grid, TCellObject defCellValue = default) {
            this.grid = grid;
            this.defCellValue = defCellValue;
            cells = new SerializableDictionary<Vector3Int, TCellObject>();
            RecalculateBounds();
        }

        public bool HasCellValueAt(Vector3Int coord) => cells.ContainsKey(coord);
        // public bool TryGetCellAt(Vector3Int coord, out TCellObject val) => cells.TryGetValue(coord, out val);
        // public TCellObject GetCellAt(Vector3Int coord) => cells.GetValueOrDefault(coord);
        public bool TryGetCellAt(Vector3Int coord, out TCellObject val) {
            bool hasVal = cells.TryGetValue(coord, out TCellObject valq);
            val = hasVal ? valq : defCellValue;
            return hasVal;
        }
        public TCellObject GetCellAt(Vector3Int coord) => HasCellValueAt(coord) ? cells.GetValueOrDefault(coord) : defCellValue;
        public IEnumerable<TCellObject> GetCellNeighbors(Vector3Int coord, IEnumerable<Vector3Int> neighborDirs) {
            return neighborDirs.Select(v => v + coord).Where(v => HasCellValueAt(v)).Select(v => GetCellAt(v));
        }
        public IEnumerable<TCellObject> GetAllCells() => cells.Select(kvp => kvp.Value);

        public bool AtBoundEdge(Vector3Int coord) {
            return bounds.IsOnBorder(coord, true);
        }


        public void TriggerBoundsUpdateEvent() {
            OnBoundsUpdateEvent?.Invoke();
        }
        public void RecalculateBounds() {
            if (bounds == null) {
                bounds = new BoundsInt();
            }
            // from scratch
            // Bounds b = new Bounds();
            // ForEach((c, v) => b.Encapsulate(c));
            // bounds = b.AsBoundsInt();
            Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            ForEach((c, v) => {
                if (c.x < minCoord.x) minCoord.x = c.x;
                if (c.y < minCoord.y) minCoord.y = c.y;
                if (c.z < minCoord.z) minCoord.z = c.z;
                if (c.x > maxCoord.x) maxCoord.x = c.x;
                if (c.y > maxCoord.y) maxCoord.y = c.y;
                if (c.z > maxCoord.z) maxCoord.z = c.z;
                //minCoord = Vector3Int.Min(minCoord, c);
                //maxCoord = Vector3Int.Max(maxCoord, c);
            });
            bounds.SetMinMax(minCoord, maxCoord + Vector3Int.one);
            TriggerBoundsUpdateEvent();
        }

        public void ClearAllCells(bool boundsRecalculate = true) {
            cells.Clear();
            if (boundsRecalculate) {
                RecalculateBounds();
            }
        }
        public bool RemoveCell(Vector3Int coord, bool boundsRecalculate = true) {
            bool worked = cells.Remove(coord);
            if (worked && boundsRecalculate) {
                // check if coord was at edge of bounds
                if (AtBoundEdge(coord)) {
                    RecalculateBounds();
                    // todo? instead of recalc from scratch, do some kind of check only at the relevent edge
                    // Vector3Int[] furthestAxis;
                }
            }
            return worked;
        }
        public void RemoveCells(IEnumerable<Vector3Int> coords, bool boundsRecalculate = true) {
            foreach (var coord in coords) {
                cells.Remove(coord);
            }
            if (boundsRecalculate) {
                RecalculateBounds();
            }
        }
        public void RemoveCells(BoundsInt coords, bool boundsRecalculate = true) {
            var ncoords = new BoundsInt(coords.position, coords.size);
            ncoords.ClampToBounds(bounds);
            foreach (var coord in ncoords.allPositionsWithin) {
                cells.Remove(coord);
            }
            if (boundsRecalculate) {
                // bound check only works because it is clamped
                if (AtBoundEdge(ncoords.min) || AtBoundEdge(ncoords.max)) {
                    RecalculateBounds();
                }
            }
        }

        /// <summary>
        /// Sets value at the coord
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="newValue"></param>
        /// <param name="skipBounds"></param>
        public void SetCell(Vector3Int coord, TCellObject newValue, bool skipBounds = false) {
            // ? could do null checks but this method needs to be done fast
            if (HasCellValueAt(coord)) {
                cells[coord] = newValue;
            } else {
                cells.Add(coord, newValue);
                // Debug.Log($"Adding {coord} {newValue} sk:{skipBounds} b:{bounds}");
                if (!skipBounds) {
                    bounds.EncapsulateInclusive(coord);
                    TriggerBoundsUpdateEvent();
                }
            }
        }
        public void SetCells(Vector3Int[] coords, TCellObject[] newValues, bool skipBounds = false) {
            // public void SetCells(IEnumerable<Vector3Int> coords, IEnumerable<TCellObject> newValues, bool skipBounds = false) {
            if (coords.Length != newValues.Length) {
                Debug.LogError($"Cannot set cells, coords and values are not the same length! {coords.Length}:{newValues.Length}");
                return;
            }
            for (int i = 0; i < coords.Length; i++) {
                Vector3Int coord = coords[i];
                TCellObject newValue = newValues[i];
                SetCell(coord, newValue, true);
                if (!skipBounds) {
                    bounds.EncapsulateInclusive(coord);
                }
            }
            if (!skipBounds) {
                TriggerBoundsUpdateEvent();
            }
        }
        public void SetCells(BoundsInt coords, TCellObject[] newCells, bool onlyIfExists = true) {
            int i = 0;
            foreach (var coord in coords.allPositionsWithin) {
                if (!onlyIfExists || HasCellValueAt(coord)) {
                    SetCell(coord, newCells[i]); //?  newCells?[i]
                    i++;
                }
            }
        }

        /// <summary>
        /// Runs an action for each existing cell
        /// </summary>
        /// <param name="action">coordinate, cell value</param>
        public void ForEach(System.Action<Vector3Int, TCellObject> action) {
            // ? try to allow early out
            foreach (var cell in cells) {
                action.Invoke(cell.Key, cell.Value);
            }
        }
        /// <summary>
        /// Runs an action for each existing cell
        /// </summary>
        /// <param name="action">coordinate, cell value</param>
        public void ForEachBounded(System.Action<Vector3Int, TCellObject> action, BoundsInt coords) {
            // ? try to allow early out
            foreach (var kvp in cells) {
                if (bounds.Contains(kvp.Key)) {
                    action.Invoke(kvp.Key, kvp.Value);
                }
            }
        }


        public IEnumerator<TCellObject> GetEnumerator() {
            return cells.Select(kvp => kvp.Value).AsEnumerable().GetEnumerator();
        }
        // IEnumerator IEnumerable.GetEnumerator() {
        //     return cells.GetEnumerator();
        // }
        public override string ToString() {
            return $"SparseGridMap({bounds}) of {typeof(TCellObject).Name}s";
        }
    }
}