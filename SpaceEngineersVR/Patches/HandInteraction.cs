using HarmonyLib;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.World;
using SpaceEngineersVR.Patches.TranspilerHelper;
using SpaceEngineersVR.Plugin;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using VRage.Game.Utils;

namespace SpaceEngineersVR.Patches
{
	[Util.InitialiseOnStart]
	public static class HandInteraction
	{
		static HandInteraction()
		{
			HarmonyMethod doDetectionPrefix = new HarmonyMethod(typeof(HandInteraction), nameof(Prefix_DoDetection));
			HarmonyMethod doDetectionTranspiler = new HarmonyMethod(typeof(HandInteraction), nameof(Transpiler_DoDetection));
			Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacterRaycastDetectorComponent), "DoDetection"), prefix: doDetectionPrefix, transpiler: doDetectionTranspiler);
			Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacterShapecastDetectorComponent), "DoDetection", parameters: new Type[] { typeof(bool) }), prefix: doDetectionPrefix, transpiler: doDetectionTranspiler);
			Common.Harmony.Patch(AccessTools.Method(typeof(MyCharacterShapecastDetectorComponent), "DoDetection", parameters: new Type[] { typeof(bool), typeof(bool) }), prefix: doDetectionPrefix, transpiler: doDetectionTranspiler);
			Common.Harmony.Patch(AccessTools.Method("Sandbox.Game.Entities.Character.Components.MyCharacterClosestDetectorComponent:DoDetection"), prefix: doDetectionPrefix, transpiler: doDetectionTranspiler);

			Logger.Info("Applied harmony game injections for hand interaction.");
		}


		private static void Prefix_DoDetection(ref bool useHead)
		{
			useHead = false; //if only it were this easy..
		}

		private static IEnumerable<CodeInstruction> Transpiler_DoDetection(IEnumerable<CodeInstruction> instructions)
		{
			TranspilerHelper.TranspilerHelper code = new TranspilerHelper.TranspilerHelper(instructions);

			Common.ReplaceHeadMatrixWithHand(code);

			foreach (CodeRange range in code.FindEachReversed(
				inst => inst.Calls(AccessTools.PropertyGetter(typeof(MySector), nameof(MySector.MainCamera))),
				inst => inst.LoadsField(AccessTools.Field(typeof(MyCamera), nameof(MyCamera.WorldMatrix)))))
			{
				CodeRange newRange = code.Insert(range.end, MoveLabels.MoveToInsertedCode, MoveBlocks.MoveToInsertedCode,
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(MyCharacterComponent), nameof(MyCharacterComponent.Character))),
					new CodeInstruction(OpCodes.Ldc_I4_0),
					new CodeInstruction(OpCodes.Ldc_I4_1),
					new CodeInstruction(OpCodes.Ldc_I4_0),
					new CodeInstruction(OpCodes.Ldc_I4_0),
					new CodeInstruction(OpCodes.Ldc_I4_0),
					new CodeInstruction(OpCodes.Call, Common.GetHandMatrix_Method));

				code.Remove(range, newRange.first);
			}

			return code;
		}
	}
}
