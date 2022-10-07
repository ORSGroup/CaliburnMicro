using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using CSharpCaliburnMicro1.ViewModels;
using System.Windows.Media;
using CSharpCaliburnMicro1.Helpers;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using System.Collections;
using CSharpCaliburnMicro1.ViewInterfaces;

namespace CSharpCaliburnMicro1.Behaviors
{
    public class PieChart3d
    {
        static readonly double STARTANGLE = 0;
        static readonly double THICKNESS = .6;
        static readonly Point3D CENTER = new Point3D() { X = 0, Y = 0, Z = 0 };
        static readonly double RADIUS = 3;
        static readonly int WIRELOWEXTRA = 40;

        static PieChart3d()
        {
            foreach (var brush in Palette)
            {
                if (brush.CanFreeze)
                    brush.Freeze();
            }
        }

        public static Brush[] Palette = new Brush[]
        {
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#846542"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF51FF"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#465CDB"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1DE030"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F7B81B"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66CCCC"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA980"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9696"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF96"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80FF80"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E786E7"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"))
            ,new SolidColorBrush((Color)ColorConverter.ConvertFromString("#73E2E8"))

        };


        // Using a DependencyProperty as the backing store for PresenterCanvas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PresenterCanvasProperty =
            DependencyProperty.RegisterAttached("PresenterCanvas", typeof(string), typeof(PieChart3d), new UIPropertyMetadata(""));
        public static string GetPresenterCanvas(DependencyObject obj)
        {
            return (string)obj.GetValue(PresenterCanvasProperty);
        }
        public static void SetPresenterCanvas(DependencyObject obj, string value)
        {
            obj.SetValue(PresenterCanvasProperty, value);
        }

        // Using a DependencyProperty as the backing store for JointOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JointOffsetProperty =
            DependencyProperty.RegisterAttached("JointOffset", typeof(string), typeof(PieChart3d), new FrameworkPropertyMetadata("0,0", FrameworkPropertyMetadataOptions.Inherits));
        public static string GetJointOffset(DependencyObject obj)
        {
            return (string)obj.GetValue(JointOffsetProperty);
        }
        public static void SetJointOffset(DependencyObject obj, string value)
        {
            obj.SetValue(JointOffsetProperty, value);
        }

