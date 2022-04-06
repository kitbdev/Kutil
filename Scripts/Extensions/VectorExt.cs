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
    }
}