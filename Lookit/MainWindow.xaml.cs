using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Drawing.Point;

namespace Lookit
{
    public class Scale
    {
        public System.Windows.Point First { get; set; }
        public System.Windows.Point Second { get; set; }
        private double _enteredDistance;
        public double EnteredDistance => _enteredDistance;
        public double ActualDistance 
            => Math.Sqrt((Math.Pow(First.X - Second.X, 2) + Math.Pow(First.Y - Second.Y, 2)));
        public double DistanceUnit
            => EnteredDistance / ActualDistance;

        public Scale(System.Windows.Point first, System.Windows.Point second, double distance)
        {
            First = first;
            Second = second;
            _enteredDistance = distance;
        }

        public void SetEnteredDistance(double distance)
        {
            _enteredDistance = distance;
        }
    }

    public enum Mode
    {
        None,
        Scale,
        Measure
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public Mode Mode { get; set; } = Mode.None;

        private System.Windows.Point? _first;
        private System.Windows.Point? _second;

        private Scale _scale;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
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
                        BitmapSource src = Clipboard.GetImage();
                        ImgMain.Source = src;
                        //System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                        //IntPtr hBitmap = bitmap.GetHbitmap();
                        //try
                        //{
                        //    ImgMain.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        //    Console.WriteLine("Clipboard copied to UIElement");
                        //}
                        //finally
                        //{
                        //    DeleteObject(hBitmap);
                        //}
                    }
                }
            }
        }

        private void BtnPaste_Click(object sender, RoutedEventArgs e)
        {
            PasteImage();
        }

        private void ImgControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mode == Mode.None)
            {
                return;
            }

            var clickPoint = e.GetPosition(ImgMain);
            if (_first is null)
            {
                _first = clickPoint;
            }
            else
            {
                _second = clickPoint;
                switch (Mode)
                {
                    case Mode.Measure:
                        var distance = GetScaledDistance(_first.Value, clickPoint);
                        MessageBox.Show(distance.ToString("F"));
                        _first = null;
                        _second = null;
                        break;
                    case Mode.Scale:
                        _second = clickPoint;
                        _scale = new Scale(_first.Value, _second.Value, double.Parse(TxtScaleDistance.Text));
                        _first = null;
                        _second = null;
                        Mode = Mode.Measure;
                        break;
                }
            }

            UpdateLabels();
        }

        private void UpdateLabels()
        {

            if (_first != null)
            {
                LblPosFirst.Content = $"{_first.Value.X:F} {_first.Value.Y:F}";
            }

            if (_second != null)
            {
                LblPosSecond.Content = $"{_second.Value.X:F} {_second.Value.Y:F}";
            }
            LblMode.Content = Mode;
        }

        private void BtnScale_Click(object sender, RoutedEventArgs e)
        {
            Mode = Mode.Scale;
            UpdateLabels();
        }

        private void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            Mode = Mode.Measure;
            UpdateLabels();
        }

        private double GetDistance(System.Windows.Point first, System.Windows.Point second)
            => Math.Sqrt((Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)));

        private double GetScaledDistance(System.Windows.Point first, System.Windows.Point second)
        {
            var clickedDistance = GetDistance(first, second);
            var measureDistance = clickedDistance * _scale.DistanceUnit;
            return measureDistance;
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            _scale.SetEnteredDistance(double.Parse(TxtScaleDistance.Text));
        }
    }
}
