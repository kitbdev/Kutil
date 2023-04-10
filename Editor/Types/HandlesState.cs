using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Kutil.Editor {
    // todo? move to runtime namespace and have #if editor blocks? eh if using, already have those blocks
    public static class HandlesExt {
        public static HandlesState SaveState() {
            return new HandlesState();
        }
        public static void ReturnToDefault() {
            new HandlesState().LoadState();
        }
    }

    /// <summary>
    /// Use to automatically restore Handles State. 
    /// Ex: using(new HandlesState()){...}
    /// </summary>
    [System.Serializable]
    public class HandlesState : IDisposable {

        /// <summary>Colors of the handles.</summary>
        public Color color;
        /// <summary>Are handles lit?</summary>
        public bool lighting;
        /// <summary>Matrix for all handle operations.</summary>
        public Matrix4x4 matrix;
        /// <summary>zTest of the handles.</summary>
        public CompareFunction zTest;

        public HandlesState() {
            SaveState();
        }

        public HandlesState(Color color, bool lighting, Matrix4x4 matrix, CompareFunction zTest) {
            this.color = color;
            this.lighting = lighting;
            this.matrix = matrix;
            this.zTest = zTest;
        }

        /// <summary>
        /// Store current Handles state
        /// </summary>
        public void SaveState() {
            color = Handles.color;
            lighting = Handles.lighting;
            matrix = Handles.matrix;
            zTest = Handles.zTest;
        }
        /// <summary>
        /// Load Handles state from saved data
        /// </summary>
        public void LoadState() {
            Handles.color = color;
            Handles.lighting = lighting;
            Handles.matrix = matrix;
            Handles.zTest = zTest;
        }

        public void Dispose() {
            LoadState();
        }
    }
}