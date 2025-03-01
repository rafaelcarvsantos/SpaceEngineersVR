using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.World;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRageMath;
using VRageRender.Animations;

namespace SpaceEngineersVR.Player.Components
{
	[InitialiseOnStart]
	public class VRBodyComponent : MyCharacterComponent
	{
		public class ScalingMode
		{
			public ScalingMode(string name, string tooltip, Func<BodyCalibration, BodyCalibration, Matrix> method)
			{
				this.name = name;
				this.tooltip = tooltip;
				this.method = method;
			}

			public readonly string name;
			public readonly string tooltip;
			public readonly Func<BodyCalibration, BodyCalibration, Matrix> method;
		}

		public static readonly ScalingMode NoScaling = new ScalingMode(
			"None",
			"Does not scale the player. Shorter players may not be able to touch the in-game floor, taller players may experience stretching in the arms.",
			(player, character) =>
			{
				return Matrix.Identity;
			});
		public static readonly ScalingMode HeightScaling = new ScalingMode(
			"Height",
			"Scales the player such that the real world floor matches the in-game floor. Taller players may see stretching in the arms, shorter players may not be able to reach as far.",
			(player, character) =>
			{
				return Matrix.CreateScale(character.height / player.height);
			});
		public static readonly ScalingMode ArmSpanScaling = new ScalingMode(
			"Arm Span",
			"Scales the player such that your arm span matches the character's. You will not experience arm stretching or short reach, but the floor may not match up",
			(player, character) =>
			{
				return Matrix.CreateScale(character.height / player.height);
			});
		public static readonly List<ScalingMode> ScalingModes = new List<ScalingMode>()
		{
			NoScaling,
			HeightScaling,
			ArmSpanScaling,
		};

		public static readonly int DefaultScalingMode = 0;

		private static readonly Handed<Matrix> HandExtraTransforms = new Handed<Matrix>(
			                                        Matrix.CreateRotationZ( MathHelper.Pi / 2) * Matrix.CreateRotationX(MathHelper.Pi / 2),
			Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateRotationZ(-MathHelper.Pi / 2) * Matrix.CreateRotationX(MathHelper.Pi / 2));

		private static readonly Handed<FieldInfo> HandIndexFields = new Handed<FieldInfo>(
			HarmonyLib.AccessTools.Field(typeof(MyCharacter), "m_leftHandIKEndBone"),
			HarmonyLib.AccessTools.Field(typeof(MyCharacter), "m_rightHandIKEndBone"));

		private static readonly Handed<FieldInfo> ArmIKStartIndexFields = new Handed<FieldInfo>(
			HarmonyLib.AccessTools.Field(typeof(MyCharacter), "m_leftHandIKStartBone"),
			HarmonyLib.AccessTools.Field(typeof(MyCharacter), "m_rightHandIKStartBone"));

		private static readonly MethodInfo CalculateHandIK = HarmonyLib.AccessTools.Method(typeof(MyCharacter), "CalculateHandIK", new Type[]
		{
			typeof(int), //startBoneIndex
			typeof(int), //endBoneIndex
			typeof(MatrixD).MakeByRefType(), //targetTransform
		});

		static VRBodyComponent()
		{
			Logger.Debug("Creating VR Body Component");
			MySession.AfterLoading += GameLoaded;
		}

		private static void GameLoaded()
		{
			/*
			MySession.Static.CameraAttachedToChanged += (oldCamera, newCamera) =>
			{
				oldCamera?.Entity?.Components.Remove<VRBodyComponent>();
				if (newCamera.Entity != null && newCamera.Entity is MyCharacter character)
				{
					character.Components.Add(new VRBodyComponent());
				}
			};
			*/
		}

		public MatrixAndInvert playerToCharacter;
		public BodyCalibration characterCalibration;

		private Handed<int> handIndexes;
		private Handed<int> armIKStartIndexes;

		public struct Hand
		{
			public Hand(MatrixD world, Matrix local)
			{
				this.world = world;
				this.local = local;
			}

			public MatrixD world;
			public Matrix local;
		}
		public Handed<Hand?> hands { get; private set; }


		public override void OnAddedToScene()
		{
			Init();
		}

		public override void OnAddedToContainer()
		{
			base.OnAddedToContainer();

			NeedsUpdateBeforeSimulation = true;
			if (Character.InScene)
			{
				Init();
			}
		}

