using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public static class DebugExt {
        public static void DrawCircle(Vector3 center, Vector3 forward, Vector3 up, float radius, Color color, float duration = 0, int res = 20) {
            DrawEllipse(center, forward, up, radius, radius, res, color, duration);
        }
        //https://forum.unity.com/threads/solved-debug-drawline-circle-ellipse-and-rotate-locally-with-offset.331397/
        public static void DrawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0) {
            float angle = 0f;
            Quaternion rot = Quaternion.LookRotation(forward, up);
            Vector3 lastPoint = Vector3.zero;
            Vector3 thisPoint = Vector3.zero;

            for (int i = 0; i < segments + 1; i++) {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0) {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }
        public static void DrawBounds(Bounds bounds, Color color, Transform t = null, float scale = 1, float duration = 0, bool depthTest = true) {
            bounds.DrawBounds((a, b) => Debug.DrawLine(a, b, color, duration, depthTest), 
                            Matrix4x4.Scale(scale * Vector3.one), t);
            // Vector3[] corners = bounds.CornerPositions();
            // foreach (var (e1, e2) in BoundsIntExtensions.edgeIndexes) {
            //     Vector3 start = corners[e1];
            //     Vector3 end = corners[e2];
            //     if (t != null) {
            //         start = t.InverseTransformPoint(start);
            //         end = t.InverseTransformPoint(end);
            //     }
            //     Debug.DrawLine(start, end, color, duration, depthTest);
            // }
        }
    }
}