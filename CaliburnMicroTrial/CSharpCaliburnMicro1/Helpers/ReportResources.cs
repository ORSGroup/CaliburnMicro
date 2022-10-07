using System.Windows.Media;

namespace CSharpCaliburnMicro1.Helpers
{
    public sealed class ReportResources
    {
        private static readonly ReportResources resourceManager = new ReportResources();
        private static readonly object lockObject = new object();

        private ReportResources()
        {

        }

        public static ReportResources Instance
        {
            get
            {
                return resourceManager;
            }
        }

        public static ImageSource WatermarkImage { get; internal set; }

        public void SetWatermarkImage(ImageSource image)
        {
            lock (lockObject)
            {
                WatermarkImage = image;
            }
        }


        public static ImageSource FooterImage { get; internal set; }

        public void SetFooterImage(ImageSource image)
        {
            lock (lockObject)
            {
                FooterImage = image;
            }
        }


        public static Color FeaturingColor { get; internal set; }

        public void SetFeaturingColor(Color color)
        {
            lock (lockObject)
            {
                FeaturingColor = color;
            }
        }


        public static double SectionLabelWidth { get; internal set; }

        public void SetSectionLabelWidth(double value)
        {
            lock (lockObject)
            {
                SectionLabelWidth = value;
            }
        }


        public static PageOrientation PageOrientation { get; internal set; }

        public void SetPageOrientation(PageOrientation orientation)
        {
            lock (lockObject)
            {
                PageOrientation = orientation;
            }
        }


    }
}
