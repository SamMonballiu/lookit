using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lookit.ViewModels
{
    public class LookitMainViewModel : BaseViewModel
    {
        private System.Drawing.Point? _first;
        private System.Drawing.Point? _second;

        private double _zoomLevel = 0.5;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public Scale Scale => ScaleContext.Scale;

        private Mode _mode = Mode.Measure;
        public Mode Mode
        {
            get => _mode;
            set
            {
                SetProperty(ref _mode, value);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PolygonMeasurementViewModel> Measurements { get; } = new ObservableCollection<PolygonMeasurementViewModel>();
        public ObservableCollection<System.Drawing.Point> TempPoints = new ObservableCollection<System.Drawing.Point>();
        
        public string TempPointsString => string.Join(",", TempPoints.Select(p => $"{p.X}, {p.Y}"));


        private BitmapSource _imageSource = null;
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private bool _straighten = true;

        public bool Straighten
        {
            get => _straighten;
            set
            {
                SetProperty(ref _straighten, value);
                OnPropertyChanged();
            }
        }


        public ICommand OnPasteImage { get; private set; }
        public ICommand OnAddPoint { get; private set; }
        public ICommand OnRemovePoint { get; private set; }
        public ICommand OnRemoveMeasurement { get; private set; }

        private void AddMeasurement(System.Drawing.Point point)
        {
            if (Mode == Mode.None)
            {
                return;
            }

            if (!TempPoints.Any() || !point.IsClose(TempPoints.ElementAt(0)))
            {
                TempPoints.Add(point);
            } else if (point.IsClose(TempPoints.ElementAt(0)))
            {
                var measurement = new PolygonalMeasurement(TempPoints.ToList());
                Measurements.Add(PolygonMeasurementViewModel.From(measurement, Scale));
                TempPoints.Clear();
            }

            OnPropertyChanged(nameof(TempPointsString));
        }

        private void RemoveLastPoint()
        {
            if (TempPoints.Any())
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
                        ImageSource = Clipboard.GetImage();
                    }
                }
            }
        }

        public LookitMainViewModel()
        {
            OnPasteImage = new Command(PasteImage);
            OnAddPoint = new Command<System.Drawing.Point>((point) => AddMeasurement(point));
            OnRemovePoint = new Command(RemoveLastPoint);
            OnRemoveMeasurement = new Command<PolygonMeasurementViewModel>(measurement =>
            {
                Measurements.Remove(measurement);
            });
            ScaleContext.OnScaleChanged += (_) => OnPropertyChanged(nameof(Scale));
        }

    }
}
