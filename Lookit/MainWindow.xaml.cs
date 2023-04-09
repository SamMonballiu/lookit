using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using Lookit.ViewModels;
using Lookit.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Lookit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public LookitMainViewModel Viewmodel => (DataContext as LookitMainViewModel);

        private Point? _clickedPoint;


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new LookitMainViewModel();
            zoomPicker.ZoomChanged += ZoomPicker_ZoomChanged;
            //ImgMain.Source = new BitmapImage(new Uri(@"Assets/test.GIF", UriKind.RelativeOrAbsolute));

            ShowPdfPicker();

            ListMeasurements.SelectionChanged += (s, e) =>
            {
                try
                {
                    Viewmodel.SelectedMeasurement = Viewmodel.Measurements[ListMeasurements.SelectedIndex]?.Measurement;
                } catch
                {
                    //TODO Fix
                }
            };
        }

        public void ShowPdfPicker()
        {
            new PdfPicker().ShowDialog();
            Viewmodel.Reset();
            Viewmodel.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));

        }

        private void ZoomPicker_ZoomChanged(object sender, UserControls.ZoomChangedEventArgs e)
        {
            (DataContext as LookitMainViewModel).ZoomLevel = e.Value;
        }

        private void ImgControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (Viewmodel.Mode == Mode.Scale || Viewmodel.Mode == Mode.None)
                    {
                        if (_clickedPoint is null)
                        {
                            _clickedPoint = e.GetPosition(ImgMain);
                            return;
                        }

                        var view = new SetScaleView(_clickedPoint.Value, e.GetPosition(ImgMain));
                        view.ShowDialog();
                        _clickedPoint = null;
                        return;
                    }

                    Viewmodel.OnAddPoint.Execute(e.GetPosition(ImgMain).ToPoint());
                    break;

                case MouseButton.Right:
                    Viewmodel.OnRemovePoint.Execute(null);
                    break;
            }
        }

        private void BtnScale_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.Scale;
        }

        private void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasurePolygon; //TODO!
        }

        private void BtnMeasureLine_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasureLine;
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            ScaleContext.Scale = ScaleContext.Scale.UpdateDistance(double.Parse(TxtScaleDistance.Text));
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
            //CnvMeasure.Children.Add(rect);
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

            //CnvMeasure.Children.Add(line);
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
            //CnvMeasure.Children.Add(ellipse);
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Loaded");
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ShowPdfPicker();
        }

        private void ListMeasurements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
