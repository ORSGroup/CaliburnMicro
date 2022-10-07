using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CSharpCaliburnMicro1
{
    public static class Images
    {
        internal static readonly ImageSource footerImage = LoadBitmap(new Uri(@"pack://application:,,,/CSharpCaliburnMicro1;Component/Images/logofooter.png"));

        internal static readonly ImageSource watermarkImage =
            LoadBitmap(new Uri(@"pack://application:,,,/CSharpCaliburnMicro1;Component/Images/watermark.png"));

        public static ImageSource LoadBitmap(Uri uri)
        {
            BitmapImage image = new BitmapImage(uri);
            image.Freeze();
            return image;
        }
    }
}
