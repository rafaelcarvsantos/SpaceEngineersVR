using HarmonyLib;
using Sandbox.Game.Entities.Character;
using SpaceEngineersVR.Patches.TranspilerHelper;
using SpaceEngineersVR.Player.Components;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using VRageMath;

namespace SpaceEngineersVR.Patches;

public static class Common
{
	public static Harmony Harmony => Plugin.Common.Plugin.Harmony;

	public static readonly HarmonyMethod Prefix_DoNotRun_Harmony = new(typeof(Common), nameof(Prefix_DoNotRun_Method));
	public static readonly MethodInfo Prefix_DoNotRun_Info = typeof(Common).GetMethod(nameof(Prefix_DoNotRun_Method));

	public static bool Prefix_DoNotRun_Method()
	{
		return false;
	}



	public static VRBodyComponent.Hand? GetActiveHand(MyCharacter character)
	{
		VRBodyComponent body = character.Components?.Get<VRBodyComponent>();
		return body?.hands.Primary ?? body?.hands.Secondary;
	}



	public static readonly MethodInfo GetHandMatrix_Method = typeof(Common).GetMethod(nameof(GetHandMatrix));

	//Method to be used in transpiled code, designed to have the same signature as MyCharacter.GetHeadMatrix for easy replacement
	public static MatrixD GetHandMatrix(MyCharacter character, bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false, bool preferLocalOverSync = false)
	{
		return GetActiveHand(character)?.world ?? character.GetHeadMatrix(includeY, includeX, forceHeadAnim, forceHeadBone, preferLocalOverSync);
	}


	public static readonly HarmonyMethod Transpiler_ReplaceHeadMatrixWithHandMatrix_Harmony = new(typeof(Common), nameof(Transpiler_ReplaceHeadMatrixWithHandMatrix));
	public static IEnumerable<CodeInstruction> Transpiler_ReplaceHeadMatrixWithHandMatrix(IEnumerable<CodeInstruction> instructions)
	{
		TranspilerHelper.TranspilerHelper code = new(instructions);
		ReplaceHeadMatrixWithHand(code);
		return code;
	}

	public static readonly MethodInfo GetHeadMatrix_Method = AccessTools.Method(typeof(MyCharacter), "GetHeadMatrix");
	public static void ReplaceHeadMatrixWithHand(TranspilerHelper.TranspilerHelper code)
	{
		foreach (CodeInstructionIndex key in code.FindEachReversed(inst => inst.Calls(GetHeadMatrix_Method)))
		{
			code.Replace(key, new CodeInstruction(OpCodes.Call, GetHandMatrix_Method));
		}
	}
}
