using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kutil {
    public static class PoolExtensions {
        /// <summary>
        /// Warms the pool, making sure it has at least 'amount' of inactive objects
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="amount">number of inactive objects to ensure pool has</param>
        /// <typeparam name="T"></typeparam>
        public static void PoolEnsureCapacity<T>(this ObjectPool<T> pool, int amount = 1) where T : class {
            // makes sure pool has n inactive objects to warm it
            // if (pool == null) return;
            int numToMake = amount - pool.CountInactive;
            List<T> objs = new();
            // need to take out all in order for pool to make more
            for (int i = 0; i < amount; i++) {
                objs.Add(pool.Get());
            }
            foreach (var obj in objs) {
                pool.Release(obj);
            }
        }

        /// <summary>
        /// Defaul Pool Take Action to use on a Pool with a component T.
        /// Enables the gameobject and unparents
        /// </summary>
        public static void DefPoolTakeActionUnparent(Component component) {
            component.gameObject.SetActive(true);
            component.transform.SetParent(null, false);
        }
        /// <summary>
        /// Defaul Pool Take Action to use on a Pool with a component T
        /// Enables the gameobject
        /// </summary>
        public static void DefPoolTakeAction(Component component) {
            component.gameObject.SetActive(true);
        }
        /// <summary>
        /// Defaul Pool Release Action to use on a Pool with a component T
        /// Disables the gameobject
        /// </summary>
        public static void DefPoolReleaseAction(Component component) {
            component.gameObject.SetActive(false);
            // ? parent
        }
        /// <summary>
        /// Defaul Pool Destroy Action to use on a Pool with a component T
        /// destroys the gameobject
        /// </summary>
        public static void DefPoolDestroyAction(Component component) {
            component.gameObject.DestroySafe();
        }
    }
}