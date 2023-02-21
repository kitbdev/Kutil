using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil
{
    public static class BoundsIntExtensions {
        /// <summary>
        /// Converts BoundsInt to Bounds.
        /// </summary>
        public static Bounds AsBounds(this BoundsInt bounds) {
            return new Bounds(bounds.center, bounds.size);
        }
        public static BoundsInt Copy(this BoundsInt bounds) {
            return new BoundsInt(bounds.position, bounds.size);
        }
        /// <summary>
        /// Converts Bounds to BoundsInt. expands volume if necessary
        /// </summary>
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
        public static bool ContainsBounds(this BoundsInt bounds, BoundsInt smaller) {
            return bounds.Contains(smaller.min) && bounds.Contains(smaller.max);
        }
        public static int Volume(this BoundsInt bounds) {
            return bounds.size.x * bounds.size.y * bounds.size.z;
        }
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
        /// Returns the closest point on the box or inside the box.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="point"></param>
        /// <returns></returns>
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
        public static void DrawGizmosBounds(this BoundsInt bounds) {
            Vector3[] poses = bounds.CornerPositions().Select(p => (Vector3)p).ToArray();
            DrawCube(poses);
        }
        public static void DrawGizmosBounds(this BoundsInt bounds, Grid grid) {
            Vector3[] poses = bounds.CornerPositions().Select(p => grid.CellToWorld(p)).ToArray();
            DrawCube(poses);
        }

        public static void DrawCube(Vector3[] poses) {
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

        // copy bounds functionality
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Geometry/Bounds.cs
        // todo rewrite for faster performance?

        /// <summary>
        /// Does another bounding box intersect with this bounding box?
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if intersects</returns>
        public static bool Intersects(this BoundsInt bounds, BoundsInt other) {
            return bounds.AsBounds().Intersects(other.AsBounds());
        }
        /// <summary>
        /// Grows the bounds to include the point.
        /// </summary>
        /// <param name="newPoint"></param>
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
        public static void Expand(this ref BoundsInt bounds, Vector3Int amount) {
            // var b = bounds.AsBounds();
            // b.Expand(amount);
            // var bi = b.AsBoundsInt();
            // bounds.SetMinMax(bi.min, bi.max);
            bounds.size += amount;
        }
    }
}