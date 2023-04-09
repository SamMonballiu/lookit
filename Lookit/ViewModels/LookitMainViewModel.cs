using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Clipboard = System.Windows.Clipboard;
using IDataObject = System.Windows.IDataObject;
using Point = System.Drawing.Point;
using Lookit.Extensions;

namespace Lookit.ViewModels
{
    public partial class LookitMainViewModel : ObservableObject
    {
        public void Reset()
        {
            Measurements.Clear();
            _tempPoints.Clear();
            ScaleContext.Scale = Scale.Default;
            _mode = Mode.MeasurePolygon;
        }

        public Scale Scale => ScaleContext.Scale;

        [ObservableProperty]
        private double _zoomLevel = 1;

        [ObservableProperty]
        private Mode _mode = Mode.MeasurePolygon;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PolygonMeasurements))]
        [NotifyPropertyChangedFor(nameof(LineMeasurements))]
        private ObservableCollection<MeasurementViewModel> _measurements = new();

        public IEnumerable<PolygonMeasurementViewModel> PolygonMeasurements => Measurements.OfType<PolygonMeasurementViewModel>();
        public IEnumerable<LineMeasurementViewModel> LineMeasurements => Measurements.OfType<LineMeasurementViewModel>();

        [ObservableProperty]
        private ObservableCollection<Point> _tempPoints = new();
        
        public string TempPointsString => string.Join(",", _tempPoints.Select(p => $"{p.X}, {p.Y}"));
        public Point FirstTempPoint => _tempPoints.Any() 
            ? new Point(TempPoints.First().X - 3, TempPoints.First().Y - 3) 
            : new Point(-10, -10);

        [ObservableProperty]
        private BitmapSource _imageSource;

        [ObservableProperty]
        private Measurement _selectedMeasurement;

        [ObservableProperty]
        private bool _straighten = true;

        public ICommand OnPasteImage { get; private set; }
        public ICommand OnAddPoint { get; private set; }
        public ICommand OnRemovePoint { get; private set; }
        public ICommand OnRemoveMeasurement { get; private set; }
        public ICommand OnToggleMeasurementHidden { get; private set; }
        public ICommand OnSetImageSource { get; private set; }
        public ICommand OnSwitchMode { get; private set; }
        
        private static bool IsControlDown()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        private void AddPoint(Point point)
        {
            switch (_mode)
            {
                case Mode.MeasurePolygon:
                    AddPolygonPoint(point);
                    break;
                case Mode.MeasureLine:
                    AddLinePoint(point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_mode));
            }
        }

        private void AddLinePoint(Point point)
        {
            if (_tempPoints.Any())
            {
                if (_straighten && !IsControlDown())
                {
                    point = Align(_tempPoints.First(), point);
                }

                var measurement = new LineMeasurement(_tempPoints.First(), point);
                Measurements.Add(new LineMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                OnPropertyChanged(nameof(LineMeasurements));
                TempPoints.Clear();
                return;
            }

            _tempPoints.Add(point);
        }

        private void AddPolygonPoint(Point point)
        {
            if (!TempPoints.Any() || !point.IsClose(TempPoints.ElementAt(0)))
            {
                if (Straighten && !IsControlDown() && TempPoints.Any())
                {
                    point = Align(_tempPoints.Last(), point);
                }
                TempPoints.Add(point);
            } else if (point.IsClose(TempPoints.ElementAt(0)))
            {
                var measurement = new PolygonalMeasurement(TempPoints.ToList());
                Measurements.Add(new PolygonMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                TempPoints.Clear();
                OnPropertyChanged(nameof(PolygonMeasurements));
            }
            OnPropertyChanged(nameof(TempPointsString));
            OnPropertyChanged(nameof(FirstTempPoint));
        }

        private Point Align(Point first, Point second)
        {
            var tolerance = Convert.ToInt32(10 * (1 / _zoomLevel));
            return second.Align(first, tolerance);
        }

        private void RemoveLastPoint()
        {
            if (_tempPoints.Any())
            {
                TempPoints.Remove(TempPoints.Last());
                OnPropertyChanged(nameof(TempPointsString));
            }
        }

        private void PasteImage()
        {
            if (Clipboard.ContainsImage())
            {
                IDataObject clipboardData = Clipboard.GetDataObject();
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                    {
                        _imageSource = Clipboard.GetImage();
                    }
                }
            }
        }

        public LookitMainViewModel()
        {
            OnSetImageSource = new RelayCommand<BitmapSource>((bitmap) =>
            {
                ImageSource = bitmap;
            });
            OnPasteImage = new RelayCommand(PasteImage);
            OnAddPoint = new RelayCommand<Point>(AddPoint);
            OnRemovePoint = new RelayCommand(RemoveLastPoint);
            OnRemoveMeasurement = new RelayCommand<MeasurementViewModel>(measurement =>
            {
                Measurements.Remove(measurement);
                OnPropertyChanged(nameof(PolygonMeasurements));
                OnPropertyChanged(nameof(LineMeasurements));
            });
            OnToggleMeasurementHidden = new RelayCommand<MeasurementViewModel>(measurement =>
            {
                measurement.Hidden = !measurement.Hidden;
            });
            OnSwitchMode = new RelayCommand<Mode>(mode => Mode = mode);
            ScaleContext.OnScaleChanged += (_) => OnPropertyChanged(nameof(Scale));
        }
    }
}
