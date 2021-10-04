using System;

namespace Sentinel.Common.Extensions
{
    public static class ThrowIfNullExtension
    {
        public static void ThrowIfNull<T>(this T item, string name = null) where T : class
        {

            if (item == null)
                throw new ArgumentNullException(name ?? "guarded argument was null");

            // If you use Code Contracts.
            // var param = typeof(T).GetProperties()[0];
            // var paramName = param.Name;
            // if (!string.IsNullOrEmpty(name))
            // {
            //     paramName = name;
            // }
            // if (param.GetValue(item, null) == null)
            //     throw new ArgumentNullException(param.Name);
        }
    }
}