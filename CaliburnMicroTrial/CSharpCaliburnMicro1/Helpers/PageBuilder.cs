using System.Windows.Documents;
using System.Windows;

namespace CSharpCaliburnMicro1.Helpers
{
	public static class PageBuilder
	{
		public static PageContent Create(double width, double height, UIElement element)
		{
			var np = new PageContent();
			np.Child = new FixedPage();
			np.Child.Width = width;
			np.Child.Height = height;
			np.Child.Children.Add(element);
			return np;
		}
	}
}
