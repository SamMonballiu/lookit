using Lookit.Models;
using Lookit.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lookit
{


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

        private System.Drawing.Point? _first;
        private System.Drawing.Point? _second;

        private Scale _scale;

        public LookitMainViewModel Viewmodel => (DataContext as LookitMainViewModel);

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new LookitMainViewModel();
            zoomPicker.ZoomChanged += ZoomPicker_ZoomChanged;
            ListMeasurements.SelectionChanged += (o, e) =>
            {
                BtnShowDistance.IsEnabled = ListMeasurements.SelectedItem != null;
                UpdateCanvas();
            };

            UpdateCanvas();
            UpdateMeasurementList(); 
        }

        private void ZoomPicker_ZoomChanged(object sender, UserControls.ZoomChangedEventArgs e)
        {
            (DataContext as LookitMainViewModel).ZoomLevel = e.Value;
            UpdateCanvas();
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

            var clickPoint = ConvertPoint(e.GetPosition(ImgMain));
            if (_first is null)
            {
                _first = clickPoint;
            }
            else
            {
                _second = clickPoint;
                var measurement = new LineMeasurement(_first.Value, _second.Value);
                //measurement.Straighten(Convert.ToInt32(20 * (1/Viewmodel().ZoomLevel)));
                switch (Mode)
                {
                    case Mode.Measure:
                        Viewmodel.Measurements.Add(MeasurementViewModel.From(measurement, Viewmodel.Scale));
                        UpdateMeasurementList();
                        _first = null;
                        _second = null;
                        break;
                    case Mode.Scale:
                        _second = clickPoint;
                        Viewmodel.SetScale(new Scale(_first.Value, _second.Value, double.Parse(TxtScaleDistance.Text)));
                        _first = null;
                        _second = null;
                        Mode = Mode.Measure;
                        UpdateMeasurementList();
                        break;
                }
            }

            UpdateLabels();
            UpdateCanvas();
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

        private System.Drawing.Point ConvertPoint(System.Windows.Point point)
        {
            return new System.Drawing.Point() { X = Convert.ToInt32(point.X), Y = Convert.ToInt32(point.Y) };
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Scale.SetEnteredDistance(double.Parse(TxtScaleDistance.Text));
            UpdateMeasurementList();
        }

        private void AddRect()
        {
            System.Windows.Shapes.Rectangle rect;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.Fill = new SolidColorBrush(Colors.Black);
            rect.Width = 200;
            rect.Height = 200;
            Canvas.SetLeft(rect, 0);
            Canvas.SetTop(rect, 0);
            CnvMeasure.Children.Add(rect);
        }

        private void UpdateMeasurementList()
        {
            return;
            //var measurements = Viewmodel().Measurements;
            //var scale = Viewmodel().Scale;
            //var list = measurements.Select(x => MeasurementViewModel.From(x, scale));
            //ListMeasurements.ItemsSource = list;
            //ListMeasurements.DisplayMemberPath = "ScaledDistance";
        }

        private void UpdateCanvas()
        {
            var selected = ListMeasurements.SelectedIndex;
            var measurements = Viewmodel.Measurements;
            var scale = Viewmodel.Scale;
            CnvMeasure.Children.Clear();

            if (scale != null)
            {
                AddLine(new LineMeasurement(scale.First, scale.Second), Brushes.Green);

            }

            foreach (var line in measurements)
            {
                AddLine(line.Measurement, selected == measurements.IndexOf(line) ? Brushes.Yellow : Brushes.Red);
            }
        }

        private void AddLine(LineMeasurement measurement, SolidColorBrush brush)
        {
            var zoomLevel = Viewmodel.ZoomLevel;

            var line = new Line
            {
                Stroke = brush,
                StrokeThickness = Math.Max(1, Math.Round(zoomLevel * 3)),
                X1 = measurement.Start.X,
                Y1 = measurement.Start.Y,
                X2 = measurement.End.X,
                Y2 = measurement.End.Y,
                Visibility = Visibility.Visible
            };

            CnvMeasure.Children.Add(line);
        }

        private void AddEllipse(System.Windows.Point point)
        {
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(Colors.DarkRed),
                Fill = new SolidColorBrush(Colors.Red),
                Width = 20,
                Height = 20
            };
            Canvas.SetLeft(ellipse, point.X - 10);
            Canvas.SetTop(ellipse, point.Y - 10);
            CnvMeasure.Children.Add(ellipse);
        }

        private void BtnShowDistance_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListMeasurements.SelectedItem as LineMeasurement;
            if (selected is null)
            {
                return;
            }

            MessageBox.Show(selected.GetScaledDistance(Viewmodel.Scale)?.ToString() ?? "No scale set");
        }
    }
}
