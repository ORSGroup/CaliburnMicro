using Caliburn.Micro;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCaliburnMicro1.Behaviors;
using CSharpCaliburnMicro1.ViewInterfaces;
using CSharpCaliburnMicro1.ViewModels;

namespace CSharpCaliburnMicro1
{
    public abstract class ReportDefinitionBase : List<ReportDefinitionItem>
    {
        private Serilog.ILogger logger;

        public ReportDefinitionBase()
        {
            this.logger = Serilog.Log.Logger;
        }

        public virtual ImageSource FooterImage
        {
            get
            {
                return Images.footerImage;
            }
        }

        public virtual Color FeaturingColor
        {
            get
            {
                return Colors.Black;
            }
        }

        public virtual double SectionLabelWidth
        {
            get
            {
                return 48;
            }
        }

        public virtual PageOrientation PageOrientation
        {
            get
            {
                return PageOrientation.Landscape;
            }
        }

        public virtual ImageSource WatermarkImage
        {
            get { return Images.watermarkImage; }
        }
    }


    public class NullPageReportDefinition : ReportDefinitionItem
    {
        System.Action render;

        public NullPageReportDefinition(System.Action render)
        {
            this.render = render;
        }

        public override object GetDataContext(ReportDefinitionContext ctx)
        {
            return null;
        }

        public override UIElement[] CreatePages(double w, double h, ReportDefinitionContext ctx)
        {
            render();
            return new UIElement[0];
        }
    }

    public class ReportDefinitionPageGroup : ReportDefinitionItem
    {
        ReportDefinitionItem[] items;
        Func<ReportDefinitionContext, object> dataBinder;
        public ReportDefinitionPageGroup(Func<ReportDefinitionContext, object> dataBinder, params ReportDefinitionItem[] items)
        {
            this.items = items;
            this.dataBinder = dataBinder;
        }

        public override object GetDataContext(ReportDefinitionContext ctx)
        {
            return dataBinder(ctx);
        }

        public override UIElement[] CreatePages(double w, double h, ReportDefinitionContext ctx)
        {
            List<UIElement> pages = new List<UIElement>();
            int idx = 0;
            foreach (var item in items)
            {
                ctx.GroupIndex = ++idx;
                var newpages = item.CreatePages(w, h, ctx);
                AttachGroupIndex(newpages, ctx);
                pages.AddRange(newpages);
            }
            return pages.ToArray();
        }

        private void AttachGroupIndex(IEnumerable<UIElement> newpages, ReportDefinitionContext ctx)
        {
            foreach (var page in newpages)
            {
                Group.SetIndex(page, ctx.GroupIndex);
            }
        }
    }

    public class ReportDefinitionSimplePage<TPage> : ReportDefinitionItem
    {
        Func<ReportDefinitionContext, object> dataBinder;
        public ReportDefinitionSimplePage(Func<ReportDefinitionContext, object> dataBinder)
        {
            this.dataBinder = dataBinder;
        }

        public override UIElement[] CreatePages(double w, double h, ReportDefinitionContext ctx)
        {
            var view = ViewLocator.GetOrCreateViewType(typeof(TPage));
            //First page binding for owerflovable items
            if (Overflowable.GetCanOverflow(view))
            {
                return HandleOverflow(typeof(TPage), view);
            }
            else
            {
                return new UIElement[] { view };
            }
        }

        private static UIElement[] HandleOverflow(Type t, UIElement view)
        {
            List<UIElement> pages = new List<UIElement>();
            var vmName = Overflowable.GetViewModel(view);

            var data = IoC.Get<object>(Overflowable.GetModel(view) + "Model") as IEnumerable;
            if (null != data)
            {
                var target = (view as FrameworkElement).FindResource(vmName) as IMultipageTarget;
                (view as FrameworkElement).Resources["OverflowVisibility"] = Visibility.Visible;

                var dd = data.Cast<object>();
                object defaultGroup = new NullGrouper();
                string breaker = Overflowable.GetBreaker(view);
                bool first = true;
                foreach (var d in dd.GroupBy(g => string.IsNullOrEmpty(breaker) ? defaultGroup : GetGroup(breaker, g)))
                {
                    var pageSize = Overflowable.GetCountPerFirstPage(view);
                    int run = 0;//Overflowable.GetCountPerFirstPage(view);
                    if (!first)
                    {
                        view = ViewLocator.GetOrCreateViewType(t);
                        (view as FrameworkElement).Resources["OverflowVisibility"] = Visibility.Collapsed;
                        target = (view as FrameworkElement).FindResource(vmName) as IMultipageTarget;
                    }
                    var count = d.Count();
                    int max = 0;
                    if (null != target)
                    {
                        int counter = 0;
                        foreach (var item in d)
                        {
                            if (max < pageSize)
                            {
                                target.Add(item);
                            }
                            else
                            {
                                break;

                            }
                            ICanBeFakeRow row = item as ICanBeFakeRow;
                            if (row == null || !row.IsFake)
                                max++;

                            counter++;
                        }
                        run = counter;
                    }

                    if (max == pageSize && max != count)
                        target.Continues = true;

                    if (max > 0)
                    {
                        pages.Add(view);
                        AttachBreakerModel(view, d.Key);
                    }

                    pageSize = Overflowable.GetCountPerPage(view);
                    //append page till finish
                    while (run < count)
                    {
                        view = ViewLocator.GetOrCreateViewType(t);
                        (view as FrameworkElement).Resources["OverflowVisibility"] = Visibility.Collapsed;
                        target = (view as FrameworkElement).FindResource(vmName) as IMultipageTarget;

                        max = 0;
                        int counter = 0;
                        foreach (var item in d.Skip(run))
                        {
                            if (max < pageSize)
                            {
                                target.Add(item);
                            }
                            else
                            {
                                break;

                            }
                            ICanBeFakeRow row = item as ICanBeFakeRow;
                            if (row == null || !row.IsFake)
                                max++;

                            counter++;
                        }
                        run += counter;
                        if (max == pageSize && run != count)
                            target.Continues = true;

                        if (max > 0)
                        {
                            pages.Add(view);
                            AttachBreakerModel(view, d.Key);
                        }
                    }
                    first = false;
                }
            }
            return pages.ToArray();
        }

        private static void AttachBreakerModel(UIElement view, object key)
        {
            //breaker is a fixed resource name
            var vm = (view as FrameworkElement).TryFindResource("Breaker") as BreakerViewModel;
            if (null != vm)
            {
                vm.GroupValue = key;
            }
        }

        private static object GetGroup(string breaker, object g)
        {
            var pi = g.GetType().GetProperty(breaker);
            if (null == pi)
                return null;
            else
            {
                return pi.GetValue(g, null);
            }

        }


        public override object GetDataContext(ReportDefinitionContext ctx)
        {
            return dataBinder(ctx);
        }
    }

    public class NullGrouper
    {
        public override string ToString()
        {
            return "";
        }
    }

    public abstract class ReportDefinitionItem
    {
        abstract public object GetDataContext(ReportDefinitionContext ctx);
        abstract public UIElement[] CreatePages(double w, double h, ReportDefinitionContext ctx);
    }

    public class ReportDefinitionContext
    {
        public int GroupIndex { get; set; }
    }

    public enum PageOrientation
    {
        Portrait,
        Landscape
    }

    public class Group
    {
        public static int GetIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(IndexProperty);
        }

        public static void SetIndex(DependencyObject obj, int value)
        {
            obj.SetValue(IndexProperty, value);
        }

        // Using a DependencyProperty as the backing store for Index.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.RegisterAttached("Index", typeof(int), typeof(Group), new PropertyMetadata(0));


    }
}
