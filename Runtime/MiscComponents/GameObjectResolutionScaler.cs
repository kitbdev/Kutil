using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// this is a kind of hacky way to get gameobjects to show properly on UI
    /// </summary>
    public class GameObjectResolutionScaler : MonoBehaviour {

        public Canvas canvas;
        public float resScale = 1f / 1000;
        private Vector2 resolution;
        private Vector3 initialScale;

        private void Awake() {
            resolution = new Vector2(Screen.width, Screen.height);
            initialScale = transform.localScale;
        }
        private void Start() {
            float cScale = canvas?.scaleFactor ?? 1f;
            transform.localScale = initialScale * Mathf.Max(resolution.x, resolution.y) * resScale * cScale;
        }

        private void Update() {
            if (resolution.x != Screen.width || resolution.y != Screen.height) {
                resolution.x = Screen.width;
                resolution.y = Screen.height;
                float cScale = canvas?.scaleFactor ?? 1f;
                transform.localScale = initialScale * Mathf.Max(resolution.x, resolution.y) * resScale * cScale;
                // transform.localScale = initialScale * resScale * cScale;
            }
        }
    }
}