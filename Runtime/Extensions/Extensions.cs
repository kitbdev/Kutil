using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        /// Destroys Object immediately if in editor, normal Destroy if playing.
        /// Usage obj.DestroySafe();
        /// </summary>
        /// <param name="obj">The Unity Object to destroy</param>
        public static void DestroySafe(this UnityEngine.Object obj) {
            if (obj == null) return;
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
    public static class RectIntExtensions {
        public static Rect AsRect(this RectInt rectInt) {
            return new Rect(rectInt.position, rectInt.size);
        }
        public static int Area(this RectInt rectInt) {
            return rectInt.width * rectInt.height;
        }
    }
    public static class TransformExtensions {
        /// <summary>
        /// Destroy all children GameObjects on this Transform safely
        /// </summary>
        /// <param name="t"></param>
        public static void DestroyAllChildren(this Transform t) {
            if (Application.isPlaying) {
                for (int i = t.childCount - 1; i >= 0; i--) {
                    UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
                }
            } else {
                for (int i = t.childCount - 1; i >= 0; i--) {
                    UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
                }
            }
        }
        //https://answers.unity.com/questions/361275/cant-convert-bounds-from-world-coordinates-to-loca.html
        public static Bounds TransformBounds(this Transform _transform, Bounds _localBounds) {
            var center = _transform.TransformPoint(_localBounds.center);

            // transform the local extents' axes
            var extents = _localBounds.extents;
            var axisX = _transform.TransformVector(extents.x, 0, 0);
            var axisY = _transform.TransformVector(0, extents.y, 0);
            var axisZ = _transform.TransformVector(0, 0, extents.z);

            // sum their absolute value to get the world extents
            extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
            extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
            extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

            return new Bounds { center = center, extents = extents };
        }
    }
    public static class QuaternionExt {
        public static bool Approximately(this Quaternion a, Quaternion b) {
            return 1f - Mathf.Abs(Quaternion.Dot(a, b)) < Mathf.Epsilon;
        }

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
    public static class MathExt {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            return ((value - fromMin) / (fromMax - fromMin)) * (toMax - toMin) + toMin;
        }
        /// <summary>
        /// Wraps a value between a min and a max.
        /// Ex. angle from -180 to 180
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Wrap(float value, float min, float max) {
            float range = max - min;
            if (range <= Mathf.Epsilon) {
                // range is negative or zero!
                Debug.LogWarning($"Invalid wrap min max - {min},{max} value:{value}");
                return value;
            }
            value = (value - min) % range;
            value += min + (value < 0 ? range : 0);
            return value;
        }
        public static int Wrap(int value, int min, int max) {
            int range = max - min;
            if (range <= Mathf.Epsilon) {
                // range is negative or zero!
                Debug.LogWarning($"Invalid wrap min max - {min},{max} value:{value}");
                return value;
            }
            value = (value - min) % range;
            value += min + (value < 0 ? range : 0);
            return value;
        }

    }
    public static class PhysicsExtensions {
        public static Collider[] OverlapCollider(this Collider col, Vector3 offset, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
            Collider[] overlapColliders;
            if (col is BoxCollider bcol) {
                overlapColliders = Physics.OverlapBox(bcol.center + col.transform.position + offset, Vector3.Scale(bcol.size, bcol.transform.lossyScale) / 2, col.transform.rotation, layerMask, queryTriggerInteraction);
            } else if (col is SphereCollider scol) {
                overlapColliders = Physics.OverlapSphere(scol.center + col.transform.position + offset, scol.radius * col.transform.lossyScale.MinValue(), layerMask, queryTriggerInteraction);
            } else if (col is CapsuleCollider ccol) {
                Vector3 cHeight = ccol.transform.up * (ccol.height / 2 - ccol.radius);
                Vector3 p0 = ccol.center + cHeight + col.transform.position + offset;
                Vector3 p1 = ccol.center - cHeight + col.transform.position + offset;
                overlapColliders = Physics.OverlapCapsule(p0, p1, ccol.radius * col.transform.lossyScale.MinValue(), layerMask, queryTriggerInteraction);
                // Debug.DrawLine(p0, p1, Color.blue, 10, false);
            } else {
                Debug.LogError($"Cannot check collider {col.GetType()} {col.name}!");
                //? do a aabb overlap? then a physics.compute penetration?
                //physics2D has a collider.overlap method
                return null;
            }
            return overlapColliders;
        }
    }
    public static class UIToolkitExtensions {
        public static void SetDisplay(this VisualElement ve, bool shown) {
            ve.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public static string ToStringBetter(this VisualElement ve) {
            if (ve == null) return "null";
            return $"({ve.GetType().Name}){ve.name}";
        }
        public static bool ValidateExists(this VisualElement ve, string errorMsg = null) {
            if (ve == null) {
                if (errorMsg != null) {
                    Debug.LogError(errorMsg);
                } else {
                    Debug.LogError($"{ve.GetType().Name} is null!");
                }
                return false;
            }
            return true;
        }
    }
}