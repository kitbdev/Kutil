using System;
using UnityEngine;

namespace Kutil {

    public static class ArrayOptions {

        [Flags]
        public enum AddRemoveFlags {
            None = 0,
            AddOnly = 1,
            RemoveOnly = 2,
            AddAndRemove = 3,// Add and Remove flags
        }
    }

    /// <summary>
    /// provides options for array list view, such as disabling add and remove functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ArrayOptionsAttribute : PropertyAttribute {
        public ArrayOptions.AddRemoveFlags canAddRemove { get; set; } = ArrayOptions.AddRemoveFlags.AddAndRemove;

        public bool CannotAddAndRemove() {
            return (int)canAddRemove == 0;
        }
        public bool CanAddAndRemove() {
            return (int)canAddRemove == 3;
        }
        public bool CanAddOnly() {
            return (int)canAddRemove == 1;
        }
        public bool CanRemoveOnly() {
            return (int)canAddRemove == 2;
        }
    }
}