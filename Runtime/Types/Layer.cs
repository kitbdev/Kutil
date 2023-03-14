using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Represents a Unity Layer
    /// </summary>
    [System.Serializable]
    public class Layer {
        public int layerValue;

        public Layer(int layer) {
            this.layerValue = layer;
        }

        /// <summary>
        /// Is this layer present in the given layermask?
        /// </summary>
        /// <param name="layermask"></param>
        /// <returns></returns>
        public bool IsInLayerMask(int layermask) {
            return (layermask & (1 << layerValue)) > 0;
        }
        /// <summary>
        /// Appends this layer to the given layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public LayerMask AddToLayerMask(LayerMask mask) {
            return mask | GetMask();
        }
        /// <summary>
        /// Turns this layer into a LayerMask
        /// </summary>
        /// <returns></returns>
        public LayerMask GetMask() {
            return 1 << layerValue;
        }

        public void SetLayer(GameObject go) {
            go.layer = layerValue;
        }

        /// <summary>
        /// Set the layer of this GameObject and all of its children.
        /// stops recursing when hits a layer in the ignore layers
        /// set ignoreLayers to false to use default ignoreLayers (UI)
        /// </summary>
        /// <param name="go"></param>
        /// <param name="ignoreLayers"></param>
        public void SetLayerAllChildren(GameObject go, LayerMask? ignoreLayers = null) {
            if (ignoreLayers == null) {
                ignoreLayers = DefaultIgnoreLayers;
            }
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

        // probably dont need to convert
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
        public static LayerMask NoneLayerMask = 0;
        public static LayerMask DefaultLayerMask = Physics.DefaultRaycastLayers;
        public static LayerMask AllLayerMask = Physics.AllLayers;
        public static LayerMask DefaultIgnoreLayers = UILayer.GetMask();
    }
    public static class LayerExt {
        /// <summary>
        /// Sets the layer of the GameObject and the layer of all children recursively, 
        /// skipping children that are in the ignore layers (or a child of an ignored child)
        /// set ignoreLayers to false to use default ignoreLayers (UI)
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
        public static LayerMask AddLayerMasksInclusive(this LayerMask layerMask, LayerMask otherLayerMask) {
            return layerMask | otherLayerMask;
        }
        public static bool HasLayer(this LayerMask layerMask, Layer layer) {
            return layer.IsInLayerMask(layerMask);
        }
        public static LayerMask Inverted(this LayerMask layerMask) {
            return ~layerMask;
        }
    }
}