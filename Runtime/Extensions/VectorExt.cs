using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public static class Vector3Ext {
        public static Vector3 Floor(this Vector3 vec) {
            return new Vector3(
            Mathf.Floor(vec.x),
            Mathf.Floor(vec.y),
            Mathf.Floor(vec.z));
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
    }
    public static class Vector2IntExt {
        public static int ManhattanDistance(this Vector2Int vec) {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y);
        }
    }
}