using DirectShowLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace SpaceEngineersVR.Util
{
	internal static class Util
	{
		private static MyStringId SQUARE = MyStringId.GetOrCompute("Square");

		public static void DrawDebugLine(Vector3D posA, Vector3D posB, Color color)
		{
			Vector4 colorVec = color;
			MySimpleObjectDraw.DrawLine(posA, posB, SQUARE, ref colorVec, 0.01f);
		}

		public static void DrawDebugRay(Vector3D pos, Vector3D dir, Color color)
		{
			Vector4 colorVec = color;
			MySimpleObjectDraw.DrawLine(pos, pos + dir * 10, SQUARE, ref colorVec, 0.01f);
		}

		public static void DrawDebugSphere(Vector3D pos, float radius, Color color)
		{
			MatrixD x = MatrixD.Identity;
			x.Translation = pos;
			MySimpleObjectDraw.DrawTransparentSphere(ref x, radius, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, SQUARE, SQUARE);
		}

		public static void DrawDebugMatrix(MatrixD matrix, string name = null)
		{
			Vector3D position = matrix.Translation;
			DrawDebugRay(position, matrix.Forward, Color.Red);
			DrawDebugRay(position, matrix.Left, Color.Green);
			DrawDebugRay(position, matrix.Up, Color.Blue);

			if(name != null)
				DrawDebugText(position, name);
		}

		public static void DrawDebugText(Vector3D pos, string text)
		{
			MyRenderProxy.DebugDrawText3D(pos, text, Color.White, 1f, true);
		}

		public static void ExecuteInMain(Action action, bool sync)
		{
			SynchronizationContext synchronization = SynchronizationContext.Current;
			if (synchronization != null)
			{
				if (sync)
				{
					synchronization.Send(_ => action(), null);
				}
				else
				{
					synchronization.Post(_ => action(), null);
				}
			}
			else
			{
				Task.Factory.StartNew(action);
			}
		}

		public static MatrixD MapViewToWorldMatrix(MatrixD view, MatrixD worldMatrix)
		{

			worldMatrix.Right = view.Right; //Vector3D.Lerp(worldMatrix.Right, view.Right, .5);
			worldMatrix.Forward = view.Forward; //Vector3D.Lerp(worldMatrix.Forward, view.Forward, .5);
			return worldMatrix;

		}

		public static double GetAngle(Vector3D one, Vector3D two, Vector3D up)
		{
			//360 code
			double angle = (Math.Acos(Vector3D.Dot(Vector3D.Normalize(one), Vector3D.Normalize(two))) * 180 / Math.PI);
			Vector3D cross = Vector3D.Cross(one, two);
			if (Vector3D.Dot(Vector3D.Normalize(up), cross) > 0)
			{
				angle += angle - 90;
			}
			return angle;
		}

		public static double GetAngleRadians(Vector3D one, Vector3D two, Vector3D up)
		{
			//360 code
			double angle = Math.Acos(Vector3D.Dot(Vector3D.Normalize(one), Vector3D.Normalize(two)));
			Vector3D cross = Vector3D.Cross(one, two);
			if (Vector3D.Dot(Vector3D.Normalize(up), cross) > 0)
			{
				angle += angle - 1;
			}
			return angle;
		}

		public static Matrix ZeroPitchAndRoll(this Matrix matrix)
		{
			Vector3 right = Vector3.Normalize(Vector3.Cross(matrix.Forward, Vector3.Up));
			Vector3 forward = Vector3.Cross(Vector3.Up, right);

			matrix.Up = Vector3.Up;
			matrix.Forward = forward;
			matrix.Right = right;
			return matrix;
		}

		public static void Orthogonalize(this Matrix matrix) //MatrixD method that Matrix is missing
		{
			Vector3 right = Vector3.Normalize(matrix.Right);
			Vector3 up = Vector3.Normalize(matrix.Up - right * matrix.Up.Dot(right));
			Vector3 backward = Vector3.Normalize(matrix.Backward - right * matrix.Backward.Dot(right) - up * matrix.Backward.Dot(up));
			matrix.Right = right;
			matrix.Up = up;
			matrix.Backward = backward;
		}

		public static bool IsNullOrEmpty<T>(this ICollection<T> self)
		{
			return self == null || self.Count == 0;
		}
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
		{
			return self == null || !self.Any();
		}
	}
}
