using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Not necessary for ObjectPool, just a helper in case other scripts want to remove
    /// </summary>
    public class GameObjectPoolObject : MonoBehaviour {

        GameObjectPool pool;
        int typeId;
        MultiGameObjectPool multiObjectPool;

        public int TypeId => typeId;
        public void Init(GameObjectPool pool) {
            this.pool = pool;
        }
        public void Init(MultiGameObjectPool pool, int typeId) {
            this.multiObjectPool = pool;
            this.typeId = typeId;
        }
        [ContextMenu("Recycle From Pool")]
        public void RecycleFromPool() {
            if (pool != null) {
                pool.Recycle(gameObject);
            } else if (multiObjectPool != null) {
                multiObjectPool.Recycle(typeId, gameObject);
            }
        }
    }
}