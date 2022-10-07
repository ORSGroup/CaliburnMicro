using Caliburn.Micro;
using CSharpCaliburnMicro1.Helpers;
using CSharpCaliburnMicro1.ViewInterfaces;
using CSharpCaliburnMicro1.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace CSharpCaliburnMicro1.Behaviors
{
    public class ReportRunner
    {
        internal static readonly LocalDataStoreSlot runnerContextSlot = Thread.GetNamedDataSlot("REPORTRUNNERSLOT");

		public static string GetTarget(DependencyObject obj)
		{
			return (string)obj.GetValue(TargetProperty);
		}

		public static void SetTarget(DependencyObject obj, string value)
		{
			obj.SetValue(TargetProperty, value);
		}

		// Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TargetProperty =
			DependencyProperty.RegisterAttached("Target", typeof(string), typeof(ReportRunner), new UIPropertyMetadata("", new PropertyChangedCallback(RunReport)));

		static void RunReport(DependencyObject depo, DependencyPropertyChangedEventArgs depa)
		{
			double w = 0;
			double h = 0;
			var doc = depo as FixedDocument;
			var mainWindowViewModel = doc.DataContext as MainWindowViewModel;
			if (mainWindowViewModel == null)
			{
				throw new InvalidCastException(String.Format("FixedDocument DataContext is not of type {0}", typeof(MainWindowViewModel)));
			}

			if (null != doc && null != mainWindowViewModel.ReportDefinition)
			{

				var ctx = new ReportDefinitionContext();
				var size = depa.NewValue as string;
				var tokens = size.Split('x');
				w = double.Parse(tokens[0], CultureInfo.InvariantCulture);
				h = double.Parse(tokens[1], CultureInfo.InvariantCulture);
				int pageCount = 0;

				
				foreach (var item in mainWindowViewModel.ReportDefinition)
				{
					var obj = item.GetDataContext(ctx);
					foreach (var dc in SmartEnumerate(obj))
					{
						SetThreadLocale(dc);

						foreach (var page in item.CreatePages(w, h, ctx))
						{
							AttachDC(dc, page, ctx);
							AppendPage(doc, w, h, page, ++pageCount);
						}
					}
				}

			}

		}

		public static void SetContext(DependencyObject obj, object value)
		{
			obj.SetValue(ContextProperty, value);
		}

		// Using a DependencyProperty as the backing store for Context.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ContextProperty =
			DependencyProperty.RegisterAttached("Context", typeof(object), typeof(ReportRunner), new PropertyMetadata(null));


		/// <summary>
		/// 
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="page"></param>
		/// <param name="paraIndex">Index inside a group of pages</param>
		public static void AttachDC(object dc, UIElement page, ReportDefinitionContext ctx)
		{
			var fe = page as FrameworkElement;
			if (null != fe)
			{
				fe.DataContext = dc;
				foreach (var res in fe.Resources.Values)
				{
					var ig = res as IGroupIndexTarget;
					if (null != ig)
					{
						if (ig.Index == 0)
						{
							var gi = Group.GetIndex(page);
							ig.Index = gi;
						}
					}
					ConventionBind(res, dc);
				}
			}
		}

		private static void ConventionBind(object res, object dc)
		{
			if (res.GetType().Name.EndsWith("Model"))
			{
				foreach (PropertyInfo pi in dc.GetType().GetProperties())
				{
					var target = res.GetType().GetProperty(pi.Name);
					if (null != target && target.CanWrite && target.PropertyType.IsAssignableFrom(pi.PropertyType))
					{
						var data = pi.GetValue(dc, null);
						target.SetValue(res, data, null);
					}
				}
			}
		}

		public static void SetThreadLocale(object dc)
		{
			Thread.SetData(runnerContextSlot, dc);
		}

		public static object GetThreadLocale()
        {
			return Thread.GetData(runnerContextSlot);
		}

		private static IEnumerable SmartEnumerate(object obj)
		{
			if (obj is string || !(obj is IEnumerable))
			{
				yield return obj;
			}
			else
			{
				foreach (var a in obj as IEnumerable)
				{
					yield return a;
				}
			}
		}

		private static void AppendPage(FixedDocument doc, double w, double h, UIElement view, int pageCount)
		{
			var model = ViewModelLocator.LocateForView(view);
			if (null != model)
				ViewModelBinder.Bind(model, view, null);
			var page = PageBuilder.Create(w, h, view);
			var r = (view as FrameworkElement).TryFindResource("PageNo") as PageNumberViewModel;
			if (null != r)
			{
				r.Page = pageCount;
				r.TotalPages = pageCount;
			}
			doc.Pages.Add(page);
		}
	}
}
