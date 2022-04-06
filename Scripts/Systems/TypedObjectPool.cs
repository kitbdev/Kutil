using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kutil {
    public class TypedObjectPool<TBase> : IDisposable {
        public struct TypedPooledObject<T> : IDisposable {
            public T poolObject;

            public TypedPooledObject(T poolObject) {
                this.poolObject = poolObject;
            }

            public void Dispose() {
                if (poolObject is IDisposable d) {
                    d.Dispose();
                }
                // throw new NotImplementedException();
                // new ObjectPool<Layer>(() => default).Get(out var a);
            }
        }

        public int CountInactive(Type type) => poolDict.ContainsKey(type) ? poolDict[type].Count : 0;
        public int CountActive(Type type) => inUseDict.ContainsKey(type) ? inUseDict[type].Count : 0;
        public int CountAll(Type type) => CountActive(type) + CountInactive(type);

        // IObjectPool objectPool;
        // Dictionary<Type, ObjectPool<TBase>> dict;
        Action<TBase> actionOnCreate;
        Action<TBase> actionOnGet;
        Action<TBase> actionOnRelease;
        Action<TBase> actionOnDestroy;
        bool collectionCheck;
        int defaultCapacity;
        int maxSize;
        Dictionary<Type, Stack<TypedPooledObject<TBase>>> poolDict;
        Dictionary<Type, List<TBase>> inUseDict;

        public TypedObjectPool(Action<TBase> actionOnCreate = null, Action<TBase> actionOnGet = null, Action<TBase> actionOnRelease = null, Action<TBase> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000, params Type[] typesToPopulate) {
            this.actionOnCreate = actionOnCreate;
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
            this.collectionCheck = collectionCheck;
            this.defaultCapacity = defaultCapacity;
            this.maxSize = maxSize;
            this.poolDict = new Dictionary<Type, Stack<TypedPooledObject<TBase>>>();
            this.inUseDict = new Dictionary<Type, List<TBase>>();
            if (typesToPopulate != null) {
                Populate(defaultCapacity, typesToPopulate);
            }
        }

        TypedPooledObject<TBase> Create(Type type) {
            TBase newObj = (TBase)Activator.CreateInstance(type);
            actionOnCreate?.Invoke(newObj);
            TypedPooledObject<TBase> newPooledObject = new TypedPooledObject<TBase>(newObj);
            return newPooledObject;
        }

        private void AddToPoolDict(Type type, TypedPooledObject<TBase> item) {
            if (!poolDict.ContainsKey(type)) {
                poolDict.Add(type, new Stack<TypedPooledObject<TBase>>(defaultCapacity));
            }
            if (collectionCheck && poolDict[type].Count >= maxSize) {
                Debug.LogWarning("TypedObjectPool has reached its max size and cannot add new objects");
                return;
            }
            poolDict[type].Push(item);
        }

        public void Populate(int amount, params Type[] types) {
            foreach (var type in types) {
                Populate(type, amount);
            }
        }
        public void Populate(Type type, int amount) {
            for (int i = 0; i < amount; i++) {
                AddToPoolDict(type, Create(type));
            }
        }
        public void Clear() {
            poolDict.Clear();
            inUseDict.Clear();
        }
        public TBase Get(Type type) {
            Get(type, out var gottenPoolObject);
            return gottenPoolObject;
        }
        public TActual Get<TActual>() where TActual : class, TBase {
            return (TActual)Get(typeof(TActual));
        }
        public TypedPooledObject<TBase> Get(Type type, out TBase v) {
            if (CountAll(type) >= maxSize) {
                Debug.LogWarning($"TypedObjectPool has reached max size cannot Get new {type}");
                v = default;
                return default;
            }
            TypedPooledObject<TBase> gottenPoolObject;
            if (!poolDict.ContainsKey(type) || poolDict[type].Count == 0) {
                gottenPoolObject = Create(type);
            } else {
                gottenPoolObject = poolDict[type].Pop();
            }
            if (!inUseDict.ContainsKey(type)) {
                inUseDict.Add(type, new List<TBase>(defaultCapacity));
            }
            inUseDict[type].Add(gottenPoolObject.poolObject);
            v = gottenPoolObject.poolObject;
            actionOnGet?.Invoke(v);
            return gottenPoolObject;
        }
        public void Release<TActual>(TActual element) where TActual : class, TBase {
            Release(typeof(TActual), element);
        }
        public void Release(Type type, TBase element) {
            if (collectionCheck && (!inUseDict.ContainsKey(type) || !inUseDict[type].Contains(element))) {
                Debug.LogWarning($"Cannot release element {type} '{element}' to Object Pool, was not from this pool");
                return;
            }
            inUseDict[type].Remove(element);
            AddToPoolDict(type, new TypedPooledObject<TBase>(element));
            actionOnRelease?.Invoke(element);
        }

        public void Dispose() {
            foreach (var stack in poolDict.Values) {
                foreach (var poolitem in stack) {
                    actionOnDestroy?.Invoke(poolitem.poolObject);
                    poolitem.Dispose();
                }
                stack.Clear();
            }
            poolDict.Clear();
            inUseDict.Clear();
        }
    }
}