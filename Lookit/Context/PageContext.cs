using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Lookit.Context
{
    internal class PageContext
    {
        private static readonly Dictionary<int, BitmapImage> _pages = new Dictionary<int, BitmapImage>();
        private static int _selectedPage = 0;

        public static BitmapImage Get(int index)
            => Has(index) ? _pages[index] : null;

        public static void Store(int index, BitmapImage image)
            => _pages[index] = image;

        public static void Clear()
            => _pages.Clear();

        public static bool Has(int index)
            => _pages.ContainsKey(index);

        public static int SelectedPage => _selectedPage;

        public static void SelectPage(int page)
            => _selectedPage = page;

        public static string Filename { get; set; }

        public static Windows.Data.Pdf.PdfDocument PDF { get; set; }
    }
}
