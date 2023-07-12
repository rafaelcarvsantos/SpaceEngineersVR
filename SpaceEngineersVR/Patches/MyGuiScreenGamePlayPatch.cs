using HarmonyLib;
using Sandbox.Game.Gui;
using SpaceEngineersVR.Player.Components;

namespace SpaceEngineersVR.Patches;

[HarmonyPatch(typeof(MyGuiScreenGamePlay))]
public static class MyGuiScreenGamePlayPatch
{
	[HarmonyPrefix]
	[HarmonyPatch(nameof(MyGuiScreenGamePlay.MoveAndRotatePlayerOrCamera))]
	public static bool Prefix()
	{
		if (VRMovementComponent.UsingControllerMovement)
			return false;

		return Plugin.Common.Config.enableKeyboardAndMouseControls;
	}
}