using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace System.Linq
{
    public static class JsonExtension
    {
        public static string ToJSON<T>(this T item) where T : new()
        {
            var result = JsonConvert.SerializeObject(item);
            return result;
        }


        public static string ToJSON(this object item)
        {
            var result = JsonConvert.SerializeObject(item);
            return result;
        }


        public static T? FromJSON<T>(this string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            T? item = JsonConvert.DeserializeObject<T>(json);
            return item;
        }
    }
}
