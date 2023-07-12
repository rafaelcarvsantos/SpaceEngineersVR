using HarmonyLib;
using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using System;

namespace SpaceEngineersVR.Patches;

[Util.InitialiseOnStart]
public static class FrameInjections
{
	public static bool DisablePresent = false;

	static FrameInjections()
	{
		Type t = AccessTools.TypeByName("VRageRender.MyRender11");

		Common.Harmony.Patch(AccessTools.Method(t, "Present"), new HarmonyMethod(typeof(FrameInjections), nameof(Prefix_Present)));

		Common.Harmony.Patch(AccessTools.Method(t, "DrawScene"), new HarmonyMethod(typeof(FrameInjections), nameof(Prefix_DrawScene)));

		Logger.Info("Applied harmony game injections for renderer.");
	}

	private static bool Prefix_DrawScene()
	{
		Player.Player.RenderUpdate();
		VRGUIManager.Draw();

		return true;
	}

	private static bool Prefix_Present()
	{
		return !DisablePresent;
	}
}