using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace CSharpCaliburnMicro1.Helpers
{
	static class Point3DColelctionExtension
	{
		public static void AddRange(this Point3DCollection coll, IEnumerable<Point3D> points)
		{
			foreach (var p in points)
				coll.Add(p);
		}
	}
}
