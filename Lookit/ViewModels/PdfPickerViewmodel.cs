using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lookit.ViewModels
{
    public partial class PdfPickerViewmodel : ObservableObject
    {
        public ICommand OnSetImageSource;

        public PdfPickerViewmodel()
        {
            OnSetImageSource = new RelayCommand<BitmapSource>(bitmap => ImageSource = bitmap);
        }

        [ObservableProperty]
        private string _filename;

        [ObservableProperty]
        private IEnumerable<int> _pageCount;

        [ObservableProperty]
        private BitmapSource _imageSource = null;
    }
}
