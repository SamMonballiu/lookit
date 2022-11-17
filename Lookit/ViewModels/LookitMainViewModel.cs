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

namespace Lookit.ViewModels
{
    public partial class LookitMainViewModel : ObservableObject
    {
        public void Reset()
        {
            Measurements.Clear();
            _tempPoints.Clear();
            ScaleContext.Scale = Scale.Default;
            _mode = Mode.Measure;
        }

        [ObservableProperty]
        private double _zoomLevel = 1;

        public Scale Scale => ScaleContext.Scale;

        [ObservableProperty]
        private Mode _mode = Mode.Measure;
        
        [ObservableProperty]
        private ObservableCollection<PolygonMeasurementViewModel> _measurements = new();

        [ObservableProperty]
        private ObservableCollection<System.Drawing.Point> _tempPoints = new();
        
        public string TempPointsString => string.Join(",", _tempPoints.Select(p => $"{p.X}, {p.Y}"));
        public System.Drawing.Point FirstTempPoint => _tempPoints.Any() 
            ? new System.Drawing.Point(TempPoints.First().X - 3, TempPoints.First().Y - 3) 
            : new System.Drawing.Point(-10, -10);

        [ObservableProperty]
        private BitmapSource _imageSource;

        [ObservableProperty]
        private bool _straighten = true;

        public ICommand OnPasteImage { get; private set; }
        public ICommand OnAddPoint { get; private set; }
        public ICommand OnRemovePoint { get; private set; }
        public ICommand OnRemoveMeasurement { get; private set; }
        public ICommand OnSetImageSource { get; private set; }
        
        private static bool IsControlDown()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        private void AddMeasurement(System.Drawing.Point point)
        {
            if (Mode == Mode.None)
            {
                return;
            }

            if (!TempPoints.Any() || !point.IsClose(TempPoints.ElementAt(0)))
            {
                if (Straighten && !IsControlDown() && TempPoints.Any())
                {
                    var previousPoint = _tempPoints.Last();
                    var tolerance = Convert.ToInt32(10 * (1 / _zoomLevel));
                    point = point.Align(previousPoint, tolerance);
                }
                TempPoints.Add(point);
            } else if (point.IsClose(TempPoints.ElementAt(0)))
            {
                var measurement = new PolygonalMeasurement(TempPoints.ToList());
                Measurements.Add(PolygonMeasurementViewModel.From(measurement, Scale));
                TempPoints.Clear();
            }
            OnPropertyChanged(nameof(TempPointsString));
            OnPropertyChanged(nameof(FirstTempPoint));
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
                    if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
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
            OnAddPoint = new RelayCommand<System.Drawing.Point>(AddMeasurement);
            OnRemovePoint = new RelayCommand(RemoveLastPoint);
            OnRemoveMeasurement = new RelayCommand<PolygonMeasurementViewModel>(measurement =>
            {
                Measurements.Remove(measurement);
            });
            ScaleContext.OnScaleChanged += (_) => OnPropertyChanged(nameof(Scale));
        }

    }
}
