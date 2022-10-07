using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSharpCaliburnMicro1.Helpers
{
	public static class InternalTreeHelper
	{
		public static FrameworkElement GetRootUserControl(FrameworkElement fe)
		{
			if (fe == null)
				return fe;
			if (fe.Parent is UserControl)
				return fe.Parent as FrameworkElement;
			return GetRootUserControl(fe.Parent as FrameworkElement);
		}
		public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
		{
			T child = default(T);

			int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < numVisuals; i++)
			{
				Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
				child = v as T;
				if (child == null)
				{
					child = GetVisualChild<T>(v);
				}
				if (child != null)
				{
					break;
				}
			}
			return child;
		}

		/// <summary>
		/// Looks for a child control within a parent by name
		/// </summary>
		public static T FindChild<T>(DependencyObject parent, string childName)
		where T : DependencyObject
		{
			// Confirm parent and childName are valid.
			if (parent == null) return null;

			T foundChild = null;

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				// If the child is not of the request child type child
				T childType = child as T;
				if (childType == null)
				{
					// recursively drill down the tree
					foundChild = FindChild<T>(child, childName);

					// If the child is found, break so we do not overwrite the found child.
					if (foundChild != null) break;
				}
				else if (!string.IsNullOrEmpty(childName))
				{
					var frameworkElement = child as FrameworkElement;
					// If the child's name is set for search
					if (frameworkElement != null && frameworkElement.Name == childName)
					{
						// if the child's name is of the request name
						foundChild = (T)child;
						break;
					}
					else
					{
						// recursively drill down the tree
						foundChild = FindChild<T>(child, childName);

						// If the child is found, break so we do not overwrite the found child.
						if (foundChild != null) break;
					}
				}
				else
				{
					// child element found.
					foundChild = (T)child;
					break;
				}
			}

			return foundChild;
		}


	}
}
