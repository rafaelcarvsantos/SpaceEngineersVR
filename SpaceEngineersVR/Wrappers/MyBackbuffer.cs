using HarmonyLib;
using SharpDX.Direct3D11;
using System;
using System.Reflection;

namespace SpaceEngineersVR.Wrappers
{
	public class MyBackbuffer
	{
		public object instance { get; }

		static MyBackbuffer()
		{
			Type t = AccessTools.TypeByName("VRage.Render11.Resources.MyBackbuffer");
			ReleaseInfo = t.GetMethod("Release", BindingFlags.Public | BindingFlags.Instance);
			ResourceInfo = t.GetProperty("Resource", BindingFlags.Public | BindingFlags.Instance);
			RtvInfo = t.GetProperty("Rtv", BindingFlags.Public | BindingFlags.Instance);
		}

		public MyBackbuffer(object instance)
		{
			this.instance = instance;
		}

		private static readonly MethodInfo ReleaseInfo;
		public void Release()
		{
			ReleaseInfo.Invoke(instance, new object[0]);
		}

		private static readonly PropertyInfo ResourceInfo;
		public Resource resource => (Resource)ResourceInfo.GetValue(instance, new object[0]);

		private static readonly PropertyInfo RtvInfo;
		public static void SetBackbufferValues(MyBackbuffer instance, object targetInstance)
		{
			RtvInfo.SetValue(targetInstance, instance.rtv);
		}
		public RenderTargetView rtv => (RenderTargetView)RtvInfo.GetValue(instance);
	}
}
