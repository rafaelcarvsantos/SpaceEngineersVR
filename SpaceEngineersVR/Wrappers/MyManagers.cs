using HarmonyLib;
using System;
using System.Reflection;

namespace SpaceEngineersVR.Wrappers
{
	public static class MyManagers
	{
		static MyManagers()
		{
			Type t = AccessTools.TypeByName("VRage.Render11.Common.MyManagers");
			RwTexturesPoolInfo = t.GetField("RwTexturesPool", BindingFlags.Public | BindingFlags.Static);
			SpritesManagerInfo = t.GetField("SpritesManager", BindingFlags.Public | BindingFlags.Static);
		}

		private static readonly FieldInfo RwTexturesPoolInfo;
		public static MyBorrowedRwTextureManager RwTexturesPool => new MyBorrowedRwTextureManager(RwTexturesPoolInfo.GetValue(null));

		private static readonly FieldInfo SpritesManagerInfo;
		public static MySpritesManager SpritesManager => new MySpritesManager(SpritesManagerInfo.GetValue(null));
	}
}
