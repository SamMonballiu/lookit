using Lookit.Context;
using Lookit.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Lookit.Views
{
    /// <summary>
    /// Interaction logic for PdfPicker.xaml
    /// </summary>
    public partial class PdfPicker : Window
    {
        public PdfPickerViewmodel Viewmodel => (DataContext as PdfPickerViewmodel);
        public PdfPicker()
        {
            DataContext = new PdfPickerViewmodel();
            InitializeComponent();

            if (PageContext.PDF != null)
            {
                Viewmodel.PageCount = Enumerable.Range(1, (int)PageContext.PDF.PageCount).ToList();
                Viewmodel.Filename = PageContext.Filename;
                cbxSelectedPage.SelectedValue = PageContext.SelectedPage;
            }
        }

        private static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                var options = new PdfPageRenderOptions { DestinationWidth = 1920 };
                await page.RenderToStreamAsync(stream, options);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }

        private async void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files|*.pdf";
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileName != Viewmodel.Filename)
            {
                Viewmodel.Filename = openFileDialog.FileName;
                PageContext.Filename = openFileDialog.FileName;
                var file = await StorageFile.GetFileFromPathAsync(Viewmodel.Filename);
                PageContext.PDF = await PdfDocument.LoadFromFileAsync(file);
                Viewmodel.PageCount = Enumerable.Range(1, (int)PageContext.PDF.PageCount).ToList();
                PageContext.Clear();
                cbxSelectedPage.SelectedIndex = 0;
                ComboBox_SelectionChanged(null, null);
            }
        }

        private async void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var pageNumber = (int)cbxSelectedPage.SelectedValue;
            PageContext.SelectPage(pageNumber);

            if (PageContext.Has(pageNumber))
            {
                Viewmodel.OnSetImageSource.Execute(PageContext.Get(pageNumber));
                return;
            }

            using (var page = PageContext.PDF.GetPage((uint)pageNumber - 1))
            {
                var img = await PageToBitmapAsync(page);
                PageContext.Store(pageNumber, img);
                Viewmodel.OnSetImageSource.Execute(img);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
