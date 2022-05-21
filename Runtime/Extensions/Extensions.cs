using System.Collections;
using UnityEngine;

namespace Kutil {
    public static class Extentions {
    }
    public static class ColorExtentions {
        public static Color FromHtml(string html) {
            Color color = Color.black;
            ColorUtility.TryParseHtmlString(html, out color);
            return color;
        }
    }
    public static class ObjectExtentions {
        /// <summary>
        /// Destroys Object immediately if is playing otherwise normal destroy
        /// Usage this.DestroySafe(obj);
        /// </summary>
        /// <param name="obj">The Unity Object to destroy</param>
        public static void DestroySafe(this UnityEngine.Object o, UnityEngine.Object obj) {
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                UnityEngine.Object.Destroy(obj);
#if UNITY_EDITOR
            } else {
                UnityEngine.Object.DestroyImmediate(obj);
            }
#endif
        }
    }
    public static class SysObjectExtensions {
        /// <summary>
        /// Puts the object into a new array containing only that element
        /// </summary>
        public static T[] InNewArray<T>(this T t) {
            return new T[] { t };
        }
    }
    public static class BoundsIntExtensions {
        private static bool Intersects(this BoundsInt bounds, BoundsInt other) {
            return AsBounds(bounds).Intersects(AsBounds(other));
        }
        private static Bounds AsBounds(this BoundsInt bounds) {
            return new Bounds(bounds.center, bounds.size);
        }
        public static bool ContainsBounds(this BoundsInt bounds, BoundsInt smaller){
            return bounds.Contains(smaller.min) && bounds.Contains(smaller.max);
        }
    }
    public static class TransformExtensions {
        /// <summary>
        /// Destroy all children GameObjects on this Transform safely
        /// </summary>
        /// <param name="t"></param>
        public static void DestroyAllChildren(this Transform t) {
            for (int i = t.childCount - 1; i >= 0; i--) {
                if (Application.isPlaying) {
                    UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
                } else {
                    UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
                }
            }
        }
    }
    public static class QuaternionExt {
        public static Quaternion ClampRotation(this Quaternion q, Vector3 bounds) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            return q.normalized;
        }
        public static Quaternion ClampRotationX(this Quaternion q, float minDeg, float maxDeg) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minDeg, maxDeg);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q.normalized;
        }
    }
}