using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kutil {
    public enum Axis {
        x = 0, y = 1, z = 2,
        Right = 0, Up = 1, Forward = 2,
    }
    /*
    ?
    //
        // Summary:
        //     A flag enumeration for specifying which axes on a PrimitiveBoundsHandle object
        //     should be enabled.
        [Flags]
        public enum Axes {
            //
            // Summary:
            //     No axes.
            None = 0,
            //
            // Summary:
            //     X-axis (bit 0).
            X = 1,
            //
            // Summary:
            //     Y-axis (bit 1).
            Y = 2,
            //
            // Summary:
            //     Z-axis (bit 2).
            Z = 4,
            //
            // Summary:
            //     All axes.
            All = 7
        }
    */
    public static class Vector3Ext {

        /// <summary>
        /// Projects a direction vector onto a plane. 
        /// doesnt normalize direction like Vector3.ProjectVectorOnPlane does 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
            return (direction - normal * Vector3.Dot(direction, normal)).normalized;
        }

        public static Vector3 Floor(this Vector3 vec) {
            return new Vector3(
                Mathf.Floor(vec.x),
                Mathf.Floor(vec.y),
                Mathf.Floor(vec.z)
            );
        }
        public static Vector3 Abs(this Vector3 vec) {
            return new Vector3(
                Mathf.Abs(vec.x),
                Mathf.Abs(vec.y),
                Mathf.Abs(vec.z)
            );
        }
        /// <summary>Divides each component by given vector</summary>
        public static Vector3 Div(this Vector3 vec, Vector3 other) {
            return new Vector3(
                vec.x / other.x,
                vec.y / other.y,
                vec.z / other.z
            );
        }
        public static Vector3 Mod(this Vector3 vec, Vector3 other) {
            return new Vector3(
                vec.x % other.x,
                vec.y % other.y,
                vec.z % other.z
            );
        }
        public static Vector3 Scaled(this Vector3 vec, Vector3 other) {
            return new Vector3(
                vec.x * other.x,
                vec.y * other.y,
                vec.z * other.z
            );
        }
        public static float MinValue(this Vector3 vec) {
            return Mathf.Min(vec.x, vec.y, vec.z);
        }
        public static float MaxValue(this Vector3 vec) {
            return Mathf.Max(vec.x, vec.y, vec.z);
        }

        /// <summary>Inverts the vector. returns 1 / vector</summary>
        public static Vector3 InvertScale(this Vector3 scaleVector) {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];
            return scaleVector;
        }

        /// <summary>returns the corresponding axis</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetAxis(this Vector3 vec, Axis axis) {
            return vec[(int)axis];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetAxisSwizzle(this Vector3 vec, Axis axis, Axis secondAxis) {
            return new float2(vec.GetAxis(axis), vec.GetAxis(secondAxis));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetAxisSwizzle(this Vector3 vec, Axis axis, Axis secondAxis, Axis thirdAxis) {
            return new float3(vec.GetAxis(axis), vec.GetAxis(secondAxis), vec.GetAxis(thirdAxis));
        }

        /// <summary>Get vector with just the given Axis</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetAxisVector(this Vector3 vec, Axis axis) {
            switch (axis) {
                case Axis.x: return new(vec.x, 0, 0);
                case Axis.y: return new(0, vec.y, 0);
                case Axis.z: return new(0, 0, vec.z);
            }
            return default;
        }
        /// <summary>Get unit Vector for the Axis in the positive direction</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetAxisVector(Axis axis) {
            switch (axis) {
                case Axis.x: return Vector3.right;
                case Axis.y: return Vector3.up;
                case Axis.z: return Vector3.forward;
            }
            return default;
        }
        public static (Axis, Axis) GetOtherAxis(Axis axis) {
            switch (axis) {
                case Axis.x: return (Axis.y, Axis.z);
                case Axis.y: return (Axis.x, Axis.z);
                case Axis.z: return (Axis.x, Axis.y);
            }
            return default;
        }

    }
    public static class Vector3IntExt {
        /// <summary>Creates a new Vector3Int with this integer as x, y, and z</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVector3Int(this int i) {
            return new Vector3Int(i, i, i);
        }

        public static Vector3Int Mul(this Vector3Int vec, int val) {
            return new Vector3Int(
                vec.x * val,
                vec.y * val,
                vec.z * val
            );
        }
        public static Vector3Int Div(this Vector3Int vec, int val) {
            return new Vector3Int(
                vec.x / val,
                vec.y / val,
                vec.z / val
            );
        }
        /// <summary>divides this vector by another component wise</summary>
        public static Vector3Int Div(this Vector3Int vec, Vector3Int val) {
            return new Vector3Int(
                vec.x / val.x,
                vec.y / val.y,
                vec.z / val.z
            );
        }
        public static Vector3Int Abs(this Vector3Int vec) {
            return new Vector3Int(
                Mathf.Abs(vec.x),
                Mathf.Abs(vec.y),
                Mathf.Abs(vec.z)
            );
        }

        public static int Volume(this Vector3Int vec) {
            return vec.x * vec.y * vec.z;
        }

        // public static Vector3Int Min(params Vector3Int[] vecs) {
        //     return new Vector3Int(
        //         Mathf.Min(vecs.Select(vecs => vecs.x).ToArray()),
        //         Mathf.Min(vecs.Select(vecs => vecs.y).ToArray()),
        //         Mathf.Min(vecs.Select(vecs => vecs.z).ToArray())
        //     );
        // }

        /// <summary>returns the corresponding axis</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAxis(this Vector3Int vec, Axis axis) {
            return vec[(int)axis];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetAxisSwizzle(this Vector3Int vec, Axis axis, Axis secondAxis) {
            return new int2(vec.GetAxis(axis), vec.GetAxis(secondAxis));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int3 GetAxisSwizzle(this Vector3Int vec, Axis axis, Axis secondAxis, Axis thirdAxis) {
        //     return new int3(vec.GetAxis(axis), vec.GetAxis(secondAxis), vec.GetAxis(thirdAxis));
        // }
        public static Vector3Int Swizzle(this Vector3Int vec, Axis axis, Axis secondAxis, Axis thirdAxis) {
            return new Vector3Int(vec.GetAxis(axis), vec.GetAxis(secondAxis), vec.GetAxis(thirdAxis));
        }
        // public static Vector3Int Swizzle(this Vector3Int vec, GridLayout.CellSwizzle cellSwizzle) {
        //     return new Vector3Int(vec.GetAxis(axis), vec.GetAxis(secondAxis), vec.GetAxis(thirdAxis));
        // }
        /// <summary>Get vector with just the given Axis</summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetAxisVector(this Vector3Int vec, Axis axis) {
            switch (axis) {
                case Axis.x: return new(vec.x, 0, 0);
                case Axis.y: return new(0, vec.y, 0);
                case Axis.z: return new(0, 0, vec.z);
            }
            return default;
        }
        /// <summary>Get unit Vector for the Axis in the positive direction</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GetAxisUnitVector(Axis axis) {
            switch (axis) {
                case Axis.x: return Vector3Int.right;
                case Axis.y: return Vector3Int.up;
                case Axis.z: return Vector3Int.forward;
            }
            return default;
        }

        public static int ManhattanDistance(this Vector3Int vec) {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z);
        }

        public static Vector3Int TranslateAround(this Vector3Int vec, Vector3Int newForward, Vector3Int point) {
            return (vec - point).TranslateDir(newForward) + point;
        }
        /// <summary>
        /// transform.TranslateDirection but for Vector3Int
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="forwardDir"></param>
        /// <returns></returns>
        public static Vector3Int TranslateDir(this Vector3Int vec, Vector3Int forwardDir) {
            if (forwardDir == Vector3Int.forward) {
                return vec;
            } else if (forwardDir == Vector3Int.zero) {
                return vec;
            } else if (forwardDir == Vector3Int.back) {
                return new Vector3Int(-vec.x, vec.y, -vec.z);
            } else if (forwardDir == Vector3Int.right) {
                return new Vector3Int(vec.z, vec.y, -vec.x);
            } else if (forwardDir == Vector3Int.left) {
                return new Vector3Int(-vec.z, vec.y, vec.x);
            } else if (forwardDir == Vector3Int.up) {
                return new Vector3Int(vec.x, vec.z, -vec.y);
            } else if (forwardDir == Vector3Int.down) {
                return new Vector3Int(vec.x, -vec.z, vec.y);
            } else {
                Debug.LogError($"Invalid rotation dir {forwardDir} for {vec}");
                return default;
            }
        }
        /// <summary>
        /// Rotate a vector around an axis.
        /// Positve for clockwise.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="axis"></param>
        /// <param name="positive"></param>
        /// <returns></returns>
        public static Vector3Int RotateUnitDir(this Vector3Int vec, Axis axis, bool positive) {
            int fMult = positive ? 1 : -1;
            int sMult = positive ? -1 : 1;
            switch (axis) {
                case Axis.x: return new Vector3Int(vec.x, fMult * vec.z, sMult * vec.y);
                case Axis.y: return new Vector3Int(fMult * vec.z, vec.y, sMult * vec.x);
                case Axis.z: return new Vector3Int(fMult * vec.y, sMult * vec.x, vec.z);
            }
            return default;
        }
        // public static Vector3Int RotateAround(this Vector3Int vec, int ninetyTurns, Vector3Int point) {
        //     return (vec - point).Rotate(ninetyTurns);
        // }
        // public static Vector3Int Rotate(this Vector3Int vec, int ninetyTurns) {
        //     if (ninetyTurns <= -4) {
        //         ninetyTurns = Mathf.Abs(ninetyTurns) + 2;
        //     }
        //     if (ninetyTurns >= 4) ninetyTurns %= 4;
        //     if (ninetyTurns == 0) {
        //         return vec;
        //     }else if (ninetyTurns==1){
        //         // rotate 90 degrees clockwise
        //     }else if (ninetyTurns==2){
        //         // rotate 180 degrees clockwise

        //     }else if (ninetyTurns==3){

        //     }else{
        //         Debug.LogError($"Invalid rotation amount {ninetyTurns} for {vec}");
        //         return default;
        //     }
        // }
    }
    public static class Vector2IntExt {
        public static int ManhattanDistance(this Vector2Int vec) {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y);
        }
    }
}