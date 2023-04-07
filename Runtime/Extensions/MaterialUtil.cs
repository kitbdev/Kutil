using UnityEngine;

namespace Kutil {
    public static class MaterialUtil {
        public static Material GetDefaultMaterial() {
            // Material.GetDefaultMaterial() is not public
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mat = cube.GetComponent<MeshRenderer>().sharedMaterial;
            GameObject.DestroyImmediate(cube);
            return mat;
        }
    }
}