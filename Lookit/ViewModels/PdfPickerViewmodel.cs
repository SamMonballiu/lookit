using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lookit.ViewModels
{
    public class PdfPickerViewmodel : ObservableObject
    {
        public event Action OnFilenameChanged;
        public ICommand OnSetImageSource;

        public PdfPickerViewmodel()
        {
            OnSetImageSource = new RelayCommand<BitmapSource>(bitmap => ImageSource = bitmap);
        }

        private string _filename;
        public string Filename
        {
            get => _filename;
            set
            {
                SetProperty(ref _filename, value);
                OnFilenameChanged?.Invoke();
            }
        }

        private IEnumerable<int> _pageCount;
        public IEnumerable<int> PageCount
        {
            get => _pageCount;
            set
            {
                SetProperty(ref _pageCount, value);
            }
        }

        private BitmapSource _imageSource = null;
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        
    }
}
