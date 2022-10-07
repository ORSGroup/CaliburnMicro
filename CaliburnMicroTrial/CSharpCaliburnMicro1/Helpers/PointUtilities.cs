using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpCaliburnMicro1.Helpers._3DTools;

namespace CSharpCaliburnMicro1.Helpers
{
	public class PointUtilities
	{
		public static Point Get2DPoint(Point3D p3d, Viewport3D vp)
		{
			bool TransformationResultOK;
			Viewport3DVisual vp3Dv = VisualTreeHelper.GetParent(vp.Children[0]) as Viewport3DVisual;
			Matrix3D m = MathUtils.TryWorldToViewportTransform(vp3Dv, out TransformationResultOK);

			if (!TransformationResultOK)
			{
				return new Point(0, 0);
			}
			Point3D pb = m.Transform(p3d);
			Point p2d = new Point(pb.X, pb.Y);
			return p2d;
		}
		public static Point Petzold2dPoint(Point3D point3D, Viewport3D viewPort)
		{
			double screenX = 0d, screenY = 0d;

			// Camera is defined in XAML as:
			//        <Viewport3D.Camera>
			//             <PerspectiveCamera Position="0,0,800" LookDirection="0,0,-1" />
			//        </Viewport3D.Camera>

			PerspectiveCamera cam = viewPort.Camera as PerspectiveCamera;

			// Translate input point using camera position
			double inputX = point3D.X - cam.Position.X;
			double inputY = point3D.Y - cam.Position.Y;
			double inputZ = point3D.Z - cam.Position.Z;

			double aspectRatio = viewPort.ActualWidth / viewPort.ActualHeight;

			// Apply projection to X and Y
			screenX = inputX / (-inputZ * Math.Tan(cam.FieldOfView / 2));

			screenY = (inputY * aspectRatio) / (-inputZ * Math.Tan(cam.FieldOfView / 2));

			// Convert to screen coordinates
			screenX = screenX * viewPort.ActualWidth;

			screenY = screenY * viewPort.ActualHeight;


			// Additional, currently unused, projection scaling factors
			/*
			double xScale = 1 / Math.Tan(Math.PI * cam.FieldOfView / 360);
			double yScale = aspectRatio * xScale;

			double zFar = cam.FarPlaneDistance;
			double zNear = cam.NearPlaneDistance;

			double zScale = zFar == Double.PositiveInfinity ? -1 : zFar / (zNear - zFar);
			double zOffset = zNear * zScale;

			*/

			return new Point(screenX, screenY);
		}
	}
}
