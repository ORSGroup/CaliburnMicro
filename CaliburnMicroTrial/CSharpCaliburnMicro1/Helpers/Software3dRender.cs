using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Media.Media3D;
using CSharpCaliburnMicro1.Behaviors;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using System;

namespace CSharpCaliburnMicro1.Helpers
{
	public class Software3dRender : IDisposable
	{
		public Vector3D LookAt { get; set; }
		public Vector3D Up { get; set; }
		public Vector3D Position { get; set; }
		public double Fov { get; set; }
		public float NearPlane { get; set; }
		public float FarPlane { get; set; }
		OpenGL gl;

		int width; int height;




		SharpGL.SceneGraph.Assets.Material[] bounchOfMaterials;

		void CreateMaterials(System.Windows.Media.Brush[] palette)
		{
			int i = 0;
			bounchOfMaterials = new SharpGL.SceneGraph.Assets.Material[palette.Length];
			foreach (var k in palette)
			{
				bounchOfMaterials[i] = new SharpGL.SceneGraph.Assets.Material();
				System.Windows.Media.SolidColorBrush brush = palette[i] as System.Windows.Media.SolidColorBrush;
				bounchOfMaterials[i].Ambient = Color.FromArgb(brush.Color.R, brush.Color.G, brush.Color.B);
				bounchOfMaterials[i].Diffuse = Color.FromArgb(brush.Color.R, brush.Color.G, brush.Color.B);
				bounchOfMaterials[i].Specular = Color.FromArgb(255, 255, 255);
				bounchOfMaterials[i].Shininess = 25f;
				i++;
			}
		}


		public Software3dRender(System.Windows.Media.Brush[] palette, int width, int height, int offsetX, int offsetY)
		{
			this.gl = new OpenGL();

			NearPlane = 0.1f;
			FarPlane = 1000.0f;
			//	Create OpenGL.
			//gl.Create(RenderContextType.DIBSection, width, height, 32, null);


			CreateMaterials(palette);
			gl.MakeCurrent();
			//  Set the most basic OpenGL styles.
			gl.ShadeModel(SharpGL.Enumerations.ShadeModel.Smooth);
			gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
			gl.ClearDepth(1.0f);
			gl.Enable(OpenGL.GL_DEPTH_TEST);
			gl.DepthFunc(OpenGL.GL_LEQUAL);
			gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);
			gl.PolygonMode(FaceMode.Front, PolygonMode.Filled);
			gl.Enable(OpenGL.GL_NORMALIZE);
			Lights();




			//	Set the viewport.
			gl.Viewport(offsetX, offsetY, width, height);
			this.width = width;
			this.height = height;

		}

