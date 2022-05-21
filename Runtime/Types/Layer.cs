using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    [System.Serializable]
    public class Layer {
        public int layerValue;

        public Layer(int layer) {
            this.layerValue = layer;
        }
        public bool IsInLayerMask(int layermask) {
            return (layermask & (1 << layerValue)) > 0;
        }
        public LayerMask AddToLayerMask(LayerMask mask) {
            return mask | GetMask();
        }
        public LayerMask GetMask() {
            return 1 << layerValue;
        }

        public void SetLayer(GameObject go) {
            go.layer = layerValue;
        }
        public void SetLayerAllChildren(GameObject go, LayerMask? ignoreLayers = null) {
            // ignore certain layers, like UI
            if (ignoreLayers != null && ((Layer)go.layer).IsInLayerMask((LayerMask)ignoreLayers)) {
                return;
            }
            go.layer = layerValue;
            int childCount = go.transform.childCount;
            for (int i = 0; i < childCount; i++) {
                SetLayerAllChildren(go.transform.GetChild(i).gameObject, ignoreLayers);
            }
        }

        public static Layer NameToLayer(string layerName) => LayerMask.NameToLayer(layerName);
        public static string LayerToName(Layer layer) => LayerMask.LayerToName(layer);

        public static implicit operator int(Layer l) => l.layerValue;
        public static implicit operator Layer(int l) => new Layer(l);

        public static Layer DefaultLayer = 0;
        public static Layer TransparentEffectsLayer = 1;
        public static Layer IgnoreRaycastLayer = 2;
        public static Layer WaterLayer = 4;
        public static Layer UILayer = 5;
        public static LayerMask EmptyLayerMask = 0;
        public static LayerMask DefaultLayerMask = Physics.DefaultRaycastLayers;
        public static LayerMask AllLayerMask = Physics.AllLayers;
    }
    public static class LayerExt {
        /// <summary>
        /// Sets the layer of the GameObject and the layer of all children recursively, 
        /// skipping children that are in the ignore layers (or a child of an ignored child)
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer">Layer to set</param>
        /// <param name="ignoreLayers">ignores children (and all their children) with this layer</param>
        public static void SetLayerAllChildren(this GameObject go, Layer layer, LayerMask? ignoreLayers = null) {
            layer.SetLayerAllChildren(go, ignoreLayers);
        }
        public static LayerMask AddLayer(this LayerMask layerMask, Layer layer) {
            return layer.AddToLayerMask(layerMask);
        }
        public static bool HasLayer(this LayerMask layerMask, Layer layer) {
            return layer.IsInLayerMask(layerMask);
        }
        public static LayerMask Inverted(this LayerMask layerMask) {
            return ~layerMask;
        }
    }
}