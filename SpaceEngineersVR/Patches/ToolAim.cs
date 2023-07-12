using HarmonyLib;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SpaceEngineersVR.Plugin;
using System;
using System.Reflection;
using VRage.Game.Entity;
using VRageMath;

namespace SpaceEngineersVR.Patches;

[Util.InitialiseOnStart]
public static class ToolAim
{
	static ToolAim()
	{
		Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacterWeaponPositionComponent), "UpdateGraphicalWeaponPosition"), prefix: new HarmonyMethod(typeof(ToolAim), nameof(Prefix_UpdateGraphicalWeaponPosition)));
		Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacterWeaponPositionComponent), "UpdateLogicalWeaponPosition"), prefix: new HarmonyMethod(typeof(ToolAim), nameof(Prefix_UpdateLogicalWeaponPosition)));
		Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacter), "GetAimedPointFromCamera"), prefix: new HarmonyMethod(typeof(ToolAim), nameof(Prefix_GetAimedPointFromCamera)));
		Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacter), "UpdateShootDirection"), prefix: new HarmonyMethod(typeof(ToolAim), nameof(Prefix_UpdateShootDirection)));
		Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacter), "UpdateDynamicRange"), transpiler: Common.Transpiler_ReplaceHeadMatrixWithHandMatrix_Harmony);

		Logger.Info("Applied harmony game injections for tool aiming.");
	}

	private static readonly MethodInfo GraphicalPositionWorldSetter = AccessTools.PropertySetter(typeof(MyCharacterWeaponPositionComponent), nameof(MyCharacterWeaponPositionComponent.GraphicalPositionWorld));
	private static bool Prefix_UpdateGraphicalWeaponPosition(MyCharacterWeaponPositionComponent __instance)
	{
		MatrixD handWorld = Common.GetHandMatrix(__instance.Character, true);
		GraphicalPositionWorldSetter.Invoke(__instance, new object[] { handWorld.Translation });
		((MyEntity)__instance.Character.CurrentWeapon).WorldMatrix = handWorld;
		return false;
	}

	private static readonly MethodInfo SetLogicalPositionLocalSpace = AccessTools.PropertySetter(typeof(MyCharacterWeaponPositionComponent), nameof(MyCharacterWeaponPositionComponent.LogicalPositionLocalSpace));
	private static readonly MethodInfo SetLogicalPositionWorld      = AccessTools.PropertySetter(typeof(MyCharacterWeaponPositionComponent), nameof(MyCharacterWeaponPositionComponent.LogicalPositionWorld     ));
	private static readonly MethodInfo SetLogicalOrientationWorld   = AccessTools.PropertySetter(typeof(MyCharacterWeaponPositionComponent), nameof(MyCharacterWeaponPositionComponent.LogicalOrientationWorld  ));
	private static readonly MethodInfo SetLogicalCrosshairPoint     = AccessTools.PropertySetter(typeof(MyCharacterWeaponPositionComponent), nameof(MyCharacterWeaponPositionComponent.LogicalCrosshairPoint    ));
	private static bool Prefix_UpdateLogicalWeaponPosition(MyCharacterWeaponPositionComponent __instance)
	{
		Player.Components.VRBodyComponent.Hand? hand = Common.GetActiveHand(__instance.Character);
		if (hand == null)
			return true;

		Vector3D toolPos = hand.Value.world.Translation;
		SetLogicalPositionLocalSpace.Invoke(__instance, new object[] { (Vector3D)hand.Value.local.Translation });
		SetLogicalPositionWorld.Invoke(__instance, new object[] { toolPos });

		//The rest is roughly unchanged from decompiled code

		Vector3D shootDirection = __instance.Character.ShootDirection;
		SetLogicalOrientationWorld.Invoke(__instance, new object[] { shootDirection });
		SetLogicalCrosshairPoint.Invoke(__instance, new object[] { toolPos + shootDirection * 2000.0 });

		if (__instance.Character.CurrentWeapon != null)
		{
			if (__instance.Character.CurrentWeapon is MyEngineerToolBase myEngineerToolBase)
			{
				myEngineerToolBase.UpdateSensorPosition();
			}
			else if (__instance.Character.CurrentWeapon is MyHandDrill myHandDrill)
			{
				myHandDrill.WorldPositionChanged(null);
			}
		}

		return false;
	}

	private static bool Prefix_GetAimedPointFromCamera(MyCharacter __instance, ref Vector3D __result)
	{
		Player.Components.VRBodyComponent.Hand? hand = Common.GetActiveHand(__instance);
		if (hand == null)
			return true;

		MatrixD handMatrix = hand.Value.world;
		Vector3D forward = handMatrix.Forward;
		Vector3D position = handMatrix.Translation;

		if (MySession.Static.ControlledEntity == __instance && __instance.CurrentWeapon != null)
		{
			float num = Math.Min(__instance.CurrentWeapon.MaximumShotLength, 100f);
			MyPhysics.HitInfo? hitInfo = MyPhysics.CastRay(position, position + forward * (double)num, 30);
			if (hitInfo != null)
			{
				__result = hitInfo.Value.Position;
				Util.Util.DrawDebugLine(position, __result, Color.Yellow);
				return false;
			}
		}

		__result = (__instance.WeaponPosition != null) ? (__instance.WeaponPosition.LogicalPositionWorld + forward * 25000.0) : (position + forward * 25000.0);
		Util.Util.DrawDebugLine(position, __result, Color.Red);
		return false;
	}

	private static bool Prefix_UpdateShootDirection(ref bool shootStraight)
	{
		shootStraight = false;
		return true;
	}
}
