using CSharpCaliburnMicro1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CSharpCaliburnMicro1.Converters
{
    public class PageOrientationHeightConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageOrientation orientation;
            try
            {
                orientation = (PageOrientation)value;
            }
            catch { return null; }

            switch (orientation)
            {
                case PageOrientation.Portrait:
                    return GraphUtils.GetA4WideSideDeviceIndependentUnit();
                case PageOrientation.Landscape:
                    return GraphUtils.GetA4NarrowSideDeviceIndependentUnit();
                default:
                    return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Back conversion not implemented.");
        }
    }
}
