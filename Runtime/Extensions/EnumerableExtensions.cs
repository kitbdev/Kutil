using System;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    public static class EnumerableExtensions {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector) {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
        /// <summary>
        /// Returns a string listing all elements of the enumerable
        /// </summary>
        /// <param name="toStrFunc">function to convert elements to string</param>
        /// <param name="includeCount">should prefix enumerable count?</param>
        /// <param name="includeBraces">should braces before and after be included? []</param>
        /// <param name="seperator">seperator between elements. default is ","</param>
        /// <param name="maxCount">maximum number of elements to check. -1 disables</param>
        /// <returns>string with all elements</returns>
        public static string ToStringFull<T>(this IEnumerable<T> enumerable, Func<T, string> toStrFunc = null, bool includeCount = false, bool includeBraces = true, string seperator = ",", int maxCount = -1) {
            if (toStrFunc == null) toStrFunc = e => e.ToString();
            // todo? formatting
            System.Text.StringBuilder str = new System.Text.StringBuilder("");
            int cnt = enumerable.Count();
            if (includeCount) str.Append($"{cnt}");
            if (includeBraces) str.Append("[");
            int i = 0;
            foreach (var e in enumerable) {
                if (maxCount > 0 && i > maxCount) break;
                str.Append(toStrFunc(e));
                if (i < cnt - 1) {
                    str.Append(seperator);
                }
                i++;
            }
            if (maxCount > 0 && i > maxCount) str.Append("...");
            if (includeBraces) str.Append("]");
            return str.ToString();
        }
        // public static string ToStringFull(this IEnumerable<GameObject> enumerable, Func<GameObject, string> toStrFunc = null, bool includeCount = false, bool includeBraces = true, string seperator = ",") {
        // }

        // public static IEnumerable<T> SelectWhere<T, U>(this IEnumerable<U> enumerable, Func<U, T?> func) {
        //     return enumerable.Select(func).Where(v => v != null);
        // }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
            foreach (var v in enumerable) {
                action.Invoke(v);
            }
        }
        public static IEnumerable<T> AppendRange<T>(this IEnumerable<T> enumerable, IEnumerable<T> other) {
            IEnumerable<T> result = enumerable;
            foreach (var v in other) {
                result = result.Append(v);
            }
            return result;
        }
        public static void AppendIfUnique<T>(this IEnumerable<T> enumerable, T value) {
            if (!enumerable.Contains(value)) {
                enumerable.Append(value);
            }
        }
    }
    public static class ListExtensions {

        public static void AddIfUnique<T>(this List<T> enumerable, T value) {
            if (!enumerable.Contains(value)) {
                enumerable.Add(value);
            }
        }
        // }
        public static void AddRangeIfUnique<T>(this List<T> enumerable, IEnumerable<T> other) {
            foreach (var val in other) {
                enumerable.AddIfUnique(val);
            }
        }
        // public static List<T> AddRangeChain<T>(this List<T> enumerable, IEnumerable<T> other) {
        //     enumerable.AddRange(other);
        //     other.Append()
        //     return enumerable;
        // }
    }
}