using Lookit.Models;
using Lookit.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

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

        public static Dictionary<int, List<Measurement>> ToPagedList(this Dictionary<int, ObservableCollection<Measurement>> dict)
        {
            var result = new Dictionary<int, List<Measurement>>();
            foreach (var page in dict)
            {
                result.Add(page.Key, page.Value.ToList());
            }

            return result;
        }

        public static string ToUnitString(this ScaleUnit unit)
        {
            return unit switch
            {
                ScaleUnit.None => string.Empty,
                ScaleUnit.Centimeters => "cm",
                ScaleUnit.Meters => "m",
                _ => throw new NotImplementedException(),
            };
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

        public static string ToShortString(this ScaleUnit unit)
        {
            return unit switch
            {
                ScaleUnit.None => string.Empty,
                ScaleUnit.Meters => "m",
                ScaleUnit.Centimeters => "cm",
                _ => throw new NotSupportedException()
            };
        }


        public static PersistablePoint ToPersistablePoint(this System.Drawing.Point point)
            => new() { X = point.X, Y = point.Y };

        public static PersistableScale ToPersistableScale(this Scale scale)
            => new()
            {
                First = scale.First.ToPersistablePoint(),
                Second = scale.Second.ToPersistablePoint(),
                EnteredDistance = scale.EnteredDistance,
                Unit = (int)scale.Unit
            };

        public static PersistableMeasurement ToPersistableMeasurement(this MeasurementViewModel model)
        {
            return new PersistableMeasurement()
            {
                Points = model.Measurement.Points.Select(x => x.ToPersistablePoint()).ToList(),
                Name = model.Name,
            };
        }

        public static MeasurementViewModel ToViewmodel(this PersistableMeasurement me, Scale scale)
        {
            var points = me.Points.Select(pt => new System.Drawing.Point(pt.X, pt.Y)).ToList();

            return points.Count switch
            {
                2 => new LineMeasurementViewModel(new LineMeasurement(points[0], points[1]), scale, me.Name),
                _ => new PolygonMeasurementViewModel(new PolygonalMeasurement(points), scale, me.Name),
            };
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
