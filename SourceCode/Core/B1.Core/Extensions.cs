using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Core
{
    /// <summary>
    /// Static Extensions methods to help with various tasks
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the value from the dictionary if found else return default.
        /// </summary>
        public static V ValueOrDefault<K, V>(this Dictionary<K, V> Col, K Key, V defaultValue)
        {
            if (Col.ContainsKey(Key) && Col[Key] != null)
                return Col[Key];
            else
                return defaultValue;
        }

        /// <summary>
        /// Returns a string with number of characters or the string if it is smaller.
        /// </summary>
        public static string LeftOrLess(this string str, int numCharacters)
        {
            return str.Substring(0, Math.Min(str.Length, numCharacters));
        }

        /// <summary>
        /// If the value is DbNull then set the property to null. Also make sure to change the type instead of casting.
        /// </summary>
        /// <param name="pinfo">Property info whose value will be set</param>
        /// <param name="obj">The object whose property will be set</param>
        /// <param name="value">Value</param>
        public static void SetPropertyValue(this System.Reflection.PropertyInfo pinfo, object obj, object value)
        {
            // If the value is DbNull then use the null for setting the property. Note that if the database
            // allows NULL for a column then the property for that column in the entity should be Nullable.
            // e.g. If the database column is a number and is nullable than the property in the entity
            // class for this column should be defined as Nullable<int>. If the property is defined as int
            // then we will have to default it to 0 even when it is actually Null.
            value = value != DBNull.Value ? value : null;

            // Use ChangeType instead of casting as sometime the data type returned by the database call
            // does not exactly match the entity property but it is convertible to the property type. e.g.
            // a large database number can be read as double in the .NET and the implicit cast will fail
            // in this case.

            // Also, ChangeType does NOT convert between the int and int? (Nullable<int>). So make sure to
            // do the ChangeType on the underlying generic type of a Nullable property.
            if (value != null)
            {
                Type t = pinfo.PropertyType.IsGenericType
                            && pinfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? pinfo.PropertyType.GetGenericArguments()[0]
                    : pinfo.PropertyType;
                value = Convert.ChangeType(value, t);
            }

            // Set the value of the property
            pinfo.SetValue(obj, value, null);
        }

        /// <summary>
        /// Extend IEnumerable to add the functionality of looping using foreach.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="source">Source enumerable collection which will be looped</param>
        /// <param name="action">Action function which is passed the item from the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Extend IEnumerable to add the functionality of looping using for.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="source">Source enumerable collection which will be looped</param>
        /// <param name="action">Action function which is passed the index and item from the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            for (int index = 0; index < source.Count(); index++)
            {
                action(index, source.ElementAt(index));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type t)
        {
            var name = t.Name;
            if(name.Length < 3)
            {
                return false;
            }
            return name[0] == '<'
                && name[1] == '>'
                && name.IndexOf("AnonymousType", StringComparison.Ordinal) > 0;
        }
    }
}
