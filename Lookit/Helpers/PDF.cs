using Docnet.Core;
using Docnet.Core.Models;
using Lookit.Context;
using Lookit.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;

namespace Lookit.Helpers
{
    internal class PDF
    {
        private static readonly IDocLib DocNet = DocLib.Instance;

        public static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                var options = new PdfPageRenderOptions
                {
                    DestinationWidth = (uint)(page.Size.Width),
                    DestinationHeight = (uint)(page.Size.Height),
                };
                await page.RenderToStreamAsync(stream, options);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }

        public static BitmapImage GetImage(string path, int pageWidth, int pageHeight, int page = 0)
        {
            using var docReader = DocNet.GetDocReader(
                path,
                //new PageDimensions(1080 * 4, 1920 * 4)
                new PageDimensions(Math.Min(pageWidth, pageHeight) * 2, Math.Max(pageWidth, pageHeight) * 2)
                );

            using var pageReader = docReader.GetPageReader(page);

            var rawBytes = pageReader.GetImage();

            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            var characters = pageReader.GetCharacters();

            using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            AddBytes(bmp, rawBytes);

            return bmp.ToBitmapImage();
        }

        public static BitmapImage GetImage(int pageNumber)
        {
            using (var page = PageContext.PDF.GetPage((uint)pageNumber - 1))
            {
                var img = GetImage(PageContext.Filename, Convert.ToInt32(page.Size.Width), Convert.ToInt32(page.Size.Height), pageNumber - 1);
                PageContext.Store(pageNumber, img);
                return img;
            }
        }


        private static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }

       

    }
}
