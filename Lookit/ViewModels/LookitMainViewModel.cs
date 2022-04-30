using Lookit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookit.ViewModels
{
    public class LookitMainViewModel : INotifyPropertyChanged
    {
        private double _zoomLevel = 1.00;
        public double ZoomLevel
        {
            get => _zoomLevel; set
            {
                _zoomLevel = value;
                OnPropertyChanged(nameof(ZoomLevel));
            }
        }

        public Scale Scale { get; private set; }
        public ObservableCollection<LineMeasurement> LineMeasurements { get; private set; }
        public Mode Mode { get; set; } = Mode.None;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void SetScale(Scale scale) {
            Scale = scale;
            OnPropertyChanged(nameof(Scale));
        }

        public LookitMainViewModel()
        {
            Scale = Scale.Default;
            LineMeasurements = new ObservableCollection<LineMeasurement>()
            {
                new LineMeasurement(new System.Drawing.Point(550, 150), new System.Drawing.Point(60, 100))
            };
        }
    }
}
