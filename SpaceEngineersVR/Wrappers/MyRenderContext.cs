using HarmonyLib;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Reflection;

namespace SpaceEngineersVR.Wrappers
{
	public class MyRenderContext
	{
		private readonly object instance;

		static MyRenderContext()
		{
			Type t = AccessTools.TypeByName("VRage.Render11.RenderContext.MyRenderContext");
			Type tIResource = AccessTools.TypeByName("VRage.Render11.Resources.IResource");

			CopyResourceInfo = AccessTools.Method(t, "CopyResource", new Type[] { tIResource, typeof(Resource) });
			MapSubresourceInfo = AccessTools.Method(t, "MapSubresource", new Type[] { typeof(Texture2D), typeof(int), typeof(int), typeof(MapMode), typeof(MapFlags), typeof(DataStream).MakeByRefType() });
			UnmapSubresourceInfo = AccessTools.Method(t, "UnmapSubresource", new Type[] { typeof(Resource), typeof(int) });
			DeviceContextInfo = AccessTools.Property(t, "DeviceContext");
		}

		public MyRenderContext(object instance)
		{
			this.instance = instance;

			//cleanUpCommandListsDel = (Action)Delegate.CreateDelegate(typeof(Action), AccessTools.Method(instance.GetType(), "CleanUpCommandLists"));
		}

		/*
		private readonly Action cleanUpCommandListsDel;
		public void CleanUpCommandLists()
		{
			cleanUpCommandListsDel.Invoke();
		}
		*/

		private static readonly MethodInfo CopyResourceInfo;
		public void CopyResource(object source, Resource destination)
		{
			CopyResourceInfo.Invoke(instance, new object[] { source, destination });
		}

		private static readonly MethodInfo MapSubresourceInfo;
		public DataBox MapSubresource(Texture2D resource, int mipSlice, int arraySlice, MapMode mode, MapFlags flags, out DataStream stream)
		{
			object[] args = new object[] { resource, mipSlice, arraySlice, mode, flags, null };
			DataBox result = (DataBox)MapSubresourceInfo.Invoke(instance, args);
			stream = (DataStream)args[5];
			return result;
		}

		private static readonly MethodInfo UnmapSubresourceInfo;
		public void UnmapSubresource(Resource resourceRef, int subresource)
		{
			UnmapSubresourceInfo.Invoke(instance, new object[] { resourceRef, subresource });
		}


		private static PropertyInfo DeviceContextInfo;
		public DeviceContext1 DeviceContext
		{
			get => (DeviceContext1)DeviceContextInfo.GetValue(instance);
			set => DeviceContextInfo.SetValue(instance, value);
		}

		public void ClearRtv(MyBackbuffer rtv, RawColor4 colorRGBA)
		{
			DeviceContext1 context = (DeviceContext1)DeviceContextInfo.GetValue(instance);
			context.ClearRenderTargetView(rtv.rtv, colorRGBA);
		}
	}
}
