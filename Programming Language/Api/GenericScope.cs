using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    static class GenericScope
    {
        public static string ConvertToString<T, T2>(this Dictionary<T, T2> value, Converter<T, string> TConverter, Converter<T2, string> T2Converter)
        {
            string result = string.Empty;
            T[] keys = value.Keys.ToArray();
            T2[] values = value.Values.ToArray();
            for (int n = 0; n < keys.Length; n++)
            {
                if (n != 0)
                    result += '\n';
                result += keys[n] + " : " + values[n];
            }
            return result;
        }
    }
}
