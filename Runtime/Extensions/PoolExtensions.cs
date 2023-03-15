using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kutil {
    public static class PoolExtensions {
        /// <summary>
        /// Warms the pool, Ensure there are at least 'amount' of inactive objects in the pool
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="amount">number of inactive objects to ensure pool has</param>
        /// <typeparam name="T"></typeparam>
        public static void Warm<T>(this IObjectPool<T> pool, int amount = 1) where T : class {
            // if (pool == null) return;
            // int numToMake = amount - pool.CountInactive;
            if (pool.CountInactive >= amount) {
                return;
            }
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
        /// Shrink the pool inactive size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        // public static void Shrink<T>(this IObjectPool<T> pool, int amount) where T: class {
        //     //?
        // }

        /// <summary>
        /// Defaul Pool Get Action to use on a Pool with a component T.
        /// Enables the gameobject and unparents
        /// </summary>
        public static void DefPoolGetActionUnparent(Component component) {
            component.gameObject.SetActive(true);
            component.transform.SetParent(null, false);
        }
        /// <summary>
        /// Defaul Pool Get Action to use on a Pool with a component T
        /// Enables the gameobject
        /// </summary>
        public static void DefPoolGetAction(Component component) {
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