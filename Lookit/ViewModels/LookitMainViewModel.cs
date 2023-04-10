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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PolygonMeasurements))]
        [NotifyPropertyChangedFor(nameof(LineMeasurements))]
        [NotifyPropertyChangedFor(nameof(Measurements))]
        [NotifyPropertyChangedFor(nameof(Scale))]
        protected int _selectedPage = 1;

        private Dictionary<int, ObservableCollection<MeasurementViewModel>> _pagedMeasurements = new();
        private Dictionary<int, Scale> _pagedScales = new();

        public Scale Scale
        {
            get
            {
                if (!_pagedScales.ContainsKey(_selectedPage))
                {
                    _pagedScales.Add(_selectedPage, Scale.Default);
                }
                return _pagedScales[_selectedPage];
            } set
            {
                _pagedScales[_selectedPage] = value;
            }
        }

        public ObservableCollection<MeasurementViewModel> Measurements
        {
            get
            {
                if (!_pagedMeasurements.ContainsKey(_selectedPage))
                {
                    _pagedMeasurements.Add(_selectedPage, new ObservableCollection<MeasurementViewModel>());
                }
                return _pagedMeasurements[_selectedPage];
            }
        }

        [ObservableProperty]
        private double _zoomLevel = 1;

        [ObservableProperty]
        private Mode _mode = Mode.MeasurePolygon;

        public IEnumerable<PolygonMeasurementViewModel> PolygonMeasurements => Measurements.OfType<PolygonMeasurementViewModel>();
        public IEnumerable<LineMeasurementViewModel> LineMeasurements => Measurements.OfType<LineMeasurementViewModel>();

        [ObservableProperty]
        private ObservableCollection<Point> _tempPoints = new();
        
        public string TempPointsString => string.Join(",", _tempPoints.Select(p => $"{p.X}, {p.Y}"));
        public Point FirstTempPoint => _tempPoints.Any() 
            ? new Point(TempPoints.First().X, TempPoints.First().Y) 
            : new Point(int.MaxValue, int.MaxValue);

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
        public ICommand OnSetScale { get; private set; }
        
        public static bool IsControlDown()
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
            if (Mode != Mode.MeasurePolygon)
            {
                return;
            }

            if (_tempPoints.Any())
            {
                TempPoints.Remove(TempPoints.Last());
                OnPropertyChanged(nameof(TempPointsString));
                OnPropertyChanged(nameof(FirstTempPoint));
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
            OnSetScale = new RelayCommand<Scale>(scale =>
            {
                Scale = scale;
                OnPropertyChanged(nameof(Scale));
                foreach (var measurement in _pagedMeasurements[_selectedPage])
                {
                    measurement.Scale = scale;
                }
            });
        }
    }
}
