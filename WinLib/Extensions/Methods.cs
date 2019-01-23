using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;

namespace WinLib.Extensions
{
    public static class Methods
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = System.Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to remove, without returning the value that has the specified key from the <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ignored;
            return dictionary.TryRemove(key, out ignored);
        }

        /// <summary>
        /// Don't use on large collections; iterations must not be affected by temporarily missing items, lock otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bag"></param>
        /// <param name="item"></param>
        public static void Remove<T>(this ConcurrentBag<T> bag, T item)
        {
            List<T> outItems = new List<T>();
            lock (bag)
            {
                while (!bag.IsEmpty)
                {
                    bag.TryTake(out T outItem);
                    if (outItem.Equals(item))
                        break;
                    outItems.Add(outItem);
                }
                foreach (T outItem in outItems)
                    bag.Add(outItem);
            }
        }

        /// <summary>
        /// If you use this extension on a dictionary to add entries, don't mix it with other methods (like TryAdd) in multi-threading environment
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AddToEnd<TValue>(this ConcurrentDictionary<int, TValue> dictionary, TValue value)
        {
            lock (dictionary)
            {
                return dictionary.TryAdd(Enumerable.Range(0, int.MaxValue).Except(dictionary.Keys).First(), value);
            }
        }
        public static T CreateInstance<T>(T type) where T : new()
        {
            return new T();
        }
        public static T CastObject<T>(object obj, T type) where T : class
        {
            return obj as T;
        }
    }

    //class HelperClass
    //{
    //    public static T CreateInstance<T>(T type) where T : new()
    //    {
    //        return new T();
    //    }
    //}
    //class MyType1 { }; class MyType2 { }; class MyType3 { };
    //class Main
    //{
    //    public Main()
    //    {
    //        Type[] myTypes = new Type[] { typeof(MyType1), typeof(MyType2), typeof(MyType3) };
    //        Type myType = myTypes[new Random().Next(1,3)];
    //        var obj1 = HelperClass.CreateInstance(myType);
    //        var obj2 = Activator.CreateInstance(myType);
    //    }
    //}
}