        // Using a DependencyProperty as the backing store for SliceItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliceItemProperty =
            DependencyProperty.RegisterAttached("SliceItem", typeof(IPieChartSlice), typeof(PieChart3d), new UIPropertyMetadata(null));
        public static IPieChartSlice GetSliceItem(DependencyObject obj)
        {
            return (IPieChartSlice)obj.GetValue(SliceItemProperty);
        }
        public static void SetSliceItem(DependencyObject obj, IPieChartSlice value)
        {
            obj.SetValue(SliceItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for JointLineCreated.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JointLineCreatedProperty =
            DependencyProperty.RegisterAttached("JointLineCreated", typeof(bool), typeof(PieChart3d), new UIPropertyMetadata(false));
        public static bool GetJointLineCreated(DependencyObject obj)
        {
            return (bool)obj.GetValue(JointLineCreatedProperty);
        }
        public static void SetJointLineCreated(DependencyObject obj, bool value)
        {
            obj.SetValue(JointLineCreatedProperty, value);
        }

        // Using a DependencyProperty as the backing store for ExitLines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExitLinesProperty =
            DependencyProperty.RegisterAttached("ExitLines", typeof(Dictionary<IPieChartSlice, Line[]>), typeof(PieChart3d), new UIPropertyMetadata(null));
        public static Dictionary<IPieChartSlice, Line[]> GetExitLines(DependencyObject obj)
        {
            return (Dictionary<IPieChartSlice, Line[]>)obj.GetValue(ExitLinesProperty);
        }
        public static void SetExitLines(DependencyObject obj, Dictionary<IPieChartSlice, Line[]> value)
        {
            obj.SetValue(ExitLinesProperty, value);
        }

        // Using a DependencyProperty as the backing store for TWodLayerUpdated.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TWodLayerUpdatedProperty =
            DependencyProperty.RegisterAttached("TWodLayerUpdated", typeof(bool), typeof(PieChart3d), new UIPropertyMetadata(false));
        public static bool GetTWodLayerUpdated(DependencyObject obj)
        {
            return (bool)obj.GetValue(TWodLayerUpdatedProperty);
        }
        public static void SetTWodLayerUpdated(DependencyObject obj, bool value)
        {
            obj.SetValue(TWodLayerUpdatedProperty, value);
        }

        // Using a DependencyProperty as the backing store for HMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HMarginProperty =
            DependencyProperty.RegisterAttached("HMargin", typeof(int), typeof(PieChart3d), new UIPropertyMetadata(0));
        public static int GetHMargin(DependencyObject obj)
        {
            return (int)obj.GetValue(HMarginProperty);
        }
        public static void SetHMargin(DependencyObject obj, int value)
        {
            obj.SetValue(HMarginProperty, value);
        }

        // Using a DependencyProperty as the backing store for VMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMarginProperty =
            DependencyProperty.RegisterAttached("VMargin", typeof(int), typeof(PieChart3d), new UIPropertyMetadata(0));
        public static int GetVMargin(DependencyObject obj)
        {
            return (int)obj.GetValue(VMarginProperty);
        }
        public static void SetVMargin(DependencyObject obj, int value)
        {
            obj.SetValue(VMarginProperty, value);
        }

        // Using a DependencyProperty as the backing store for TwoDLayer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TwoDLayerProperty =
            DependencyProperty.RegisterAttached("TwoDLayer", typeof(string), typeof(PieChart3d), new UIPropertyMetadata(""));
        public static string GetTwoDLayer(DependencyObject obj)
        {
            return (string)obj.GetValue(TwoDLayerProperty);
        }
        public static void SetTwoDLayer(DependencyObject obj, string value)
        {
            obj.SetValue(TwoDLayerProperty, value);
        }

        // Using a DependencyProperty as the backing store for Presenter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PresenterProperty =
            DependencyProperty.RegisterAttached("Presenter", typeof(string), typeof(PieChart3d), new UIPropertyMetadata(""));
        public static string GetPresenter(DependencyObject obj)
        {
            return (string)obj.GetValue(PresenterProperty);
        }
        public static void SetPresenter(DependencyObject obj, string value)
        {
            obj.SetValue(PresenterProperty, value);
        }

        // Using a DependencyProperty as the backing store for WriteNameOnLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WriteNameOnLabelProperty =
            DependencyProperty.RegisterAttached("WriteNameOnLabel", typeof(bool), typeof(PieChart3d), new UIPropertyMetadata(false));
        public static bool GetWriteNameOnLabel(DependencyObject obj)
        {
            return (bool)obj.GetValue(WriteNameOnLabelProperty);
        }
        public static void SetWriteNameOnLabel(DependencyObject obj, bool value)
        {
            obj.SetValue(WriteNameOnLabelProperty, value);
        }

        // Using a DependencyProperty as the backing store for ExternalLabels.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExternalLabelsProperty =
            DependencyProperty.RegisterAttached("ExternalLabels", typeof(bool), typeof(PieChart3d), new UIPropertyMetadata(false));
        public static bool GetExternalLabels(DependencyObject obj)
        {
            return (bool)obj.GetValue(ExternalLabelsProperty);
        }
        public static void SetExternalLabels(DependencyObject obj, bool value)
        {
            obj.SetValue(ExternalLabelsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Mesh.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeshProperty =
            DependencyProperty.RegisterAttached("Mesh", typeof(string), typeof(PieChart3d), new UIPropertyMetadata("", new PropertyChangedCallback(MeshChanged)));
        public static string GetMesh(DependencyObject obj)
        {
            return (string)obj.GetValue(MeshProperty);
        }
        public static void SetMesh(DependencyObject obj, string value)
        {
            obj.SetValue(MeshProperty, value);
        }

        static void MeshChanged(DependencyObject depo, DependencyPropertyChangedEventArgs depa)
        {

            (depo as FrameworkElement).DataContextChanged += (s, e) =>
            {

                var mesh = (depo as FrameworkElement).FindName(depa.NewValue as string);
                if (null != mesh)
                {
                    CreatePie(GetSliceList(e.NewValue),
                                mesh as Model3DGroup,
                                depo as Viewport3D,
                                STARTANGLE,
                                THICKNESS,
                                CENTER,
                                RADIUS);
                }
            };
            (depo as FrameworkElement).LayoutUpdated += (s, e) =>
            {
                bool wired = !String.IsNullOrEmpty(GetPresenter(depo));
                bool writeNameOnLabel = GetWriteNameOnLabel(depo);
                bool externalLabels = GetExternalLabels(depo);

                double[] ylist = new double[0];
                //place the pie chart exit lines
                Viewport3DVisual vp3Dv = VisualTreeHelper.GetParent(
                    (depo as Viewport3D).Children[0]) as Viewport3DVisual;
                var vp = vp3Dv.Viewport;
                if (!vp.IsEmpty && (depo as FrameworkElement).DataContext != null && GetTWodLayerUpdated(depo) == false)
                {
                    SetTWodLayerUpdated(depo, true);
                    Clear2dLayer(depo as Viewport3D);
                    var newList = Handle2dLayer(depo as Viewport3D, out ylist, STARTANGLE, THICKNESS, CENTER, RADIUS, wired, writeNameOnLabel, externalLabels);
                    BoundReorderedList(depo as Viewport3D, newList);
                }
                //join with the data presenter
                if (GetTWodLayerUpdated(depo) == true && GetJointLineCreated(depo) == false)
                {
                    var datap = GetPresenterCanvas(depo);
                    SetJointLineCreated(depo, true);
                    if (!string.IsNullOrEmpty(datap))
                    {
                        Canvas c = InternalTreeHelper.GetRootUserControl(depo as FrameworkElement).FindName(datap) as Canvas;
                        if (null != c)
                        {
                            HandleConnections(depo, c, ylist);
                        }
                    }
                }
            };
        }

        private static IList<IPieChartSlice> GetSliceList(object slicesCollection)
        {
            var iList = (slicesCollection as IList).Cast<IPieChartSlice>().ToList();
            // Rimuovo eventuali slices nulle
            var purgedList = iList.Where(x => x.Percentage > 0).ToList();
            return purgedList;
        }

        private static Vector GetJointPointOffset(DependencyObject obj)
        {
            var s = GetJointOffset(obj);
            var tokens = s.Split(',');
            return new Vector(double.Parse(tokens[0], CultureInfo.InvariantCulture), double.Parse(tokens[1], CultureInfo.InvariantCulture));
        }

        private static void HandleConnections(DependencyObject depo, Canvas c, double[] ylist)
        {
            var master = InternalTreeHelper.GetRootUserControl(c);
            var target = master.FindName(GetPresenter(depo)) as FrameworkElement;
            var exitLines = GetExitLines(depo);

            var targetOffset = VisualOffset.From(c, target);

            //handle joint on target side
            target.LayoutUpdated += (s, e) =>
            {
                var ic = target as ItemsControl;
                ItemsPresenter ip = InternalTreeHelper.GetVisualChild<ItemsPresenter>(ic);
                var panel = VisualTreeHelper.GetChild(ip, 0) as Panel;
                foreach (FrameworkElement single in panel.Children)
                {
                    var slice = single.DataContext as IPieChartSlice;
                    var exit = exitLines[slice];
                    var joint = c.Children.OfType<Line>().Where(l => GetSliceItem(l) == slice).Single();
                    var ptOffset = GetJointPointOffset(joint);
                    joint.Y2 = VisualTreeHelper.GetOffset(single).Y + single.ActualHeight + ptOffset.Y;
                    //optimize lines if possible

                    Optimize(exit, ylist, joint);

                }

            };

            foreach (var el in exitLines.Keys)
            {
                var offset = VisualOffset.From(c, depo as Visual);

                var lines = exitLines[el];
                Line l = new Line();
                l.StrokeThickness = 1;
                l.Stroke = c.FindResource("TotalBorder") as Brush;// Brushes.Black;

                l.X1 = lines.Last().X2 + offset.X - GetHMargin(depo);
                l.Y1 = lines.Last().Y2 + offset.Y - GetVMargin(depo);
                l.X2 = l.X1 + targetOffset.X;
                l.Y2 = l.Y1;
                SetSliceItem(l, el);
                c.Children.Add(l);
            }
        }

        private static void Optimize(Line[] exit, double[] ylist, Line joint)
        {
            var slope = joint.Y2 - joint.Y1;

            if (exit.Length == 3)
            {
                if (exit[2].Y2 + slope <= 0)
                    slope = -exit[2].Y2;
                int currOrder = Array.IndexOf(ylist, exit.Last().Y2);

                var modified = ylist.Select(u => u).ToArray();
                if (currOrder != -1)
                {
                    modified[currOrder] += slope;
                    int neworder = modified.OrderBy(u => u).ToList().IndexOf(exit.Last().Y2 + slope);
                    if (neworder == currOrder // non intersecting modification does not change the point order
                        && Math.Sign(exit[1].Y2 - exit[1].Y1) == Math.Sign(exit[1].Y2 + slope - exit[1].Y1) //and direction of exit does not change
                        )
                    {

                        exit[2].Y2 += slope;
                        joint.Y1 += slope;
                        exit[2].Y1 += slope;
                        exit[1].Y2 += slope;
                    }
                }
            }
        }

        private static IList<IPieChartSlice> Handle2dLayer(Viewport3D viewport, out double[] ylist, double startAngle, double thickness, Point3D center, double radius, bool wired, bool writeNameOnLabel, bool externalLabels)
        {
            var iList = GetSliceList(viewport.DataContext);
            int i = 0;
            var canvas = Get2dLayer(viewport);

            var wireFactory = WireStrategyFactory.Create(iList, startAngle,
                                                        canvas.ActualWidth, viewport.ActualHeight + GetVMargin(viewport) + WIRELOWEXTRA, GetVMargin(viewport));

            List<KeyValuePair<double, IPieChartSlice>> geoOrdered = new List<KeyValuePair<double, IPieChartSlice>>();
            Dictionary<IPieChartSlice, Line[]> exitLines = new Dictionary<IPieChartSlice, Line[]>();

            IDictionary<string, SliceInfo> sliceInfos = GetSliceInformations(iList, startAngle, thickness);
            foreach (var k in iList)
            {
                i++;

                //add labels to 2d associated layer
                var tb = PlaceLabel(k, viewport, sliceInfos[k.Name].Thickness, sliceInfos[k.Name].Angle, center, radius, writeNameOnLabel, externalLabels);
                canvas.Children.Add(tb);

                var wireStartegy = wireFactory.GetStrategy(k);
                var lines = wireStartegy.Wire(sliceInfos[k.Name].Angle + k.Percentage.ToAngleSpan() / 2, viewport, center, Point3D.Add(center, new Vector3D(0, sliceInfos[k.Name].Thickness, 0)), radius);
                if (lines.Count() > 0)
                {
                    geoOrdered.Add(new KeyValuePair<double, IPieChartSlice>(lines.Last().Y2, k));
                    exitLines[k] = lines.ToArray();
                }

                if (wired)
                {
                    //add wires to 2d associated layer
                    foreach (var l in lines)
                    {
                        canvas.Children.Add(l);
                    }
                }
            }
            //attach the exit lines to the object for further use
            SetExitLines(viewport, exitLines);
            var ret = geoOrdered.OrderBy(u => u.Key);
            ylist = ret.Select(k => k.Key).ToArray();
            return ret.Select(u => u.Value).ToList();
        }

        private static void Clear2dLayer(Viewport3D viewport3D)
        {
            var canvas = Get2dLayer(viewport3D);
            if (canvas != null)
            {
                canvas.Children.Clear();
            }
        }

        private static void BoundReorderedList(Viewport3D viewport3D, IList<IPieChartSlice> newList)
        {
            var root = InternalTreeHelper.GetRootUserControl(viewport3D);
            var target = root.FindName(GetPresenter(viewport3D)) as FrameworkElement;
            if (null != target)
            {
                var control = target as ItemsControl;
                if (null != control)
                {
                    control.ItemsSource = newList;
                }
            }
        }

        private static void CreatePie(IList<IPieChartSlice> iList,
                                      Model3DGroup model3DGroup,
                                      Viewport3D viewport,
                                      double startAngle,
                                      double thickness,
                                      Point3D center,
                                      double radius)
        {
            int i = 0;
            IDictionary<string, SliceInfo> sliceInfos = GetSliceInformations(iList, startAngle, thickness);
            foreach (var k in iList)
            {
                i++;
                Brush br = Palette[i % Palette.Length];
                model3DGroup.Children.Add(Create3dSlice(k, sliceInfos[k.Name].Thickness, sliceInfos[k.Name].Angle, center, radius, br));
            }
            return;
        }

        private struct SliceInfo
        {
            internal double Angle;
            internal double Thickness;
        }

        private static IDictionary<string, SliceInfo> GetSliceInformations(IList<IPieChartSlice> iList, double startAngle, double thickness)
        {
            double maxAscendantDegree = Math.PI * 1.5;

            IDictionary<double, string> angleSliceNameDict = new Dictionary<double, string>();
            List<double> startAngles = new List<double>();
            double lastValue = startAngle;
            foreach (var k in iList)
            {
                if (angleSliceNameDict.ContainsKey(lastValue))
                    throw new Exception(string.Format("GetSliceInformations error: item {0} already present in angleSliceNameDict. IPieChartSlice info: Name = {1} - Percentage = {2}", lastValue, k.Name, k.Percentage));
                angleSliceNameDict.Add(lastValue, k.Name);
                lastValue += k.Percentage.ToAngleSpan();
                startAngles.Add(lastValue);
            }
            startAngles.Remove(lastValue);

            int ascCount = startAngles.Count(x => x <= maxAscendantDegree);
            int descCount = startAngles.Count - ascCount;

            IDictionary<string, SliceInfo> retVal = new Dictionary<string, SliceInfo>();
            if (angleSliceNameDict.Count == 0)
                return retVal;

            KeyValuePair<double, string> firstVal = angleSliceNameDict.First();
            retVal.Add(firstVal.Value, new SliceInfo() { Angle = firstVal.Key, Thickness = thickness });

            double maxthickness = Math.Max(ascCount, descCount + 1) * thickness * 0.66;

            double ascDelta = ascCount == 0 ? 0.0 : maxthickness / ascCount;
            double descDelta = descCount == 0 ? 0.0 : maxthickness / (descCount + 1);

            IList<double> ascAngles = startAngles.Where(x => x <= maxAscendantDegree).OrderBy(x => x).ToList();
            for (int idx = 0; idx < ascAngles.Count(); idx++)
            {
                double angle = ascAngles[idx];
                Double actualthickness = thickness + ascDelta * (idx + 1);
                retVal.Add(angleSliceNameDict[angle], new SliceInfo() { Angle = angle, Thickness = actualthickness });
            }

            IList<double> descAngles = startAngles.Where(x => x > maxAscendantDegree).OrderByDescending(x => x).ToList();
            for (int idx = 0; idx < descAngles.Count(); idx++)
            {
                double angle = descAngles[idx];
                Double actualthickness = thickness + descDelta * (idx + 1);
                retVal.Add(angleSliceNameDict[angle], new SliceInfo() { Angle = angle, Thickness = actualthickness });
            }
            return retVal;
        }

        private static Canvas Get2dLayer(Viewport3D viewport)
        {
            var root = (viewport.Parent as FrameworkElement).Parent as FrameworkElement;
            var canvas = root.FindName(GetTwoDLayer(viewport)) as Canvas;
            return canvas;
        }

        private static TextBlock PlaceLabel(IPieChartSlice k, Viewport3D viewport, double thickness, double startAngle, Point3D center, double radius, bool writeName, bool externalLabels)
        {
            double radiusResolved;
            if (externalLabels)
            {
                radiusResolved = radius;
            }
            else
            {
                radiusResolved = radius * 0.5;
            }

            var angle = startAngle + k.Percentage.ToAngleSpan() / 2;
            Quadrants labelQuadrant = GetQuadrantFromAngle(angle);
            var pt = Point3D.Add(center, new Vector3D(radiusResolved * Math.Cos(angle), thickness, radiusResolved * Math.Sin(angle)));
            var pt2d = Point.Add(PointUtilities.Get2DPoint(pt, viewport), GetMarginAdjust(viewport));
            var fnt = viewport.FindResource("BlackCaptionUltraBold8");

            var tb = new TextBlock();
            //tb.Background = Brushes.Yellow;
            //tb.FontFamily = new FontFamily("Arial");
            //tb.FontWeight = FontWeights.Bold;
            //tb.FontSize = 18;
            tb.Style = fnt as Style;
            //tb.Margin = new Thickness(4);
            tb.Text = String.Format("{0:0.00}%", k.Percentage);
            if (writeName)
            {
                tb.Text = String.Format("{0} {1}", k.Name, tb.Text);
            }

            if (!externalLabels)
            {
                Canvas.SetLeft(tb, pt2d.X - 15);
                Canvas.SetTop(tb, pt2d.Y - 2);
            }
            else
            {
                tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                tb.Arrange(new Rect(tb.DesiredSize));
                switch (labelQuadrant)
                {
                    case Quadrants.I_low:
                    case Quadrants.IV_up:
                        Canvas.SetLeft(tb, pt2d.X + 5);
                        Canvas.SetTop(tb, pt2d.Y - tb.ActualHeight / 2);
                        break;
                    case Quadrants.I_up:
                        Canvas.SetLeft(tb, pt2d.X + 2);
                        Canvas.SetTop(tb, pt2d.Y - tb.ActualHeight);
                        break;
                    case Quadrants.II_low:
                    case Quadrants.III_up:
                        Canvas.SetLeft(tb, pt2d.X - tb.ActualWidth - 5);
                        Canvas.SetTop(tb, pt2d.Y - tb.ActualHeight / 2);
                        break;
                    case Quadrants.II_up:
                        Canvas.SetLeft(tb, pt2d.X - tb.ActualWidth - 5);
                        Canvas.SetTop(tb, pt2d.Y - tb.ActualHeight - 3);
                        break;
                    case Quadrants.III_low:
                        Canvas.SetLeft(tb, pt2d.X - tb.ActualWidth - 5);
                        Canvas.SetTop(tb, pt2d.Y + 3);
                        break;
                    case Quadrants.IV_low:
                        Canvas.SetLeft(tb, pt2d.X + 2);
                        Canvas.SetTop(tb, pt2d.Y + 15);
                        break;
                    default:
                        break;
                }
            }

            return tb;
        }

        private static Quadrants GetQuadrantFromAngle(double angle)
        {
            if (angle < 0.25 * Math.PI)
            {
                return Quadrants.IV_up;
            }
            if (angle < 0.5 * Math.PI)
            {
                return Quadrants.IV_low;
            }
            if (angle < 0.75 * Math.PI)
            {
                return Quadrants.III_low;
            }
            if (angle < Math.PI)
            {
                return Quadrants.III_up;
            }
            if (angle < 1.25 * Math.PI)
            {
                return Quadrants.II_low;
            }
            if (angle < 1.5 * Math.PI)
            {
                return Quadrants.II_up;
            }
            if (angle < 1.75 * Math.PI)
            {
                return Quadrants.I_up;
            }
            return Quadrants.I_low;
        }

        private static Vector GetMarginAdjust(Viewport3D viewport)
        {
            return new Vector(GetHMargin(viewport), GetVMargin(viewport));
        }

        public static Point MarginAdjust(Viewport3D viewport, Point p)
        {
            return Point.Add(p, GetMarginAdjust(viewport));
        }
        //static int remove = 0;
        private static readonly double centerYTranslation = 0.7;
        private static Model3D Create3dSlice(IPieChartSlice k, double thickness, double startAngle, Point3D center, double radius, Brush br)
        {

            var model = new GeometryModel3D();
            var geometry = new MeshGeometry3D();

            geometry.Positions = new Point3DCollection();
            geometry.TriangleIndices = new Int32Collection();

            //we use an eager mesh to obtain automatically
            //sharper cylinders contours

            Point3D centerUp = Point3D.Add(center, new Vector3D(0, thickness, 0));
            Point3D[] lowerSlicePoints = GetContourPoints(center, radius, startAngle, k.Percentage.ToAngleSpan());
            Point3D[] upperSlicePoints = GetContourPoints(centerUp, radius, startAngle, k.Percentage.ToAngleSpan());
            int borderIndex = 0;
            borderIndex += MakeLowerFan(geometry, lowerSlicePoints, center);
            borderIndex += MakeUpperFan(geometry, upperSlicePoints, centerUp);

            borderIndex += MakeClosing(geometry, lowerSlicePoints, upperSlicePoints, center, centerUp);
            borderIndex += MakeStarting(geometry, lowerSlicePoints, upperSlicePoints, center, centerUp);

            MakeEdge(geometry, lowerSlicePoints, upperSlicePoints);

            model.Geometry = geometry;
            var mat = new MaterialGroup();

            mat.Children.Add(new DiffuseMaterial() { Brush = br });
            mat.Children.Add(new SpecularMaterial() { Color = Colors.White, Brush = Brushes.White, SpecularPower = 16 });
            model.Material = mat;
            //XamlWriter.Save(geometry, new FileStream(System.IO.Path.ChangeExtension(remove.ToString(),"xaml"), FileMode.Create, FileAccess.Write));
            //remove++;

            Raw3DData.SetData(geometry, new BorderData() { BorderIndex = borderIndex, CenterLow = center, CenterUp = centerUp });
            return model;
        }

        private static void MakeEdge(MeshGeometry3D geometry, Point3D[] lowerSlicePoints, Point3D[] upperSlicePoints)
        {
            var low = geometry.Positions.Count;
            geometry.Positions.AddRange(lowerSlicePoints);
            var up = geometry.Positions.Count;
            geometry.Positions.AddRange(upperSlicePoints);
            for (int i = 0; i < lowerSlicePoints.Length - 1; ++i)
            {
                //low,up,low+1
                geometry.TriangleIndices.Add(low + i);
                geometry.TriangleIndices.Add(up + i);
                geometry.TriangleIndices.Add(low + 1 + i);

                //up+1,up,low+1
                geometry.TriangleIndices.Add(up + i);
                geometry.TriangleIndices.Add(up + 1 + i);
                geometry.TriangleIndices.Add(low + i + 1);
            }
        }

        private static int MakeStarting(MeshGeometry3D geometry, Point3D[] lowerSlicePoints, Point3D[] upperSlicePoints, Point3D centerLow, Point3D centerUp)
        {
            var zero = geometry.Positions.Count;

            geometry.Positions.Add(centerLow);
            geometry.Positions.Add(Point3D.Add(centerUp, new Vector3D(0, centerYTranslation, 1)));
            geometry.Positions.Add(lowerSlicePoints.First());
            geometry.Positions.Add(upperSlicePoints.First());

            geometry.TriangleIndices.Add(zero);
            geometry.TriangleIndices.Add(1 + zero);
            geometry.TriangleIndices.Add(3 + zero);

            geometry.TriangleIndices.Add(zero);
            geometry.TriangleIndices.Add(3 + zero);
            geometry.TriangleIndices.Add(2 + zero);
            return 2;
        }

        private static int MakeClosing(MeshGeometry3D geometry, Point3D[] lowerSlicePoints, Point3D[] upperSlicePoints, Point3D centerLow, Point3D centerUp)
        {
            var zero = geometry.Positions.Count;

            geometry.Positions.Add(centerLow);
            geometry.Positions.Add(Point3D.Add(centerUp, new Vector3D(0, centerYTranslation, 1)));
            geometry.Positions.Add(lowerSlicePoints.Last());
            geometry.Positions.Add(upperSlicePoints.Last());

            geometry.TriangleIndices.Add(zero);
            geometry.TriangleIndices.Add(3 + zero);
            geometry.TriangleIndices.Add(1 + zero);

            geometry.TriangleIndices.Add(zero);
            geometry.TriangleIndices.Add(2 + zero);
            geometry.TriangleIndices.Add(3 + zero);
            return 2;

        }

        private static int MakeUpperFan(MeshGeometry3D geometry, Point3D[] points, Point3D center)
        {
            int zero = geometry.Positions.Count;
            var ptx = center;
            ptx.Offset(0, centerYTranslation, 1);
            geometry.Positions.Add(ptx);
            geometry.Positions.AddRange(points);
            int idx = 0;
            for (int i = 0; i < points.Length - 1; ++i)
            {
                geometry.TriangleIndices.Add(zero);
                geometry.TriangleIndices.Add(zero + i + 2);
                geometry.TriangleIndices.Add(zero + i + 1);
                ++idx;
            }
            return idx;
        }

        private static int MakeLowerFan(MeshGeometry3D geometry, Point3D[] points, Point3D center)
        {
            int zero = geometry.Positions.Count;
            geometry.Positions.Add(center);
            geometry.Positions.AddRange(points);
            int idx = 0;
            for (int i = 0; i < points.Length - 1; ++i)
            {
                geometry.TriangleIndices.Add(zero);
                geometry.TriangleIndices.Add(zero + i + 1);
                geometry.TriangleIndices.Add(zero + i + 2);
                ++idx;
            }
            return idx;
        }

        private static Point3D[] GetContourPoints(Point3D center, double radius, double startAngle, double sweep)
        {
            List<Point3D> l = new List<Point3D>();

            var count = Math.Max(2, (int)(sweep / (Math.PI / 25)));
            var dstep = sweep / count;
            for (int i = 0; i <= count; ++i)
            {
                var angle = startAngle + (i * dstep);
                l.Add(new Point3D(center.X + radius * Math.Cos(angle), center.Y, center.Z + radius * Math.Sin(angle)));
            }
            return l.ToArray();
        }

    }


    internal enum Quadrants
    {
        I_low,
        I_up,
        II_low,
        II_up,
        III_low,
        III_up,
        IV_low,
        IV_up
    }
}
