using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ModuleBased.Utils
{
    public static class CollectionUtils
    {
        public static string ToArrayString(this IEnumerable list, string sep = ", ")
        {
            StringBuilder builder = new StringBuilder();
            var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
                builder.Append(enumerator.Current.ToString());
            while (enumerator.MoveNext())
            {
                builder.Append(sep);
                builder.Append(enumerator.Current.ToString());
            }
            return builder.ToString();
        }

        public static string ToArrayString<T>(this IEnumerable<T> list, string sep = ", ")
        {
            StringBuilder builder = new StringBuilder();
            var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
                builder.Append(enumerator.Current.ToString());
            while (enumerator.MoveNext())
            {
                builder.Append(sep);
                builder.Append(enumerator.Current.ToString());
            }
            return builder.ToString();
        }

    }
}