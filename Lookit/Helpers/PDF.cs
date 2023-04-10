using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;

namespace Lookit.Helpers
{
    internal class PDF
    {
        public static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                var options = new PdfPageRenderOptions
                {
                    DestinationWidth = (uint)(page.Dimensions.MediaBox.Width * 1),
                    DestinationHeight = (uint)(page.Dimensions.MediaBox.Height * 1),
                };
                await page.RenderToStreamAsync(stream, options);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }

    }
}
