using System.Windows;
using System.Windows.Media;

namespace CSharpCaliburnMicro1.Helpers
{
    internal class VisualOffset
    {
		public static Vector From(Visual v1, Visual v2)
		{
			//suppose they share the same master user control
			var master = InternalTreeHelper.GetRootUserControl(v1 as FrameworkElement);
			var refP = new Point(0, 0);
			var t1 = v1.TransformToAncestor(master);
			var p1 = t1.Transform(refP);
			var t2 = v2.TransformToAncestor(master);
			var p2 = t2.Transform(refP);
			return Point.Subtract(p2, p1);
		}
	}
}
