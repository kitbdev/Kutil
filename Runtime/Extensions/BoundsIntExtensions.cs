using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    public static class BoundsIntExtensions {
        /// <summary>
        /// Converts BoundsInt to Bounds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds AsBounds(this BoundsInt bounds) {
            return new Bounds(bounds.center, bounds.size);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt Copy(this BoundsInt bounds) {
            return new BoundsInt(bounds.position, bounds.size);
        }
        /// <summary>
        /// Converts Bounds to BoundsInt. expands volume if necessary
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt AsBoundsInt(this Bounds bounds) {
            BoundsInt boundsInt = new BoundsInt();
            boundsInt.SetMinMax(Vector3Int.FloorToInt(bounds.min), Vector3Int.CeilToInt(bounds.max));
            return boundsInt;
        }
        /// <summary>
        /// Flattens into a RectInt. Removes the Z axis. Can Swizzle for different axis.
        /// </summary>
        /// <param name="swizzle">Swizzle to flatten on other axis</param>
        public static Rect AsRect(this Bounds bounds, GridLayout.CellSwizzle swizzle = default) {
            return new Rect((Vector2)Grid.Swizzle(swizzle, bounds.min), (Vector2)Grid.Swizzle(swizzle, bounds.size));
        }
        /// <summary>
        /// Flattens into a RectInt. Removes the Z axis. Can Swizzle for different axis.
        /// </summary>
        /// <param name="swizzle">Swizzle to flatten on other axis</param>
        public static RectInt AsRectInt(this BoundsInt bounds, GridLayout.CellSwizzle swizzle = default) {
            return new RectInt(Vector2Int.FloorToInt((Vector2)Grid.Swizzle(swizzle, bounds.min)),
                Vector2Int.FloorToInt(((Vector2)Grid.Swizzle(swizzle, bounds.size))));
        }
        /// <summary>
        /// Returns true if this BoundsInt contains another entirely
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBounds(this BoundsInt bounds, BoundsInt smaller) {
            return bounds.Contains(smaller.min) && bounds.Contains(smaller.max);
        }
        /// <summary>integer volume of this bounds</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Volume(this BoundsInt bounds) {
            return bounds.size.x * bounds.size.y * bounds.size.z;
        }

        // copy bounds functionality
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Geometry/Bounds.cs
        // ? rewrite for faster performance

        /// <summary>
        /// Does another bounding box intersect with this bounding box?
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if intersects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Intersects(this BoundsInt bounds, BoundsInt other) {
            // return bounds.AsBounds().Intersects(other.AsBounds());
            return (bounds.min.x <= other.max.x) && (bounds.max.x >= other.min.x) &&
                (bounds.min.y <= other.max.y) && (bounds.max.y >= other.min.y) &&
                (bounds.min.z <= other.max.z) && (bounds.max.z >= other.min.z);
        }
        /// <summary>
        /// Grows the bounds to include the point.
        /// </summary>
        /// <param name="newPoint"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Encapsulate(this ref BoundsInt bounds, Vector3Int newPoint) {
            // var b = bounds.AsBounds();
            // b.Encapsulate(newPoint);
            // var bi = b.AsBoundsInt();
            // bounds.SetMinMax(bi.min, bi.max);
            // Debug.Log($"bounds encapsulate o:{bounds} p:{newPoint} min:{Vector3Int.Min(bounds.min, newPoint)}, max:{Vector3Int.Max(bounds.max, newPoint)}");
            bounds.SetMinMax(Vector3Int.Min(bounds.min, newPoint), Vector3Int.Max(bounds.max, newPoint));
            // Debug.Log($"new bounds:{bounds}");
        }
        /// <summary>
        /// Grows the bounds to include the point. Inclusive
        /// </summary>
        /// <param name="newPoint"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncapsulateInclusive(this ref BoundsInt bounds, Vector3Int newPoint) {
            // Debug.Log($"bounds encapsulate o:{bounds} p:{newPoint} min:{Vector3Int.Min(bounds.min, newPoint + Vector3Int.one)}, max:{Vector3Int.Max(bounds.max, newPoint)}");
            //? call it inflated instead of inclusive?
            bounds.SetMinMax(Vector3Int.Min(bounds.min, newPoint), Vector3Int.Max(bounds.max, newPoint + Vector3Int.one));
            // Debug.Log($"new bounds:{bounds}");
        }
        /// <summary>
        /// Expand the bounds by increasing its size by amount along each side
        /// </summary>
        /// <param name="amount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand(this ref BoundsInt bounds, int amount) {
            // var b = bounds.AsBounds();
            // b.Expand(amount);
            // var bi = b.AsBoundsInt();
            // bounds.SetMinMax(bi.min, bi.max);
            bounds.Expand(new Vector3Int(amount, amount, amount));
        }
        /// <summary>
        /// Expand the bounds by increasing its size by amount along each side
        /// </summary>
        /// <param name="amount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand(this ref BoundsInt bounds, Vector3Int amount) {
            // var b = bounds.AsBounds();
            // b.Expand(amount);
            // var bi = b.AsBoundsInt();
            // bounds.SetMinMax(bi.min, bi.max);
            bounds.size += amount;
        }

        public static Vector3Int CenterInt(this BoundsInt bounds) => bounds.position + bounds.size / 2;

        // /// <summary>
        // /// Returns true if this BoundsInt contains another entirely
        // /// </summary>
        // public static bool ContainsInclusive(this BoundsInt bounds, Vector3Int point) {
        //     return bounds.Contains(point)||bounds;
        // }
        /// <summary>
        /// grows bounds to include top right corner
        /// </summary>
        /// <returns>new boundsint</returns>
        public static BoundsInt MakeInclusive(this BoundsInt bounds) {
            BoundsInt b = new();
            b.SetMinMax(bounds.min, bounds.max + Vector3Int.one);
            return b;
        }
        /// <summary>
        /// shrinks bounds to exclude top right corner
        /// </summary>
        /// <returns>new boundsint</returns>
        public static BoundsInt MakeExclusive(this BoundsInt bounds) {
            BoundsInt b = new();
            b.SetMinMax(bounds.min, bounds.max - Vector3Int.one);
            return b;
        }
        /// <summary>
        /// Returns true if the point is on the boundaries of the BoundsInt.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="point"></param>
        /// <param name="inclusive">are the bounds inclusive</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOnBorder(this BoundsInt bounds, Vector3Int point, bool inclusive = false) {
            int inc = inclusive ? -1 : 0;
            return
                point.x == bounds.xMin ||
                point.x == bounds.xMax + inc ||
                point.y == bounds.yMin ||
                point.y == bounds.yMax + inc ||
                point.z == bounds.zMin ||
                point.z == bounds.zMax + inc;
        }
        /// <summary>
        /// Returns the Euclidean distance to the point from the closest point on the border.
        /// negative if interior
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3Int DistanceTo(this BoundsInt bounds, Vector3Int point) {
            // Vector3Int maxDist = Vector3IntExt.Abs(bounds.max - point);
            // Vector3Int minDist = Vector3IntExt.Abs(bounds.min - point);
            // return Vector3Int.Min(maxDist, minDist);
            Vector3Int bpoint = bounds.ClosestPointOnBorder(point);
            return point - bpoint;
        }
        /// <summary>
        /// Returns the closest point on the box or inside the box. Clamps
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ClosestPoint(this BoundsInt bounds, Vector3Int point) {
            // Vector3Int closestPoint = point;
            // if (point.x < bounds.xMin) closestPoint.x = bounds.xMin;
            // else if (point.x > bounds.xMax) closestPoint.x = bounds.xMax;
            // if (point.y < bounds.yMin) closestPoint.y = bounds.yMin;
            // else if (point.y > bounds.yMax) closestPoint.y = bounds.yMax;
            // if (point.z < bounds.zMin) closestPoint.z = bounds.zMin;
            // else if (point.z > bounds.zMax) closestPoint.z = bounds.zMax;
            // return closestPoint;
            point.Clamp(bounds.min, bounds.max);// - Vector3Int.one
            return point;
        }
        /// <summary>
        /// Returns the closest point on the box. not inside the box.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        static Vector3Int ClosestPointOnBorder(this BoundsInt bounds, Vector3Int point) {
            Vector3Int closestPoint = bounds.ClosestPoint(point);
            if (bounds.IsOnBorder(closestPoint)) {
                return closestPoint;
            }
            // find the nearest axis
            Vector3Int minCDist = bounds.min - closestPoint;
            Vector3Int maxCDist = bounds.max - closestPoint;
            Vector3Int[] dirs = new Vector3Int[6]{
                minCDist.x * Vector3Int.right,
                minCDist.y * Vector3Int.up,
                minCDist.z * Vector3Int.down,
                maxCDist.x * Vector3Int.right,
                maxCDist.y * Vector3Int.up,
                maxCDist.z * Vector3Int.down,
            };
            float smallestDist = float.MaxValue;
            int smallestIndex = int.MaxValue;
            for (int i = 0; i < dirs.Length; i++) {
                Vector3Int dir = dirs[i];
                if (dir.sqrMagnitude < smallestDist) {
                    smallestDist = dir.sqrMagnitude;
                    smallestIndex = i;
                }
            }
            closestPoint += dirs[smallestIndex];
            return closestPoint;
        }

        public static Vector3Int[] CornerPositions(this BoundsInt bounds) {
            return new Vector3Int[]{
                new Vector3Int(bounds.xMin, bounds.yMin, bounds.zMin),
                new Vector3Int(bounds.xMax, bounds.yMin, bounds.zMin),
                new Vector3Int(bounds.xMin, bounds.yMax, bounds.zMin),
                new Vector3Int(bounds.xMax, bounds.yMax, bounds.zMin),
                new Vector3Int(bounds.xMin, bounds.yMin, bounds.zMax),
                new Vector3Int(bounds.xMax, bounds.yMin, bounds.zMax),
                new Vector3Int(bounds.xMin, bounds.yMax, bounds.zMax),
                new Vector3Int(bounds.xMax, bounds.yMax, bounds.zMax),
            };
        }
        public static Vector3[] CornerPositions(this Bounds bounds) {
            return new Vector3[]{
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
            };
        }
        public static (int, int)[] edgeIndexes => new (int, int)[12]{
            (0, 1),// bottom square
            (0, 2),
            (2, 3),
            (1, 3),
            (0, 4),// vertical lines
            (1, 5),
            (2, 6),
            (3, 7),
            (4, 5),// top square
            (4, 6),
            (6, 7),
            (5, 7),
        };

        // gizmos stuff

        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawGizmosBounds(this BoundsInt bounds, Grid grid = null) {
            DrawGizmosBounds(bounds, Vector3.zero, grid);
        }
        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawGizmosBounds(this BoundsInt bounds, Vector3 offset, Grid grid = null) {
            Vector3[] poses = bounds.CornerPositions().Select(p => (grid?.CellToWorld(p) ?? (Vector3)p) + offset).ToArray();
            HandlesDrawCube(poses);
        }
        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawGizmosBounds(this Bounds bounds) {
            HandlesDrawCube(bounds.CornerPositions());
        }
        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawGizmosBounds(this Bounds bounds, Transform transform) {
            Vector3[] points = bounds.CornerPositions();
            System.Span<Vector3> poses = new(points);
            transform.TransformPoints(poses, poses);
            // Vector3[] poses = bounds.CornerPositions().Select(p => transform.TransformPoint(p)).ToArray();
            HandlesDrawCube(poses.ToArray());
        }

        /// <summary>Draw outline of a cube from 8 positions</summary>
        public static void HandlesDrawCube(Vector3[] poses) {
            if (poses == null || poses.Length != 8) {
                Debug.LogError($"DrawCube needs 8 positions to draw has {(poses?.Length.ToString() ?? "null")}");
                return;
            }
#if UNITY_EDITOR
            Handles.DrawLine(poses[0], poses[1]);// bottom square
            Handles.DrawLine(poses[0], poses[2]);
            Handles.DrawLine(poses[2], poses[3]);
            Handles.DrawLine(poses[1], poses[3]);
            Handles.DrawLine(poses[0], poses[4]);// vertical lines
            Handles.DrawLine(poses[1], poses[5]);
            Handles.DrawLine(poses[2], poses[6]);
            Handles.DrawLine(poses[3], poses[7]);
            Handles.DrawLine(poses[4], poses[5]);// top square
            Handles.DrawLine(poses[4], poses[6]);
            Handles.DrawLine(poses[6], poses[7]);
            Handles.DrawLine(poses[5], poses[7]);
#endif
        }
    }
}