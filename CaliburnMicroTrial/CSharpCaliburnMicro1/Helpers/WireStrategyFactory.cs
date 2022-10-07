using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpCaliburnMicro1.ViewModels;
using CSharpCaliburnMicro1.Helpers;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpCaliburnMicro1.Behaviors;
using CSharpCaliburnMicro1.ViewInterfaces;

namespace CSharpCaliburnMicro1.Helpers
{
	interface IWireStrategy
	{
		IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius);
		WireStrategyContext Context { get; set; }
	}

	class WireStrategyContext
	{
		public int LineDownStraightOnCount { get; set; }
		public int LineUpStraightOnCount { get; set; }
		public int LineDownStraightOnUsed { get; set; }
		public int LineUpStraightOnUsed { get; set; }
		public double MaxX { get; set; }
		public double MaxY { get; set; }
		public double VMargin { get; set; }
	}

	class WireStrategyFactory
	{
		IEnumerable<IPieChartSlice> items;
		WireStrategyContext context;
		private WireStrategyFactory(IEnumerable<IPieChartSlice> items)
		{
			this.items = items;
			this.context = new WireStrategyContext();
		}


		Dictionary<object, Type> toFactorize = new Dictionary<object, Type>();

		public static WireStrategyFactory Create(IEnumerable<IPieChartSlice> items, double startangle, double width, double height, double VMargin)
		{
			var res = new WireStrategyFactory(items);
			var angle = startangle;
			res.context.MaxX = width;
			res.context.MaxY = height;
			res.context.VMargin = VMargin;
			foreach (var v in items)
			{
				var current = v.Percentage.ToAngleSpan();
				Type strategyType = res.GetTypeFor(angle + current / 2);
				if (strategyType == typeof(LineDownStraightOn))
					res.context.LineDownStraightOnCount++;
				if (strategyType == typeof(LineUpStraightOn))
					res.context.LineUpStraightOnCount++;
				res.toFactorize[v] = strategyType;
				angle += current;
			}
			return res;
		}

		public IWireStrategy GetStrategy(IPieChartSlice item)
		{
			IWireStrategy s = Activator.CreateInstance(toFactorize[item]) as IWireStrategy;
			s.Context = context;
			return s;
		}

		private Type GetTypeFor(double angle)
		{
			while (angle < 0)
				angle += 2 * Math.PI;
			if (angle >= 0 && angle < Math.PI / 2)
				return typeof(LowLineStraightOn);
			else
				if (angle >= Math.PI / 2 && angle < Math.PI)
				return typeof(LineDownStraightOn);
			else
					if (angle >= Math.PI && angle < Math.PI * 3 / 2)
				return typeof(LineUpStraightOn);
			else
				return typeof(UpLineStraightOn);
		}
	}

	abstract class WireStrategyBase : IWireStrategy
	{

		public abstract IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius);
		protected Line GetLine(List<Line> lines, Viewport3D viewport)
		{

			Line l1 = new Line();
			lines.Add(l1);
			l1.Stroke = viewport.FindResource("TotalBorder") as Brush;// Brushes.Black;
			l1.Fill = viewport.FindResource("TotalBorder") as Brush;// Brushes.Black;
			l1.StrokeThickness = 1;
			return l1;
		}

		public WireStrategyContext Context
		{
			get; set;

		}
	}

	class LowLineStraightOn : WireStrategyBase
	{
		public override IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius)
		{
			var ticklen = radius * 1.1;
			List<Line> lines = new List<Line>();
			//the tic line
			var l1 = GetLine(lines, viewport);
			var p1 = Point3D.Add(centerLow, new Vector3D(radius * Math.Cos(angle), 0, radius * Math.Sin(angle)));
			var p2 = Point3D.Add(centerLow, new Vector3D(ticklen * Math.Cos(angle), 0, ticklen * Math.Sin(angle)));
			var s1 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p1, viewport));
			var s2 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p2, viewport));
			l1.X1 = s1.X; l1.Y1 = s1.Y;
			l1.X2 = s2.X; l1.Y2 = s2.Y;

			//the border line
			var l2 = GetLine(lines, viewport);
			l2.X1 = s2.X; l2.Y1 = s2.Y;
			l2.X2 = Context.MaxX; l2.Y2 = s2.Y;

			return lines;
		}

	}
	class LineDownStraightOn : WireStrategyBase
	{
		public override IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius)
		{
			var ticklen = radius * 1.1;
			List<Line> lines = new List<Line>();
			//the tic line
			var l1 = GetLine(lines, viewport);
			var p1 = Point3D.Add(centerLow, new Vector3D(radius * Math.Cos(angle), 0, radius * Math.Sin(angle)));
			var p2 = Point3D.Add(centerLow, new Vector3D(ticklen * Math.Cos(angle), 0, ticklen * Math.Sin(angle)));
			var s1 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p1, viewport));
			var s2 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p2, viewport));
			l1.X1 = s1.X; l1.Y1 = s1.Y;
			l1.X2 = s2.X; l1.Y2 = s2.Y;
			//the downing line
			var l2 = GetLine(lines, viewport);
			l2.X1 = s2.X; l2.Y1 = s2.Y;
			l2.X2 = s2.X; l2.Y2 = GetNextY();
			// the border line;
			var l3 = GetLine(lines, viewport);
			l3.X1 = l2.X2; l3.Y1 = l2.Y2;
			l3.X2 = Context.MaxX; l3.Y2 = l2.Y2;

			return lines;
		}

		private double GetNextY()
		{
			var delta = Context.VMargin / Context.LineDownStraightOnCount;
			var ret = Context.MaxY - (Context.LineDownStraightOnCount - Context.LineDownStraightOnUsed) * delta - 1;
			Context.LineDownStraightOnUsed++;
			return ret;
		}

	}
	class LineUpStraightOn : WireStrategyBase
	{
		public override IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius)
		{
			var ticklen = radius * 1.1;
			List<Line> lines = new List<Line>();
			//the tic line
			var l1 = GetLine(lines, viewport);
			var p1 = Point3D.Add(centerUP, new Vector3D(radius * Math.Cos(angle), 0, radius * Math.Sin(angle)));
			var p2 = Point3D.Add(centerUP, new Vector3D(ticklen * Math.Cos(angle), 0, ticklen * Math.Sin(angle)));
			var s1 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p1, viewport));
			var s2 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p2, viewport));
			l1.X1 = s1.X; l1.Y1 = s1.Y;
			l1.X2 = s2.X; l1.Y2 = s2.Y;
			//the upping line
			var l2 = GetLine(lines, viewport);
			l2.X1 = s2.X; l2.Y1 = s2.Y;
			l2.X2 = s2.X; l2.Y2 = GetNextY();
			// the border line;
			var l3 = GetLine(lines, viewport);
			l3.X1 = l2.X2; l3.Y1 = l2.Y2;
			l3.X2 = Context.MaxX; l3.Y2 = l2.Y2;

			return lines;
		}
		private double GetNextY()
		{
			var delta = Context.VMargin / Context.LineUpStraightOnCount;
			var ret = Context.LineUpStraightOnUsed * delta + 1;
			Context.LineUpStraightOnUsed++;
			return ret;
		}

	}
	class UpLineStraightOn : WireStrategyBase
	{
		public override IEnumerable<Line> Wire(double angle, Viewport3D viewport, Point3D centerLow, Point3D centerUP, double radius)
		{
			var ticklen = radius * 1.1;
			List<Line> lines = new List<Line>();
			//the tic line
			var l1 = GetLine(lines, viewport);
			var p1 = Point3D.Add(centerUP, new Vector3D(radius * Math.Cos(angle), 0, radius * Math.Sin(angle)));
			var p2 = Point3D.Add(centerUP, new Vector3D(ticklen * Math.Cos(angle), 0, ticklen * Math.Sin(angle)));
			var s1 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p1, viewport));
			var s2 = PieChart3d.MarginAdjust(viewport, PointUtilities.Get2DPoint(p2, viewport));
			l1.X1 = s1.X; l1.Y1 = s1.Y;
			l1.X2 = s2.X; l1.Y2 = s2.Y;
			//the border line
			var l2 = GetLine(lines, viewport);
			l2.X1 = s2.X; l2.Y1 = s2.Y;
			l2.X2 = Context.MaxX; l2.Y2 = s2.Y;

			return lines;
		}

	}
}
