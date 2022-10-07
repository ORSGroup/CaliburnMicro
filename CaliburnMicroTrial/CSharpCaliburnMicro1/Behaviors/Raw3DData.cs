using System.Windows;
using System.Windows.Media.Media3D;

namespace CSharpCaliburnMicro1.Behaviors
{
    internal class BorderData
    {
        public Point3D CenterUp { get; set; }
        public Point3D CenterLow { get; set; }
        public int BorderIndex { get; set; }
    }
    internal class Raw3DData
    {


        public static BorderData GetData(DependencyObject obj)
        {
            return (BorderData)obj.GetValue(DataProperty);
        }

        public static void SetData(DependencyObject obj, BorderData value)
        {
            obj.SetValue(DataProperty, value);
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(BorderData), typeof(Raw3DData), new PropertyMetadata(null));


    }
}
