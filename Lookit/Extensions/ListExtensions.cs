using Lookit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lookit.Extensions
{
    internal static class Extensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            var collection = new ObservableCollection<T>();
            foreach (var item in list)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static string ToSquaredString(this ScaleUnit unit)
        {
            return unit switch
            {
                ScaleUnit.None => string.Empty,
                ScaleUnit.Meters => "m²",
                ScaleUnit.Centimeters => "cm²",
                _ => throw new NotSupportedException()
            };
        }
    }
}
