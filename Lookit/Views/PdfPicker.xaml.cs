using Lookit.Context;
using Lookit.Helpers;
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

            Loaded += btnOpenFile_Click;
        }

        private void PdfPicker_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OpenFileDialog()
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

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var pageNumber = (int)cbxSelectedPage.SelectedValue;
            PageContext.SelectPage(pageNumber);

            if (PageContext.Has(pageNumber))
            {
                Viewmodel.OnSetImageSource.Execute(PageContext.Get(pageNumber));
                return;
            }

            Viewmodel.OnSetImageSource.Execute(PDF.GetImage(pageNumber));
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
