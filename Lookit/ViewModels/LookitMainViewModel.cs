using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lookit.ViewModels
{
    public class LookitMainViewModel : BaseViewModel
    {
        private System.Drawing.Point? _first;
        private System.Drawing.Point? _second;

        private double _zoomLevel = 1.00;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public Scale Scale => ScaleContext.Scale;

        private Mode _mode = Mode.Scale;
        public Mode Mode
        {
            get => _mode; 
            set
            {
                SetProperty(ref _mode, value);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MeasurementViewModel> Measurements { get; } = new ObservableCollection<MeasurementViewModel>();

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
            set {
                SetProperty(ref _straighten, value);
                OnPropertyChanged();
            }
        }


        public ICommand OnPasteImage { get; private set; }
        public ICommand OnAddPoint { get; private set; }

        private void AddMeasurement(System.Drawing.Point point)
        {
            if (Mode == Mode.None)
            {
                return;
            }

            var clickPoint = point;
            if (_first is null)
            {
                _first = clickPoint;
            }
            else
            {
                _second = clickPoint;
                var measurement = new LineMeasurement(_first.Value, _second.Value);
                if (Straighten)
                {
                    measurement.Straighten(Convert.ToInt32(20 * (1/ZoomLevel)));
                }
                switch (Mode)
                {
                    case Mode.Measure:
                        Measurements.Add(MeasurementViewModel.From(measurement, Scale));
                        _first = null;
                        _second = null;
                        break;
                }
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
        }
    }
}
