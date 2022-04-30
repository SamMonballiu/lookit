﻿using Lookit.Models;
using Lookit.ViewModels;
using System;
using System.Collections.Generic;
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

        private readonly List<LineMeasurement> _lineMeasurements = new List<LineMeasurement>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new LookitMainViewModel();
            zoomPicker.ZoomChanged += ZoomPicker_ZoomChanged;
        }

        private void ZoomPicker_ZoomChanged(object sender, UserControls.ZoomChangedEventArgs e)
        {
            (DataContext as LookitMainViewModel).ZoomLevel = e.Value;
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
                (DataContext as LookitMainViewModel).LineMeasurements.Add(measurement);
                switch (Mode)
                {
                    case Mode.Measure:
                        var distance = measurement.GetScaledDistance(_scale);
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

        private System.Drawing.Point ConvertPoint(System.Windows.Point point)
        {
            return new System.Drawing.Point() { X = Convert.ToInt32(point.X), Y = Convert.ToInt32(point.Y) };
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            _scale.SetEnteredDistance(double.Parse(TxtScaleDistance.Text));
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
    }
}
