using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    public static class BoundsIntExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt Create(Vector3Int min, Vector3Int max) {
            BoundsInt b = new();
            b.SetMinMax(min, max);
            return b;
        }

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
        /// <param name="swizzle">Will drop the third swizzle axis. ex: XYZ drops the Z axis</param>
        public static Rect AsRect(this Bounds bounds, GridLayout.CellSwizzle swizzle = default) {
            return new Rect((Vector2)Grid.Swizzle(swizzle, bounds.min), (Vector2)Grid.Swizzle(swizzle, bounds.size));
        }
        /// <summary>
        /// Flattens into a RectInt. Removes the Z axis. Can Swizzle for different axis.
        /// </summary>
        /// <param name="swizzle">Will drop the third swizzle axis. ex: XYZ drops the Z axis</param>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectsAndOverlaps(this BoundsInt bounds, BoundsInt other) {
            // return bounds.AsBounds().Intersects(other.AsBounds());
            return (bounds.min.x < other.max.x) && (bounds.max.x > other.min.x) &&
                (bounds.min.y < other.max.y) && (bounds.max.y > other.min.y) &&
                (bounds.min.z < other.max.z) && (bounds.max.z > other.min.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncapsulateBoundsInt(this ref BoundsInt bounds, BoundsInt other) {
            bounds.Encapsulate(other.min);
            bounds.Encapsulate(other.max);
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

        /// <summary>
        /// Get the BoundsInt that intersects these two boundsints. assumes they do intersect.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt GetIntersection(this BoundsInt bounds, BoundsInt other) {
            BoundsInt intersection = new();
            intersection.SetMinMax(Vector3Int.Max(bounds.min, other.min), Vector3Int.Min(bounds.max, other.max));
            return intersection;
        }


        public static BoundsInt[] SplitBounds(BoundsInt bounds, Vector3Int pos, bool allowx = true, bool allowy = true, bool allowz = true) {
            // todo > or >= ??
            bool cutx = pos.x > bounds.xMin && pos.x < bounds.xMax && allowx;
            bool cuty = pos.y > bounds.yMin && pos.y < bounds.yMax && allowy;
            bool cutz = pos.z > bounds.zMin && pos.z < bounds.zMax && allowz;
            // Debug.Log($"cutting {bounds} on {pos} allow:{allowx},{allowy},{allowz} cut:{cutx},{cuty},{cutz}");

            Vector3Int size = (pos - bounds.min);
            if (!cutx) {
                size.x = bounds.size.x;
            }
            if (!cuty) {
                size.y = bounds.size.y;
            }
            if (!cutz) {
                size.z = bounds.size.z;
            }
            Vector3Int isize = bounds.size - size;

            if (cutx && cuty && cutz) {
                // cut into 8
                return new BoundsInt[]{
                    new(bounds.min, size),
                    new(new(pos.x,        bounds.min.y, bounds.min.z), new(isize.x, size.y,  size.z)),
                    new(new(bounds.min.x, pos.y,        bounds.min.z), new(size.x,  isize.y, size.z)),
                    new(new(pos.x,        pos.y,        bounds.min.z), new(isize.x, isize.y, size.z)),
                    new(new(bounds.min.x, bounds.min.y, pos.z),        new(size.x,  size.y,  isize.z)),
                    new(new(pos.x,        bounds.min.y, pos.z),        new(isize.x, size.y,  isize.z)),
                    new(new(bounds.min.x, pos.y,        pos.z),        new(size.x,  isize.y, isize.z)),
                    new(pos, isize),
                };
            } else if (cutx && cuty && !cutz) {
                // cut on x and y into 4
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(pos.x, bounds.min.y, bounds.min.z), new(isize.x, size.y, size.z)),
                    new(new(bounds.min.x, pos.y, bounds.min.z), new(size.x, isize.y, size.z)),
                    new(new(pos.x, pos.y, bounds.min.z), new(isize.x, isize.y, size.z)),
                };
            } else if (cutx && !cuty && cutz) {
                // cut on x and z into 4
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(pos.x, bounds.min.y, bounds.min.z), new(isize.x, size.y, size.z)),
                    new(new(bounds.min.x, bounds.min.y, pos.z), new(size.x, size.y, isize.z)),
                    new(new(pos.x, bounds.min.y, pos.z), new(isize.x, size.y, isize.z)),
                };
            } else if (cutx && !cuty && !cutz) {
                // cut only x
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(pos.x, bounds.min.y, bounds.min.z), new(isize.x, size.y, size.z)),
                };
            } else if (!cutx && cuty && cutz) {
                // cut on y and z into 4
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(bounds.min.x, pos.y, bounds.min.z), new(size.x, isize.y, size.z)),
                    new(new(bounds.min.x, bounds.min.y, pos.z), new(size.x, size.y, isize.z)),
                    new(new(bounds.min.x, pos.y, pos.z), new(size.x, isize.y, isize.z)),
                };
            } else if (!cutx && cuty && !cutz) {
                // cut only y
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(bounds.min.x, pos.y, bounds.min.z), new(size.x, isize.y, size.z)),
                };
            } else if (!cutx && !cuty && cutz) {
                // cut only z
                return new BoundsInt[]{
                    new(new(bounds.min.x, bounds.min.y, bounds.min.z), new(size.x, size.y, size.z)),
                    new(new(bounds.min.x, bounds.min.y, pos.z), new(size.x, size.y, isize.z)),
                };
            } else if (!cutx && !cuty && !cutz) {
                // dont cut at all
                return new BoundsInt[]{
                    new(bounds.min, bounds.size)
                };
            }
            return null;
        }
        // public static IEnumerable<BoundsInt> SplitBoundsR(BoundsInt bounds, Vector3Int pos, bool allowx = true, bool allowy = true, bool allowz = true) {
        //     // todo test this please
        //     // todo > or >= ??
        //     bool cutx = pos.x > bounds.xMin && pos.x < bounds.xMax && allowx;
        //     bool cuty = pos.y > bounds.yMin && pos.y < bounds.yMax && allowy;
        //     bool cutz = pos.z > bounds.zMin && pos.z < bounds.zMax && allowz;
        //     Debug.Log($"rcutting {bounds} on {pos} allow:{allowx},{allowy},{allowz} cut:{cutx},{cuty},{cutz}");
        //     if (cutx) {
        //         int cutxsize1 = pos.x - bounds.xMin;
        //         int cutxsize2 = bounds.xMax - pos.x;
        //         var b1 = new BoundsInt(bounds.min.x, bounds.min.y, bounds.min.z, cutxsize1, bounds.size.y, bounds.size.z);
        //         var b2 = new BoundsInt(cutxsize1, bounds.min.y, bounds.min.z, cutxsize2, bounds.size.y, bounds.size.z);
        // // ! this doesnt remove old ones that get split
        //         IEnumerable<BoundsInt> boundsr = SplitBoundsR(b1, pos, false, allowy, allowz);
        //         boundsr = boundsr.AppendRange(SplitBoundsR(b2, pos, false, allowy, allowz));
        //         return boundsr;
        //     }
        //     if (cuty) {
        //         int cutysize1 = pos.y - bounds.yMin;
        //         int cutysize2 = bounds.yMax - pos.y;
        //         var b1 = new BoundsInt(bounds.min.y, bounds.min.y, bounds.min.z, bounds.size.x, cutysize1, bounds.size.z);
        //         var b2 = new BoundsInt(bounds.min.x, cutysize1, bounds.min.z, bounds.size.x, cutysize2, bounds.size.z);
        //         IEnumerable<BoundsInt> boundsr = SplitBoundsR(b1, pos, allowx, false, allowz);
        //         boundsr = boundsr.AppendRange(SplitBoundsR(b2, pos, allowx, false, allowz));
        //         return boundsr;
        //     }
        //     if (cutz) {
        //         int cutzsize1 = pos.z - bounds.zMin;
        //         int cutzsize2 = bounds.zMax - pos.z;
        //         var b1 = new BoundsInt(bounds.min.x, bounds.min.y, bounds.min.z, bounds.size.x, bounds.size.y, cutzsize1);
        //         var b2 = new BoundsInt(bounds.min.x, bounds.min.y, cutzsize1, bounds.size.x, bounds.size.y, cutzsize2);
        //         IEnumerable<BoundsInt> boundsr = SplitBoundsR(b1, pos, allowx, allowy, false);
        //         boundsr = boundsr.AppendRange(SplitBoundsR(b2, pos, allowx, allowy, false));
        //         return boundsr;
        //     }
        //     return bounds.InNewArray();
        // }


        /// <summary>
        /// Returns true if bounds can be turned into one without adding new volume
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CanMergeBounds(this BoundsInt bounds, BoundsInt other) {
            // contain each other entirely
            if (bounds.ContainsBounds(other) || other.ContainsBounds(bounds)) return true;
            // share 2 axis
            if (bounds.AsRectInt(GridLayout.CellSwizzle.XYZ).Equals(other.AsRectInt(GridLayout.CellSwizzle.XYZ))) return true;
            if (bounds.AsRectInt(GridLayout.CellSwizzle.XZY).Equals(other.AsRectInt(GridLayout.CellSwizzle.XZY))) return true;
            if (bounds.AsRectInt(GridLayout.CellSwizzle.YZX).Equals(other.AsRectInt(GridLayout.CellSwizzle.YZX))) return true;

            return false;
        }
        public static BoundsInt MergeBounds(this BoundsInt bounds, BoundsInt other) {
            // contain each other entirely
            if (bounds.ContainsBounds(other)) return bounds;
            if (other.ContainsBounds(bounds)) return other;
            // share a rectint
            if (bounds.AsRectInt(GridLayout.CellSwizzle.XYZ).Equals(other.AsRectInt(GridLayout.CellSwizzle.XYZ))) {
                var b = new BoundsInt();
                b.SetMinMax(new(bounds.xMin, bounds.yMin, Mathf.Min(bounds.zMin, other.zMin)),
                            new(bounds.xMax, bounds.yMax, Mathf.Max(bounds.zMax, other.zMax)));
                return b;
            }
            if (bounds.AsRectInt(GridLayout.CellSwizzle.XZY).Equals(other.AsRectInt(GridLayout.CellSwizzle.XZY))) {
                var b = new BoundsInt();
                b.SetMinMax(new(bounds.xMin, Mathf.Min(bounds.yMin, other.yMin), bounds.zMin),
                            new(bounds.xMax, Mathf.Max(bounds.yMax, other.yMax), bounds.zMax));
                return b;
            }
            if (bounds.AsRectInt(GridLayout.CellSwizzle.YZX).Equals(other.AsRectInt(GridLayout.CellSwizzle.YZX))) {
                var b = new BoundsInt();
                b.SetMinMax(new(Mathf.Min(bounds.xMin, other.xMin), bounds.yMin, bounds.zMin),
                            new(Mathf.Max(bounds.xMax, other.xMax), bounds.yMax, bounds.zMax));
                return b;
            }
            throw new System.Exception($"BoundsInt cannot merge {bounds} and {other}");
            // return default;
        }


        /// <summary>
        /// Get the 8 corners for this boundsint
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
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
        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawGizmosBounds(this BoundsInt bounds, Transform transform, Grid grid) {
            Vector3Int[] points = bounds.CornerPositions();
            var ps = points.Select(p => grid.CellToLocal(p)).ToArray();
            System.Span<Vector3> poses = new(ps);
            transform.TransformPoints(poses, poses);
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

        /// <summary>
        /// Simple bounds int comparer - position XYZ, size XYZ 
        /// </summary>
        public class BoundsIntComparer : IComparer<BoundsInt> {
            public int Compare(BoundsInt x, BoundsInt y) {
                if (x.position.x != y.position.x) return y.position.x - x.position.x;
                if (x.position.y != y.position.y) return y.position.y - x.position.y;
                if (x.position.z != y.position.z) return y.position.z - x.position.z;
                if (x.size.x != y.size.x) return y.size.x - x.size.x;
                if (x.size.y != y.size.y) return y.size.y - x.size.y;
                if (x.size.z != y.size.z) return y.size.z - x.size.z;
                return 0;
            }
        }
    }
}