using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace CSharpCaliburnMicro1.Helpers
{
    internal class VisualSaver
    {
		public static void Save(Visual v, int width, int height, string file, Brush background)
		{
			RenderTargetBitmap bmp = new RenderTargetBitmap(
						width, height, 96, 96, PixelFormats.Pbgra32);

			Rectangle vRect = new Rectangle();
			vRect.Width = width;
			vRect.Height = height;
			vRect.Fill = background;
			vRect.Arrange(new Rect(0, 0, vRect.Width, vRect.Height));

			bmp.Render(vRect);
			bmp.Render(v);

			PngBitmapEncoder png = new PngBitmapEncoder();
			png.Frames.Add(BitmapFrame.Create(bmp));

			using (Stream stm = File.Create(file))
			{
				png.Save(stm);
			}
		}
		static int debugFile;
		public static ImageSource HiResSave(Viewport3D v3d)
		{

			Viewport3DVisual visual3d = VisualTreeHelper.GetParent(
					v3d.Children[0]) as Viewport3DVisual;

			Rect rect = visual3d.Viewport;

			// Scale dimensions from 96 dpi to 600 dpi.
			double scale = 600 / 96;

			RenderTargetBitmap bitmap = new RenderTargetBitmap((int)(scale * (rect.Width + 1)),
															   (int)(scale * (rect.Height + 1)),
															   scale * 96,
															   scale * 96, PixelFormats.Default);
			bitmap.Render(visual3d);

			PngBitmapEncoder png = new PngBitmapEncoder();
			png.Frames.Add(BitmapFrame.Create(bitmap));


			BitmapImage bitmapImage = new BitmapImage();
			debugFile++;
			//using(var file = new FileStream(debugFile.ToString()+".png",FileMode.Create,FileAccess.ReadWrite))
			using (var stream = new MemoryStream())
			{
				png.Save(stream);
				//png.Save(file);
				stream.Seek(0, SeekOrigin.Begin);

				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
			}

			return bitmapImage;
		}
		static BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
		{
			BitmapImage bi = new BitmapImage();
			MemoryStream ms = new MemoryStream();
			{
				bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				ms.Position = 0;
				bi.CacheOption = BitmapCacheOption.OnLoad;
				bi.BeginInit();
				bi.StreamSource = ms;
				bi.EndInit();
			}
			return bi;
		}

		public static ImageSource HiResGLSave(Viewport3D v3d, Brush[] palette, List<string> tempFiles)
		{

			Viewport3DVisual visual3d = VisualTreeHelper.GetParent(
					v3d.Children[0]) as Viewport3DVisual;

			Rect rect = visual3d.Viewport;

			// Scale dimensions from 96 dpi to 600 dpi.
			double scale = 600 / 96;

			ModelVisual3D model = v3d.Children[0] as ModelVisual3D;

			List<MeshGeometry3D> meshes = new List<MeshGeometry3D>();
			foreach (var piece in (model.Content as Model3DGroup).Children)
			{
				var geo = piece as GeometryModel3D;
				if (null != geo)
				{
					meshes.Add(geo.Geometry as MeshGeometry3D);
				}
			}

			var ret = new BitmapImage();
			using (Software3dRender render = new Software3dRender(palette, (int)(scale * (rect.Width + 1)), (int)(scale * (rect.Height + 1)), +40, -227))
			{
				var lookAt = (v3d.Camera as PerspectiveCamera).LookDirection;
				var up = (v3d.Camera as PerspectiveCamera).UpDirection;
				var fov = (v3d.Camera as PerspectiveCamera).FieldOfView;
				var pos = (v3d.Camera as PerspectiveCamera).Position;

				render.LookAt = new Vector3D(lookAt.X, lookAt.Y, -lookAt.Z);
				render.Up = new Vector3D(up.X, up.Y, -up.Z);
				render.Fov = fov;// +.55;
				render.Position = new Vector3D(pos.X, pos.Y, -pos.Z);

				var bmp = render.Render(meshes.ToArray());

				var tmp = PathExtension.GetTempFileWithExtension("tmp");
				tempFiles.Add(tmp);

				bmp.SetResolution(600, 600);
				bmp.Save(tmp, System.Drawing.Imaging.ImageFormat.Png);

				ret.BeginInit();
				ret.UriSource = new Uri(string.Format("file://{0}", tmp), UriKind.Absolute);
				ret.EndInit();
				bmp.Dispose();
			}
			return ret;
		}
	}
}
