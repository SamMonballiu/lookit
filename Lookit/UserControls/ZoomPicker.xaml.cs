using System;
using System.Linq;
using System.Windows.Controls;

namespace Lookit.UserControls
{

    class ZoomOption
    {
        public double Value { get; private set; }
        public string Name => $"{Value * 100}%";

        private ZoomOption() { }
        public static ZoomOption From(double value) => new ZoomOption() { Value = value };
    }

    public class ZoomChangedEventArgs: EventArgs
    {
        public double Value { get; private set; }
        public ZoomChangedEventArgs(double value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Interaction logic for ZoomPicker.xaml
    /// </summary>
    /// 
    public partial class ZoomPicker : UserControl
    {
        private readonly double[] _zoomOptions = new[] { 0.125, 0.25, 0.33, 0.5, 0.66, 0.75, 1, 1.25, 1.5, 2, 2.5, 3, 3.5, 4 };

        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;
        protected void OnZoomChanged(double value) => ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(value));
        public ZoomPicker()
        {
            InitializeComponent();

            BtnZoomIn.Click += (o, e) =>
            {
                ddnZoom.SelectedIndex++;
            };

            BtnZoomOut.Click += (o, e) =>
            {
                ddnZoom.SelectedIndex--;
            };

            ddnZoom.ItemsSource = _zoomOptions.Select(ZoomOption.From);
            ddnZoom.DisplayMemberPath = "Name";
            ddnZoom.SelectedValuePath = "Value";
            ddnZoom.SelectedIndex = _zoomOptions.ToList().IndexOf(1);
        }

        private void ddnZoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dropdown = (sender as ComboBox);
            var zoomValue = (double)dropdown.SelectedValue;
            OnZoomChanged(zoomValue);
            BtnZoomIn.IsEnabled = dropdown.SelectedIndex < _zoomOptions.Length - 1;
            BtnZoomOut.IsEnabled = dropdown.SelectedIndex > 0;
        }

        public void ZoomIn()
        {
            if (BtnZoomIn.IsEnabled)
            {
                ddnZoom.SelectedIndex++;
            }
        }

        public void ZoomOut()
        {
            if (BtnZoomOut.IsEnabled)
            {
                ddnZoom.SelectedIndex--;
            }
        }
    }
}
