using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpCaliburnMicro1.Behaviors
{
	public class Overflowable
	{

		public static string GetBreaker(DependencyObject obj)
		{
			return (string)obj.GetValue(BreakerProperty);
		}

		public static void SetBreaker(DependencyObject obj, string value)
		{
			obj.SetValue(BreakerProperty, value);
		}

		// Using a DependencyProperty as the backing store for Breaker.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BreakerProperty =
			DependencyProperty.RegisterAttached("Breaker", typeof(string), typeof(Overflowable), new UIPropertyMetadata(null));




		public static bool GetCanOverflow(DependencyObject obj)
		{
			return (bool)obj.GetValue(CanOverflowProperty);
		}

		public static void SetCanOverflow(DependencyObject obj, bool value)
		{
			obj.SetValue(CanOverflowProperty, value);
		}

		// Using a DependencyProperty as the backing store for CanOverflow.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanOverflowProperty =
			DependencyProperty.RegisterAttached("CanOverflow", typeof(bool), typeof(Overflowable), new UIPropertyMetadata(false));



		public static string GetViewModel(DependencyObject obj)
		{
			return (string)obj.GetValue(ViewModelProperty);
		}

		public static void SetViewModel(DependencyObject obj, string value)
		{
			obj.SetValue(ViewModelProperty, value);
		}


		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.RegisterAttached("ViewModel", typeof(string), typeof(Overflowable), new UIPropertyMetadata(""));



		public static string GetModel(DependencyObject obj)
		{
			return (string)obj.GetValue(ModelProperty);
		}

		public static void SetModel(DependencyObject obj, string value)
		{
			obj.SetValue(ModelProperty, value);
		}

		// Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ModelProperty =
			DependencyProperty.RegisterAttached("Model", typeof(string), typeof(Overflowable), new UIPropertyMetadata(""));



		public static int GetCountPerFirstPage(DependencyObject obj)
		{
			return (int)obj.GetValue(CountPerFirstPageProperty);
		}

		public static void SetCountPerFirstPage(DependencyObject obj, int value)
		{
			obj.SetValue(CountPerFirstPageProperty, value);
		}

		// Using a DependencyProperty as the backing store for CountPerFirstPage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CountPerFirstPageProperty =
			DependencyProperty.RegisterAttached("CountPerFirstPage", typeof(int), typeof(Overflowable), new UIPropertyMetadata(0));



		public static int GetCountPerPage(DependencyObject obj)
		{
			return (int)obj.GetValue(CountPerPageProperty);
		}

		public static void SetCountPerPage(DependencyObject obj, int value)
		{
			obj.SetValue(CountPerPageProperty, value);
		}

		// Using a DependencyProperty as the backing store for CountPerPage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CountPerPageProperty =
			DependencyProperty.RegisterAttached("CountPerPage", typeof(int), typeof(Overflowable), new UIPropertyMetadata(0));


	}
}
