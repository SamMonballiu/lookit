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

        //private readonly Dictionary<int, LookitMainViewModel> _viewmodels = new Dictionary<int, LookitMainViewModel>();

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

            if (PageContext.PDF != null)
            {
                var pages = Enumerable.Range(1, (int)PageContext.PDF.PageCount).ToList();
                //foreach (var page in pages)
                //{
                //   if (!_viewmodels.ContainsKey(page))
                //    {
                //        _viewmodels.Add(page, new LookitMainViewModel());
                //    }
                //}
                //this.DataContext = _viewmodels[PageContext.SelectedPage];
                ddnPages.ItemsSource = pages;
                ddnPages.SelectedValue = PageContext.SelectedPage;
                ddnPages.SelectionChanged += DdnPages_SelectionChanged;
            }

            //Viewmodel.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
        }

        private async void DdnPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageContext.SelectPage((int)ddnPages.SelectedValue);
            //this.DataContext = _viewmodels[PageContext.SelectedPage];
            //Viewmodel.Reset();
            Viewmodel.SelectedPage = PageContext.SelectedPage;
            Viewmodel.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
            var pageNumber = PageContext.SelectedPage;
            using (var page = PageContext.PDF.GetPage((uint)pageNumber - 1))
            {
                var img = await Helpers.PDF.PageToBitmapAsync(page);
                PageContext.Store(pageNumber, img);
                Viewmodel.OnSetImageSource.Execute(img);
            }
        }

        public void ShowPdfPicker()
        {
            new PdfPicker().ShowDialog();
            Viewmodel?.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));

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
                        view.OnConfirm += (Scale scale) => Viewmodel.OnSetScale?.Execute(scale);
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