		private void Init()
		{
			Logger.Debug("Initalizing VR hands");

			//MyCharacterBone[] bones = Character.AnimationController.CharacterBones;

			//MyCharacterBone headBone = bones[Character.HeadBoneIndex];
			//Vector3 headPos = headBone.GetAbsoluteRigTransform().Translation;

			//characterCalibration.height = headPos.Y;

			//handIndexes = new Handed<int>(
			//	(int)HandIndexFields.left.GetValue(Character),
			//	(int)HandIndexFields.right.GetValue(Character));

			//armIKStartIndexes = new Handed<int>(
			//	(int)ArmIKStartIndexFields.left.GetValue(Character),
			//	(int)ArmIKStartIndexFields.right.GetValue(Character));

			//Handed<MyCharacterBone> handBones = new Handed<MyCharacterBone>(
			//	handIndexes.left >= 0 ? bones[handIndexes.left] : null,
			//	handIndexes.right >= 0 ? bones[handIndexes.right] : null);

			//Handed<MyCharacterBone> shoulders = new Handed<MyCharacterBone>(
			//	armIKStartIndexes.left >= 0 ? bones[armIKStartIndexes.left] : null,
			//	armIKStartIndexes.right >= 0 ? bones[armIKStartIndexes.right] : null);

			//Handed<float> lengths = new Handed<float>(
			//	CalculateArmLength(handBones.left, shoulders.left),
			//	CalculateArmLength(handBones.right, shoulders.right));

			//float shoulderWidth = Vector3.Distance(shoulders.left.GetAbsoluteRigTransform().Translation, shoulders.right.GetAbsoluteRigTransform().Translation);

			//characterCalibration.armSpan = lengths.left + lengths.right + shoulderWidth;

			//float CalculateArmLength(MyCharacterBone hand, MyCharacterBone shoulder)
			//{
			//	float totalLength = 0;
			//	for (MyCharacterBone bone = hand; bone != shoulder; bone = bone.Parent)
			//	{
			//		totalLength += bone.BindTransform.Translation.Length();
			//	}
			//	return totalLength;
			//}

			//Player.OnPlayerCalibrationChanged += RecalculatePlayerScale;
			//RecalculatePlayerScale(Player.GetBodyCalibration());
		}

		private void RecalculatePlayerScale(BodyCalibration playerCalibration)
		{
			//playerToCharacter = new MatrixAndInvert(ScalingModes[Main.Config.bodyScalingModeIndex.value].method(playerCalibration, characterCalibration));
		}

		public override void OnCharacterDead()
		{
		}

		public override void UpdateBeforeSimulation()
		{
			//MatrixD charHeadMatrix = Character.GetHeadMatrix(false);

			//Matrix aimOffset;
			//if (Character.CurrentWeapon != null) //TODO: Custom offsets for each weapon/tool, preferably with an easy in game way to adjust it for modded weapons/tools
			//	aimOffset = Matrix.CreateRotationX(Main.Config.handAimPitch.value.radians) * Matrix.CreateRotationZ(Main.Config.handAimYaw.value.radians);
			//else
			//	aimOffset = Matrix.CreateRotationX(Main.Config.handActivationPitch.value.radians) * Matrix.CreateRotationZ(Main.Config.handActivationYaw.value.radians);

			//hands = new Handed<Hand?>(
			//	UpdateHand(Player.Hands.left),
			//	UpdateHand(Player.Hands.right));

			//if (hands.left != null)  UpdateHandBones(hands.left.Value,  armIKStartIndexes.left,  handIndexes.left,  HandExtraTransforms.left);
			//if (hands.right != null) UpdateHandBones(hands.right.Value, armIKStartIndexes.right, handIndexes.right, HandExtraTransforms.right);

			//Hand? UpdateHand(Controller controller)
			//{
			//	if (!controller.pose.isTracked)
			//		return null;

			//	Matrix local = aimOffset * controller.deviceToAbsolute.matrix * Player.NeutralHeadToAbsolute.inverted * playerToCharacter.matrix;
			//	local.Orthogonalize();
			//	MatrixD world = local * charHeadMatrix;

			//	return new Hand(world, local);
			//}

			//void UpdateHandBones(in Hand hand, int ikStartIndex, int handBoneIndex, Matrix extraMatrix)
			//{
			//	CalculateHandIK.Invoke(Character, new object[] { ikStartIndex, handBoneIndex, extraMatrix * hand.world });
			//}
		}

		public override string ComponentTypeDebugString => "VR Body Component";
	}
}