		private void Lights()
		{
			float[] global_ambient = new float[] { 0.40f, 0.40f, 0.40f, 1.0f };     // Set Ambient Lighting To Fairly Dark Light (No Color)

			float[] light0pos = new float[] { 2.0f, 5.0f, 1.0f, 1.0f };     // Set The Light Direction
			float[] light0ambient = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };     // More Ambient Light
			float[] light0diffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };     // Set The Diffuse Light A Bit Brighter
			float[] light0specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };        // Fairly Bright Specular Lighting

			float[] light1pos = new float[] { 1.0f, -2.5f, -3f, 1.0f };     // Set The Light Direction
			float[] light1ambient = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };     // More Ambient Light
			float[] light1diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };     // Set The Diffuse Light A Bit Brighter
			float[] light1specular = new float[] { 5.0f, 5.0f, 5.0f, 1.0f };        // Fairly Bright Specular Lighting

			float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };            // And More Ambient Light
			gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);       // Set The Ambient Light Model
			gl.Enable(OpenGL.GL_LIGHTING);                                      // Enable Lighting

			gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);       // Set The Global Ambient Light Model
			gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);              // Set The Lights Position
			gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);           // Set The Ambient Light
			gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);           // Set The Diffuse Light
			gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);         // Set Up Specular Lighting
			gl.Enable(OpenGL.GL_LIGHT0);                                        // Enable Light0

			gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);              // Set The Lights Position
			gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);           // Set The Ambient Light
			gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);           // Set The Diffuse Light
			gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);         // Set Up Specular Lighting
			gl.Enable(OpenGL.GL_LIGHT1);                                        // Enable LIGHT1

			gl.Enable(OpenGL.GL_CULL_FACE);
		}

		public Bitmap Render(MeshGeometry3D[] meshes)
		{
			gl.MakeCurrent();
			PlaceCamera();
			gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);  // Clear The Screen And The Depth Buffer
			Draw(meshes);
			return ExtractBmp();
		}

		private Bitmap ExtractBmp()
		{
			Bitmap bmp = new Bitmap(width, height);
			System.Drawing.Imaging.BitmapData data =
				bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly,
							 System.Drawing.Imaging.PixelFormat.Format24bppRgb);


			gl.ReadPixels(0, 0, width, height, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE, data.Scan0);
			bmp.UnlockBits(data);
			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
			return bmp;
		}

		private void Draw(MeshGeometry3D[] meshes)
		{

			gl.MatrixMode(MatrixMode.Modelview);

			gl.LoadIdentity();
			gl.Rotate(-30, 0.0f, 0.0f);



			int k = 0;
			foreach (var mesh in meshes)
			{
				var borderData = Raw3DData.GetData(mesh);

				k++;
				var mat = bounchOfMaterials[k % bounchOfMaterials.Length];
				mat.Push(gl);
				gl.Begin(OpenGL.GL_TRIANGLES);
				int j = 0;
				for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
				{

					var v1x = -mesh.Positions[mesh.TriangleIndices[i]].X;
					var v1y = mesh.Positions[mesh.TriangleIndices[i]].Y;
					var v1z = -mesh.Positions[mesh.TriangleIndices[i]].Z;


					var v2x = -mesh.Positions[mesh.TriangleIndices[i + 1]].X;
					var v2y = mesh.Positions[mesh.TriangleIndices[i + 1]].Y;
					var v2z = -mesh.Positions[mesh.TriangleIndices[i + 1]].Z;

					var v3x = -mesh.Positions[mesh.TriangleIndices[i + 2]].X;
					var v3y = mesh.Positions[mesh.TriangleIndices[i + 2]].Y;
					var v3z = -mesh.Positions[mesh.TriangleIndices[i + 2]].Z;

					if (j < borderData.BorderIndex)
					{
						FlatFace(v2x, v1x, v2y, v1y, v2z, v1z, v3x, v3y, v3z);
					}
					else
					{
						SmoothFace(v2x, v1x, v2y, v1y, v2z, v1z, v3x, v3y, v3z, borderData);
					}
					j++;
				}
				gl.End();
				mat.Pop(gl);
			}
			gl.Flush();
		}

		private void FlatFace(double v2x, double v1x, double v2y, double v1y, double v2z, double v1z, double v3x, double v3y, double v3z)
		{
			Vector3D v1 = new Vector3D(v2x - v1x, v2y - v1y, v2z - v1z);
			Vector3D v2 = new Vector3D(v3x - v1x, v3y - v1y, v3z - v1z);

			var normal = Vector3D.CrossProduct(v1, v2);
			gl.Normal(normal.X, normal.Y, normal.Z);

			gl.Vertex(v1x,
					  v1y,
					  v1z
				);
			gl.Vertex(v2x,
					  v2y,
					  v2z
				);
			gl.Vertex(v3x,
					  v3y,
					  v3z
				);
		}

		private double MaxCoord(double c1, double c2, double c3)
		{
			return Math.Max(c1, Math.Max(c2, c3));
		}
		private double MinCoord(double c1, double c2, double c3)
		{
			return Math.Min(c1, Math.Min(c2, c3));
		}

		private void SmoothFace(double v2x, double v1x, double v2y, double v1y, double v2z, double v1z, double v3x, double v3y, double v3z, BorderData data)
		{
			Vector3D v1 = new Vector3D(v2x - v1x, v2y - v1y, v2z - v1z);
			Vector3D v2 = new Vector3D(v3x - v1x, v3y - v1y, v3z - v1z);


			var avgY = (MinCoord(v1y, v2y, v3y) + MaxCoord(v1y, v2y, v3y)) * .5;

			//var normal = Vector3D.CrossProduct(v1, v2);
			//gl.Normal(normal.X, normal.Y, normal.Z);
			Vector3D vNormal;
			vNormal = new Vector3D(v1x - data.CenterUp.X, v1y - avgY, v1z - data.CenterUp.Z);
			gl.Normal(vNormal.X, vNormal.Y, vNormal.Z);
			gl.Vertex(v1x,
					  v1y,
					  v1z
				);





			vNormal = new Vector3D(v2x - data.CenterUp.X, v2y - avgY, v2z - data.CenterUp.Z);
			gl.Normal(vNormal.X, vNormal.Y, vNormal.Z);


			gl.Vertex(v2x,
					  v2y,
					  v2z
				);

			vNormal = new Vector3D(v3x - data.CenterUp.X, v3y - avgY, v3z - data.CenterUp.Z);
			gl.Normal(vNormal.X, vNormal.Y, vNormal.Z);
			gl.Vertex(v3x,
					  v3y,
					  v3z
				);

		}

		private void PlaceCamera()
		{
			gl.MatrixMode(OpenGL.GL_PROJECTION);
			var camera = new LookAtCamera();
			camera.FieldOfView = Fov;
			camera.Target = LookAt.ToVertex();
			camera.Position = Position.ToVertex();
			camera.UpVector = Up.ToVertex();
			camera.Near = NearPlane;
			camera.Far = FarPlane;
			gl.LoadIdentity();
			camera.TransformProjectionMatrix(gl);
		}

		public void Dispose()
		{
			gl.RenderContextProvider.Destroy();
		}
	}


	internal static class Vector3dExt
	{
		public static Vertex ToVertex(this Vector3D v)
		{
			return new Vertex((float)v.X, (float)v.Y, (float)v.Z);
		}
	}
}
