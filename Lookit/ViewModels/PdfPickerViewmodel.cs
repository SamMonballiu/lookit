using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lookit.ViewModels
{
    public class PdfPickerViewmodel : BaseViewModel
    {
        public event Action OnFilenameChanged;
        public ICommand OnSetImageSource;

        public PdfPickerViewmodel()
        {
            OnSetImageSource = new Command<BitmapSource>(bitmap => ImageSource = bitmap);
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
