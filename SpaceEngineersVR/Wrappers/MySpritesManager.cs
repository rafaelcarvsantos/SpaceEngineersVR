using HarmonyLib;
using System;
using System.Reflection;

namespace SpaceEngineersVR.Wrappers
{
	public class MySpritesManager
	{
		public object instance { get; }

		static MySpritesManager()
		{
			Type t = AccessTools.TypeByName("VRage.Render11.Sprites.MySpritesManager");
			AcquireDrawMessagesInfo = AccessTools.Method(t, "AcquireDrawMessages");
			CloseDebugDrawMessagesInfo = AccessTools.Method(t, "CloseDebugDrawMessages");
		}

		public MySpritesManager(object instance)
		{
			this.instance = instance;
		}

		private static readonly MethodInfo AcquireDrawMessagesInfo;
		public object AcquireDrawMessages(string textureName)
		{
			return AcquireDrawMessagesInfo.Invoke(instance, new object[] { textureName });
		}

		private static readonly MethodInfo CloseDebugDrawMessagesInfo;
		public object CloseDebugDrawMessages()
		{
			return CloseDebugDrawMessagesInfo.Invoke(instance, null);
		}
	}
}
