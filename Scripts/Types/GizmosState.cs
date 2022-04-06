using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public static class GizmosExt {
        public static GizmosState SaveState() {
            return new GizmosState();
        }
    }

    /// <summary>
    /// Use to automatically restore Gizmos State. 
    /// Ex: using(new GizmosState()){...}
    /// </summary>
    [System.Serializable]
    public class GizmosState : IDisposable {
        /// <summary>Sets the color for the gizmos that will be drawn next.</summary>
        public Color color = Color.white;
        /// <summary>Sets the Matrix4x4 that the Unity Editor uses to draw Gizmos.</summary>
        public Matrix4x4 matrix = Matrix4x4.identity;
        /// <summary>Set a texture that contains the exposure correction for LightProbe gizmos. The value is sampled from the red channel in the middle of the texture.</summary>
        public Texture exposure;

        public GizmosState() {
            SaveState();
        }

        public GizmosState(Color color, Matrix4x4 matrix, Texture exposure) {
            this.color = color;
            this.matrix = matrix;
            this.exposure = exposure;
        }

        /// <summary>
        /// Store current Gizmos state
        /// </summary>
        public void SaveState() {
            color = Gizmos.color;
            matrix = Gizmos.matrix;
            exposure = Gizmos.exposure;
        }
        /// <summary>
        /// Load Gizmos state from saved data
        /// </summary>
        public void LoadState() {
            Gizmos.color = color;
            Gizmos.matrix = matrix;
            Gizmos.exposure = exposure;
        }

        public void Dispose() {
            LoadState();
        }
    }
}