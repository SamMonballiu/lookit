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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Windows.Point? _first;
        private System.Windows.Point? _second;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Derp()
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
            Derp();
        }

        private void ImgControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(ImgMain);
            if (_first is null)
            {
                _first = clickPoint;
            }
            else
            {
                _second = clickPoint;
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
        }
    }
}
