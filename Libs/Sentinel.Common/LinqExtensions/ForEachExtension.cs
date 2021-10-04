using System;
using System.Collections.Generic;
using Sentinel.Common.Extensions;

namespace System.Linq
{
    public static class ForEachExtension
    {

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.ThrowIfNull("source");
            action.ThrowIfNull("action");
            foreach (T element in source)
            {
                action(element);
            }
        }
    }
}