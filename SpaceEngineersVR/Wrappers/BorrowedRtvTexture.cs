using HarmonyLib;
using SharpDX.Direct3D11;
using System;
using System.Reflection;

namespace SpaceEngineersVR.Wrappers
{
	public class BorrowedRtvTexture
	{
		public object instance { get; }

		static BorrowedRtvTexture()
		{
			Type t = AccessTools.TypeByName("VRage.Render11.Resources.Textures.MyBorrowedTexture");
			ReleaseInfo = t.GetMethod("Release", BindingFlags.Public | BindingFlags.Instance);

			Type myBorrowedRtvTexture = AccessTools.TypeByName("VRage.Render11.Resources.Textures.MyBorrowedRtvTexture");
			ResourceInfo = myBorrowedRtvTexture.GetProperty("Resource", BindingFlags.Public | BindingFlags.Instance);
		}

		public BorrowedRtvTexture(object instance)
		{
			this.instance = instance;
		}

		private static readonly MethodInfo ReleaseInfo;
		public void Release()
		{
			ReleaseInfo.Invoke(instance, new object[0]);
		}

		private static readonly PropertyInfo ResourceInfo;
		public Texture2D resource => (Texture2D)ResourceInfo.GetValue(instance, new object[0]);

	}
}
