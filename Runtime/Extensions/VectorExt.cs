using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
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
        public static float MinValue(this Vector3 vec) {
            return Mathf.Min(vec.x, vec.y, vec.z);
        }
        public static float MaxValue(this Vector3 vec) {
            return Mathf.Max(vec.x, vec.y, vec.z);
        }

    }
    public static class Vector3IntExt {
        public static Vector3Int Mul(this Vector3Int vec, int val) {
            return new Vector3Int(
                vec.x * val,
                vec.y * val,
                vec.z * val
            );
        }
        public static Vector3Int Abs(this Vector3Int vec) {
            return new Vector3Int(
                Mathf.Abs(vec.x),
                Mathf.Abs(vec.y),
                Mathf.Abs(vec.z)
            );
        }
        public static int ManhattanDistance(this Vector3Int vec) {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z);
        }

        public static Vector3Int RotateAround(this Vector3Int vec, Vector3Int newForward, Vector3Int point) {
            return (vec - point).Rotate(newForward) + point;
        }
        public static Vector3Int Rotate(this Vector3Int vec, Vector3Int newForward) {
            if (newForward == Vector3Int.forward) {
                return vec;
            } else if (newForward == Vector3Int.zero) {
                return vec;
            } else if (newForward == Vector3Int.back) {
                return new Vector3Int(-vec.x, vec.y, -vec.z);
            } else if (newForward == Vector3Int.right) {
                return new Vector3Int(vec.z, vec.y, -vec.x);
            } else if (newForward == Vector3Int.left) {
                return new Vector3Int(-vec.z, vec.y, vec.x);
            } else if (newForward == Vector3Int.up) {
                return new Vector3Int(vec.x, vec.z, -vec.z);
            } else if (newForward == Vector3Int.down) {
                return new Vector3Int(vec.x, -vec.z, vec.z);
            } else {
                Debug.LogError($"Invalid rotation dir {newForward} for {vec}");
                return default;
            }
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