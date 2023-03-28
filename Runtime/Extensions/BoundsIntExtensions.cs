using UnityEngine;
// using Unity.Mathematics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kutil {
    public static class BoundsExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds Scale(this Bounds bounds, Vector3 scale) {
            return new Bounds(bounds.center, Vector3.Scale(bounds.size, scale));
        }
        public static Bounds Scale(this Bounds bounds, float scale) {
            return new Bounds(bounds.center, bounds.size * scale);
        }

    }
    public static class BoundsIntExtensions {

        /*
            Note: boundsint is (min-inclusive, max-exclusive) by default
            ex - for regular points boundsint.contains(boundsint.max) returns false
            ex - allPositionsWithin goes from min to (max-Vector3Int.one)
            helps with iterating and size calculations being 0-indexed
        */



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
        /// Converts Bounds to BoundsInt. expands volume if necessary, using floor and ceil
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt AsBoundsInt(this Bounds bounds) {
            BoundsInt boundsInt = new BoundsInt();
            boundsInt.SetMinMax(Vector3Int.FloorToInt(bounds.min), Vector3Int.CeilToInt(bounds.max));
            return boundsInt;
        }
        /// <summary>
        /// Converts Bounds to BoundsInt. rounds min and max points
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundsInt AsBoundsIntRounded(this Bounds bounds) {
            BoundsInt boundsInt = new BoundsInt();
            boundsInt.SetMinMax(Vector3Int.RoundToInt(bounds.min), Vector3Int.RoundToInt(bounds.max));
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
        /// Returns true if this BoundsInt contains another entirely.
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
        // and rectint https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Geometry/RectInt.cs
        // ? rewrite for faster performance

        /// <summary>
        /// Does another bounding box intersect with this bounding box?
        /// includes just touching.
        /// recommended to use Overlaps() by default instead
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if intersects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectsOrTouches(this BoundsInt bounds, BoundsInt other) {
            return other.xMin <= bounds.xMax
                && other.xMax >= bounds.xMin
                && other.yMin <= bounds.yMax
                && other.yMax >= bounds.yMin
                && other.zMin <= bounds.zMax
                && other.zMax >= bounds.zMin;
        }
        /// <summary>
        /// Does this bounding box overlap with another?
        /// does not include just touching, must be some overlap
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if overlaps</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this BoundsInt bounds, BoundsInt other) {
            return other.xMin < bounds.xMax
                && other.xMax > bounds.xMin
                && other.yMin < bounds.yMax
                && other.yMax > bounds.yMin
                && other.zMin < bounds.zMax
                && other.zMax > bounds.zMin;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncapsulateBoundsInt(this ref BoundsInt bounds, BoundsInt other) {
            // bounds.Encapsulate(other.min);
            // bounds.Encapsulate(other.max);
            bounds.SetMinMax(Vector3Int.Min(bounds.min, other.min), Vector3Int.Max(bounds.max, other.max));
        }
        /// <summary>
        /// Grows the bounds to include the point.
        /// </summary>
        /// <param name="newPoint"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Encapsulate(this ref BoundsInt bounds, Vector3Int newPoint) {
            // Debug.Log($"bounds encapsulate o:{bounds} p:{newPoint} min:{Vector3Int.Min(bounds.min, newPoint)}, max:{Vector3Int.Max(bounds.max, newPoint)}");
            bounds.SetMinMax(Vector3Int.Min(bounds.min, newPoint), Vector3Int.Max(bounds.max, newPoint));
            // Debug.Log($"new bounds:{bounds}");
        }
        /// <summary>
        /// Grows the bounds to include the point. 
        /// treats the new point as Inclusive, includes points on the max border
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
        /// Expand the bounds by increasing its size by amount along each axis
        /// </summary>
        /// <param name="amount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand(this ref BoundsInt bounds, int amount) {
            bounds.Expand(new Vector3Int(amount, amount, amount));
        }
        /// <summary>
        /// Expand the bounds by increasing its size by amount along each axis.
        /// only moves the maximum point if size and amount are positive.
        /// if size is negative, amount should also be negative to expand.
        /// </summary>
        /// <param name="amount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand(this ref BoundsInt bounds, Vector3Int amount) {
            bounds.size += amount;
        }
        /// <summary>
        /// Expand the bounds by moving its minimum point by amount.
        /// only moves the minimum point if size and amount are positive.
        /// if size is negative, amount should also be negative to expand.
        /// </summary>
        /// <param name="amount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExpandMin(this ref BoundsInt bounds, Vector3Int amount) {
            bounds.position -= amount;
            bounds.size += amount;
        }

        public static Vector3Int CenterIntFloored(this BoundsInt bounds) => bounds.position + bounds.size / 2;
        public static Vector3Int CenterIntRounded(this BoundsInt bounds) =>
            bounds.position + Vector3Int.RoundToInt((Vector3)bounds.size / 2f);
        public static BoundsInt SetCenterRounded(this BoundsInt bounds, Vector3Int newCenter) {
            // Debug.Log($"center {newCenter} r{Vector3Int.RoundToInt((Vector3)bounds.size / 2f)} p:{bounds.position} s:{bounds.size}");
            bounds.position = newCenter - Vector3Int.RoundToInt((Vector3)bounds.size / 2f);
            //Vector3Int.RoundToInt((Vector3)bounds.size / 2f);
            return bounds;
        }

        /// <summary>
        /// Returns a new bounds moved by amount
        /// </summary>
        /// <param name="amount"></param>
        public static BoundsInt Moved(this BoundsInt bounds, Vector3Int amount) {
            return new BoundsInt(bounds.position + amount, bounds.size);
        }
        /// <summary>
        /// Returns a new bounds with size scaled by scale
        /// </summary>
        /// <param name="scale"></param>
        public static BoundsInt Scaled(this BoundsInt bounds, int scale) => bounds.Scaled(new Vector3Int(scale, scale, scale));
        /// <summary>
        /// Returns a new bounds with size scaled by scale
        /// </summary>
        /// <param name="scale"></param>
        public static BoundsInt Scaled(this BoundsInt bounds, Vector3Int scale) {
            return new BoundsInt(bounds.position, Vector3Int.Scale(bounds.size, scale));
        }
        public static BoundsInt ScaledCentered(this BoundsInt bounds, Vector3Int scale) {
            Vector3Int size = Vector3Int.Scale(bounds.size, scale);
            Vector3Int offset = (size - bounds.size) / 2;
            return new BoundsInt(bounds.position - offset, size);
        }
        public static BoundsInt ScaledCenteredRounded(this BoundsInt bounds, Vector3 scale) {
            Vector3Int size = Vector3Int.RoundToInt(Vector3.Scale(bounds.size, scale));
            Vector3Int offset = (size - bounds.size) / 2;
            return new BoundsInt(bounds.position - offset, size);
        }

        /// <summary>
        /// grows bounds to include max(top,right,frwd) corner.
        /// ! doesnt actually change other boundsint method functionality
        /// </summary>
        /// <returns>new boundsint</returns>
        public static BoundsInt MakeInclusive(this BoundsInt bounds) {
            BoundsInt b = new();
            b.SetMinMax(bounds.min, bounds.max + Vector3Int.one);
            return b;
        }
        /// <summary>
        /// shrinks bounds to exclude max(top,right,frwd) corner
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
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOnBorder(this BoundsInt bounds, Vector3Int point) {
            return bounds.Contains(point) && (
                    point.x == bounds.xMin ||
                    point.x == bounds.xMax - 1 ||
                    point.y == bounds.yMin ||
                    point.y == bounds.yMax - 1 ||
                    point.z == bounds.zMin ||
                    point.z == bounds.zMax - 1);
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

        /// <summary>
        /// Splits the bounds into mutliple based on the given position.
        /// pos acts as a plane on each axis that will split the bounds.
        /// will return 1-8 new boundsInts.
        /// new boundInts will start at an axis of pos
        /// </summary>
        /// <param name="bounds">the bounds to split</param>
        /// <param name="pos">split position per axis. if pos equals min or max, no splitting will occur</param>
        /// <param name="allowx">allow splitting the bounds on the x axis</param>
        /// <param name="allowy">allow splitting the bounds on the y axis</param>
        /// <param name="allowz">allow splitting the bounds on the z axis</param>
        /// <returns></returns>
        public static BoundsInt[] SplitBounds(this BoundsInt bounds, Vector3Int pos, bool allowx = true, bool allowy = true, bool allowz = true) {
            // need to cut when point overlaps in any axis
            // dont have zero length bounds, so dont use pos>=bounds.min
            // x? pos < bounds.max-1 ? probably not, cause using size pos-min and bounds.size is max-min
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
            } else { // if (!cutx && !cuty && !cutz)
                // dont cut at all
                return new BoundsInt[]{
                    new(bounds.min, bounds.size)
                };
            }
        }

        // /// <summary>
        // /// Returns true if bounds can be turned into one without adding new volume
        // /// </summary>
        // /// <param name="bounds"></param>
        // /// <param name="other"></param>
        // /// <returns></returns>
        // public static bool CanMergeBounds(this BoundsInt bounds, BoundsInt other) {
        //     // contain each other entirely
        //     if (bounds.ContainsBounds(other) || other.ContainsBounds(bounds)) return true;
        //     // share 2 axis
        //     // todo faster
        //     // if (bounds.min.x == other.min.x && bounds.max.x == other.max.x
        //     //     && bounds.min.y == other.min.y&& bounds.max.y == other.max.y) return true;
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.XYZ).Equals(other.AsRectInt(GridLayout.CellSwizzle.XYZ))) return true;
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.XZY).Equals(other.AsRectInt(GridLayout.CellSwizzle.XZY))) return true;
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.YZX).Equals(other.AsRectInt(GridLayout.CellSwizzle.YZX))) return true;
        //     return false;
        // }
        // public static BoundsInt MergeBounds(this BoundsInt bounds, BoundsInt other) {
        //     // contain each other entirely
        //     if (bounds.ContainsBounds(other)) return bounds;
        //     if (other.ContainsBounds(bounds)) return other;
        //     // share a rectint
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.XYZ).Equals(other.AsRectInt(GridLayout.CellSwizzle.XYZ))) {
        //         var b = new BoundsInt();
        //         b.SetMinMax(new(bounds.xMin, bounds.yMin, Math.Min(bounds.zMin, other.zMin)),
        //                     new(bounds.xMax, bounds.yMax, Math.Max(bounds.zMax, other.zMax)));
        //         return b;
        //     }
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.XZY).Equals(other.AsRectInt(GridLayout.CellSwizzle.XZY))) {
        //         var b = new BoundsInt();
        //         b.SetMinMax(new(bounds.xMin, Math.Min(bounds.yMin, other.yMin), bounds.zMin),
        //                     new(bounds.xMax, Math.Max(bounds.yMax, other.yMax), bounds.zMax));
        //         return b;
        //     }
        //     if (bounds.AsRectInt(GridLayout.CellSwizzle.YZX).Equals(other.AsRectInt(GridLayout.CellSwizzle.YZX))) {
        //         var b = new BoundsInt();
        //         b.SetMinMax(new(Math.Min(bounds.xMin, other.xMin), bounds.yMin, bounds.zMin),
        //                     new(Math.Max(bounds.xMax, other.xMax), bounds.yMax, bounds.zMax));
        //         return b;
        //     }
        //     throw new System.Exception($"BoundsInt cannot merge {bounds} and {other}");
        //     // return default;
        // }




        /// <summary>
        /// Get the 8 corners for this boundsint
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Vector3Int[] CornerPositions(this BoundsInt bounds, bool exclusive = true) {
            int ofs = exclusive ? 0 : -1;
            return new Vector3Int[]{
                new Vector3Int(bounds.xMin, bounds.yMin, bounds.zMin),
                new Vector3Int(bounds.xMax+ofs, bounds.yMin, bounds.zMin),
                new Vector3Int(bounds.xMin, bounds.yMin, bounds.zMax+ofs),
                new Vector3Int(bounds.xMax+ofs, bounds.yMin, bounds.zMax+ofs),
                new Vector3Int(bounds.xMin, bounds.yMax+ofs, bounds.zMin),
                new Vector3Int(bounds.xMax+ofs, bounds.yMax+ofs, bounds.zMin),
                new Vector3Int(bounds.xMin, bounds.yMax+ofs, bounds.zMax+ofs),
                new Vector3Int(bounds.xMax+ofs, bounds.yMax+ofs, bounds.zMax+ofs),
            };
        }
        public static Vector3[] CornerPositions(this Bounds bounds) {
            return new Vector3[]{
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
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

        public class Vector3YZXComparer : IComparer<Vector3> {
            public int Compare(Vector3 a, Vector3 b) {
                if (a.y != b.y) return (int)(b.y - a.y);
                if (a.z != b.z) return (int)(b.z - a.z);
                if (a.x != b.x) return (int)(b.x - a.x);
                return 0;
            }
        }

        // gizmos stuff

        // todo easy handle maniplulator? to be able to edit a bounds in the scene like a boxcollider

        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawBoundsHandles(this Bounds bounds, Transform transform = null) {
            DrawBounds(bounds, HandlesDrawLine, null, transform);
            // Handles.han
        }


        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawBoundsHandles(this BoundsInt bounds, Vector3 offset, Grid grid = null) {
            DrawBounds(bounds, HandlesDrawLine, offset, grid);
        }
        /// <summary>
        /// draw the bounds using handles
        /// </summary>
        public static void DrawBoundsHandles(this BoundsInt bounds, Transform transform = null, Grid grid = null) {
            DrawBounds(bounds, HandlesDrawLine, transform, grid);
        }


        public static void DrawBounds(this BoundsInt bounds, System.Action<Vector3, Vector3> drawFunc, Vector3 offset, Grid grid = null) {
            Matrix4x4 matrix4x4 = Matrix4x4.Translate(offset);
            DrawBounds(bounds, drawFunc, matrix4x4, null, grid);
        }
        public static void DrawBounds(this BoundsInt bounds, System.Action<Vector3, Vector3> drawFunc, Transform transform = null, Grid grid = null) {
            DrawBounds(bounds, drawFunc, null, transform, grid);
        }

        public static void DrawBounds(this BoundsInt bounds, System.Action<Vector3, Vector3> drawFunc, Matrix4x4? transformMatrix = null, Transform transform = null, Grid grid = null) {
            DrawCube(bounds.CornerPositions(), drawFunc, transformMatrix, transform, grid);
        }
        public static void DrawBounds(this Bounds bounds, System.Action<Vector3, Vector3> drawFunc, Matrix4x4? transformMatrix = null, Transform transform = null) {
            DrawCube(bounds.CornerPositions(), drawFunc, transformMatrix, transform);
        }
        public static void DrawCube(Vector3Int[] points, System.Action<Vector3, Vector3> drawFunc, Matrix4x4? transformMatrix = null, Transform transform = null, Grid grid = null) {
            // apply the grid
            Vector3[] ps;
            if (grid != null) {
                ps = points.Select(p => grid.CellToLocal(p)).ToArray();
            } else {
                ps = points.Select(p => (Vector3)p).ToArray();
            }
            DrawCube(ps, drawFunc, transformMatrix, transform);
        }
        public static void DrawCube(Vector3[] points, System.Action<Vector3, Vector3> drawFunc, Matrix4x4? transformMatrix = null, Transform transform = null) {
            // apply trs matrix or transform
            if (transformMatrix != null) {
                Matrix4x4 t = (Matrix4x4)transformMatrix;
                points = points.Select(p => t.MultiplyPoint3x4(p)).ToArray();
            }
            System.Span<Vector3> poses = new(points);
            if (transform != null) {
                transform.TransformPoints(poses, poses);
            }
            DrawCube(poses.ToArray(), drawFunc);
        }
        public static void HandlesDrawLine(Vector3 start, Vector3 end) {
#if UNITY_EDITOR
            Handles.DrawLine(start, end);
#endif
        }
        public static void GizmosDrawLine(Vector3 start, Vector3 end) => Gizmos.DrawLine(start, end);
        /// <summary>
        /// calls drawFunc on each edge pair of cornerPositions, assuming they are ordered yzx like CornerPositions()
        /// </summary>
        /// <param name="cornerPositions"></param>
        /// <param name="drawFunc"></param>
        public static void DrawCube(Vector3[] cornerPositions, System.Action<Vector3, Vector3> drawFunc = null) {
            if (cornerPositions == null || cornerPositions.Length != 8) {
                Debug.LogError($"DrawCube needs 8 positions to draw has {(cornerPositions?.Length.ToString() ?? "null")}");
                return;
            }
            if (drawFunc == null) {
                drawFunc = GizmosDrawLine;
            }
            foreach (var (e1, e2) in BoundsIntExtensions.edgeIndexes) {
                Vector3 start = cornerPositions[e1];
                Vector3 end = cornerPositions[e2];
                drawFunc(start, end);
            }
        }

        /// <summary>Draw outline of a cube from 8 positions</summary>
        public static void HandlesDrawCube(Vector3[] cornerPositions) {
#if UNITY_EDITOR
            if (cornerPositions == null || cornerPositions.Length != 8) {
                Debug.LogError($"DrawCube needs 8 positions to draw has {(cornerPositions?.Length.ToString() ?? "null")}");
                return;
            }
            Handles.DrawLine(cornerPositions[0], cornerPositions[1]);// bottom square
            Handles.DrawLine(cornerPositions[0], cornerPositions[2]);
            Handles.DrawLine(cornerPositions[2], cornerPositions[3]);
            Handles.DrawLine(cornerPositions[1], cornerPositions[3]);
            Handles.DrawLine(cornerPositions[0], cornerPositions[4]);// vertical lines
            Handles.DrawLine(cornerPositions[1], cornerPositions[5]);
            Handles.DrawLine(cornerPositions[2], cornerPositions[6]);
            Handles.DrawLine(cornerPositions[3], cornerPositions[7]);
            Handles.DrawLine(cornerPositions[4], cornerPositions[5]);// top square
            Handles.DrawLine(cornerPositions[4], cornerPositions[6]);
            Handles.DrawLine(cornerPositions[6], cornerPositions[7]);
            Handles.DrawLine(cornerPositions[5], cornerPositions[7]);
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