using System;
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
            {
                if (enumerator.Current != null)
                    builder.Append(enumerator.Current.ToString());
            }
            while (enumerator.MoveNext())
            {
                builder.Append(sep);
                if (enumerator.Current != null)
                    builder.Append(enumerator.Current.ToString());
            }
            return builder.ToString();
        }

        public static string ToArrayString<T>(this IEnumerable<T> list, string sep = ", ")
        {
            StringBuilder builder = new StringBuilder();
            var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
            {
                if(enumerator.Current != null)
                    builder.Append(enumerator.Current.ToString());
            }
            while (enumerator.MoveNext())
            {
                builder.Append(sep); 
                if (enumerator.Current != null)
                    builder.Append(enumerator.Current.ToString());
            }
            return builder.ToString();
        }

        /// <summary>
        /// Deverge source to much more destination properties
        /// </summary>
        /// <remarks>
        /// Get the destination properties which the sources have.
        /// </remarks>
        public static IEnumerable<TDest> Deverge<TSource, TDest>(this IEnumerable<TSource> sources, Func<TSource, IEnumerable<TDest>> converter)
        {
            List<TDest> list = new List<TDest>();
            foreach(var source in sources)
            {
                list.AddRange(converter(source));
            }
            return list;
        }

        /// <summary>
        /// Deverge source to much more destination properties and guaranteed the destination unique
        /// </summary>
        /// <remarks>
        /// Get the unique destination properties which the sources have.
        /// </remarks>
        public static IEnumerable<TDest> UniqueDeverge<TSource, TDest>(this IEnumerable<TSource> sources, Func<TSource, IEnumerable<TDest>> converter)
        {
            HashSet<TDest> set = new HashSet<TDest>();
            foreach (var source in sources)
            {
                set.UnionWith(converter(source));
            }
            return set;
        }

        /// <summary>
        /// Divert sources to multiple diversion with specific predicate
        /// </summary>
        public static void Divert<T>(
            this IEnumerable<T> sources, 
            Predicate<T> predicate1, out IEnumerable<T> diversion1, 
            Predicate<T> predicate2, out IEnumerable<T> diversion2)
        {
            List<T> d1 = new List<T>();
            List<T> d2 = new List<T>();
            foreach(var source in sources)
            {
                if (predicate1(source))
                    d1.Add(source);
                if (predicate2(source))
                    d2.Add(source);
            }
            diversion1 = d1;
            diversion2 = d2;
        }

        /// <summary>
        /// Divert sources to multiple diversion with specific predicate
        /// </summary>
        public static void Divert<T>(
            this IEnumerable<T> sources,
            Predicate<T> predicate1, out IEnumerable<T> diversion1,
            Predicate<T> predicate2, out IEnumerable<T> diversion2,
            Predicate<T> predicate3, out IEnumerable<T> diversion3)
        {
            List<T> d1 = new List<T>();
            List<T> d2 = new List<T>();
            List<T> d3 = new List<T>();
            foreach (var source in sources)
            {
                if (predicate1(source))
                    d1.Add(source);
                if (predicate2(source))
                    d2.Add(source);
                if (predicate3(source))
                    d3.Add(source);
            }
            diversion1 = d1;
            diversion2 = d2;
            diversion3 = d3;
        }

        /// <summary>
        /// Divert sources to multiple diversion with specific predicate
        /// </summary>
        public static void Divert<T>(
            this IEnumerable<T> sources,
            Predicate<T> predicate1, out IEnumerable<T> diversion1,
            Predicate<T> predicate2, out IEnumerable<T> diversion2,
            Predicate<T> predicate3, out IEnumerable<T> diversion3,
            Predicate<T> predicate4, out IEnumerable<T> diversion4)
        {
            List<T> d1 = new List<T>();
            List<T> d2 = new List<T>();
            List<T> d3 = new List<T>();
            List<T> d4 = new List<T>();
            foreach (var source in sources)
            {
                if (predicate1(source))
                    d1.Add(source);
                if (predicate2(source))
                    d2.Add(source);
                if (predicate3(source))
                    d3.Add(source);
                if (predicate4(source))
                    d4.Add(source);
            }
            diversion1 = d1;
            diversion2 = d2;
            diversion3 = d3;
            diversion4 = d4;
        }
    }
}