using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http.Extensions
{
    internal static class DictionaryExtensions
    {
        public static bool TryGetValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection, Func<KeyValuePair<TKey, TValue>, bool> predicate, out TValue value)
            where TValue : class
        {
            value = default;

            foreach (KeyValuePair<TKey, TValue> pair in collection)
            {
                if (predicate.Invoke(pair))
                {
                    value = pair.Value;
                    break;
                }
            }

            return value != null;
        }
    }
}
