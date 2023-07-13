using SpaceEngineersVR.Player.Control;
using SpaceEngineersVR.Plugin;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Valve.VR;

// See:
// https://github.com/ValveSoftware/openvr/wiki/SteamVR-Input
// https://github.com/ValveSoftware/openvr/wiki/Action-manifest

namespace SpaceEngineersVR.Player
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class Controls
	{
		public static class Walking
		{
			public static readonly OpenVRAnalog Move;
			public static readonly OpenVRAnalog TurnRightLeft;

			public static readonly OpenVRButton JumpOrClimbUp;
			public static readonly OpenVRButton CrouchOrClimbDown;

			private static readonly ActionSets ActionSets;

			static Walking()
			{
				Move = new OpenVRAnalog("/actions/walking/in/Move");
				TurnRightLeft = new OpenVRAnalog("/actions/walking/in/TurnRightLeft");
				JumpOrClimbUp = new OpenVRButton("/actions/walking/in/JumpOrClimbUp");
				CrouchOrClimbDown = new OpenVRButton("/actions/walking/in/CrouchOrClimbDown");

				ActionSets = new ActionSets("/actions/walking", "/actions/common");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Update()
			{
				ActionSets.Update();

				Move.Update();
				TurnRightLeft.Update();
				JumpOrClimbUp.Update();
				CrouchOrClimbDown.Update();

				UpdateCommon();
			}
		}

		public static class Flight
		{
			public static readonly OpenVRAnalog ThrustLRUD = new OpenVRAnalog("/actions/flying/in/ThrustLRUD");
			public static readonly OpenVRAnalog ThrustLRFB = new OpenVRAnalog("/actions/flying/in/ThrustLRFB");
			public static readonly OpenVRAnalog ThrustUp = new OpenVRAnalog("/actions/flying/in/ThrustUp");
			public static readonly OpenVRAnalog ThrustDown = new OpenVRAnalog("/actions/flying/in/ThrustDown");
			public static readonly OpenVRAnalog ThrustForward = new OpenVRAnalog("/actions/flying/in/ThrustForward");
			public static readonly OpenVRAnalog ThrustBackward = new OpenVRAnalog("/actions/flying/in/ThrustBackward");
			public static readonly OpenVRAnalog RotateYawPitch = new OpenVRAnalog("/actions/flying/in/RotateYawPitch");
			public static readonly OpenVRAnalog RotateRoll = new OpenVRAnalog("/actions/flying/in/RotateRoll");
			public static readonly VirtualJoystick VirtualJoystickLeftThrust = new VirtualJoystick(new OpenVRButton("/actions/flying/in/GrabLeftThrustVirtualJoystick"), Player.Hands.left, VirtualJoystick.ControlType.Translation, VirtualJoystick.Invert.None, 5f);
			public static readonly VirtualJoystick VirtualJoystickRightThrust = new VirtualJoystick(new OpenVRButton("/actions/flying/in/GrabRightThrustVirtualJoystick"), Player.Hands.right, VirtualJoystick.ControlType.Translation, VirtualJoystick.Invert.None, 5f);
			public static readonly VirtualJoystick VirtualJoystickLeftRotate = new VirtualJoystick(new OpenVRButton("/actions/flying/in/GrabLeftRotateVirtualJoystick"), Player.Hands.left, VirtualJoystick.ControlType.Rotation, VirtualJoystick.Invert.None, 5f);
			public static readonly VirtualJoystick VirtualJoystickRightRotate = new VirtualJoystick(new OpenVRButton("/actions/flying/in/GrabRightRotateVirtualJoystick"), Player.Hands.right, VirtualJoystick.ControlType.Rotation, VirtualJoystick.Invert.None, 5f);
			public static readonly OpenVRButton Dampener = new OpenVRButton("/actions/flying/in/Dampener");

			private static readonly ActionSets ActionSets = new ActionSets("/actions/flying", "/actions/common");

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Update()
			{
				ActionSets.Update();

				ThrustLRUD.Update();
				ThrustLRFB.Update();
				ThrustUp.Update();
				ThrustDown.Update();
				ThrustForward.Update();
				ThrustBackward.Update();
				RotateYawPitch.Update();
				RotateRoll.Update();

				VirtualJoystickLeftThrust.Update();
				VirtualJoystickRightThrust.Update();
				VirtualJoystickLeftRotate.Update();
				VirtualJoystickRightRotate.Update();

				Dampener.Update();

				UpdateCommon();
			}
		}

		public static class Tool
		{
			public static readonly OpenVRButton Primary;
			public static readonly OpenVRButton Secondary;
			public static readonly OpenVRButton Reload;
			public static readonly OpenVRButton Unequip;
			public static readonly OpenVRButton CutGrid;
			public static readonly OpenVRButton CopyGrid;
			public static readonly OpenVRButton PasteGrid;

			static Tool()
			{
				Primary = new OpenVRButton("/actions/common/in/Primary");
				Secondary = new OpenVRButton("/actions/common/in/Secondary");
				Reload = new OpenVRButton("/actions/common/in/Reload");
				Unequip = new OpenVRButton("/actions/common/in/Unequip");
				CutGrid = new OpenVRButton("/actions/common/in/CutGrid");
				CopyGrid = new OpenVRButton("/actions/common/in/CopyGrid");
				PasteGrid = new OpenVRButton("/actions/common/in/PasteGrid");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Update()
			{
				Primary.Update();
				Secondary.Update();
				Reload.Update();
				Unequip.Update();
				CutGrid.Update();
				CopyGrid.Update();
				PasteGrid.Update();
			}
		}

		public static class System
		{
			public static readonly OpenVRButton Interact;
			public static readonly OpenVRButton Helmet;
			public static readonly OpenVRButton Jetpack;
			public static readonly OpenVRButton Broadcasting;
			public static readonly OpenVRButton Park;
			public static readonly OpenVRButton Power;
			public static readonly OpenVRButton Lights;
			public static readonly OpenVRButton Respawn;
			public static readonly OpenVRButton ToggleSignals;

			static System()
			{
				Interact = new OpenVRButton("/actions/common/in/Interact");
				Helmet = new OpenVRButton("/actions/common/in/Helmet");
				Jetpack = new OpenVRButton("/actions/common/in/Jetpack");
				Broadcasting = new OpenVRButton("/actions/common/in/Broadcasting");
				Park = new OpenVRButton("/actions/common/in/Park");
				Power = new OpenVRButton("/actions/common/in/Power");
				Lights = new OpenVRButton("/actions/common/in/Lights");
				Respawn = new OpenVRButton("/actions/common/in/Respawn");
				ToggleSignals = new OpenVRButton("/actions/common/in/ToggleSignals");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Update()
			{
				Interact.Update();
				Helmet.Update();
				Jetpack.Update();
				Broadcasting.Update();
				Park.Update();
				Power.Update();
				Lights.Update();
				Respawn.Update();
				ToggleSignals.Update();
			}
		}

		public static class Placement
		{
			public static readonly OpenVRButton ToggleSymmetry;
			public static readonly OpenVRButton SymmetrySetup;
			public static readonly OpenVRButton PlacementMode;
			public static readonly OpenVRButton CubeSize;

			static Placement()
			{
				ToggleSymmetry = new OpenVRButton("/actions/common/in/ToggleSymmetry");
				SymmetrySetup = new OpenVRButton("/actions/common/in/SymmetrySetup");
				PlacementMode = new OpenVRButton("/actions/common/in/PlacementMode");
				CubeSize = new OpenVRButton("/actions/common/in/CubeSize");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Update()
			{
				ToggleSymmetry.Update();
				SymmetrySetup.Update();
				PlacementMode.Update();
				CubeSize.Update();
			}
		}

		public static class WristTablet
		{
			public static readonly OpenVRButton Terminal;
			public static readonly OpenVRButton Inventory;
			public static readonly OpenVRButton ColorSelector;
			public static readonly OpenVRButton ColorPicker;
			public static readonly OpenVRButton BuildPlanner;
			public static readonly OpenVRButton ToolbarConfig;
			public static readonly OpenVRButton BlockSelector;
			public static readonly OpenVRButton Contract;
			public static readonly OpenVRButton Chat;

			static WristTablet()
			{
				Terminal = new OpenVRButton("/actions/common/in/Terminal");
				Inventory = new OpenVRButton("/actions/common/in/Inventory");
				ColorSelector = new OpenVRButton("/actions/common/out/ColorSelector");
				ColorPicker = new OpenVRButton("/actions/common/out/ColorPicker");
				BuildPlanner = new OpenVRButton("/actions/common/in/BuildPlanner");
				ToolbarConfig = new OpenVRButton("/actions/common/in/ToolbarConfig");
				BlockSelector = new OpenVRButton("/actions/common/in/BlockSelector");
				Contract = new OpenVRButton("/actions/common/in/Contract");
				Chat = new OpenVRButton("/actions/common/in/Chat");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Update()
			{
				Terminal.Update();
				Inventory.Update();
				ColorSelector.Update();
				ColorPicker.Update();
				BuildPlanner.Update();
				ToolbarConfig.Update();
				BlockSelector.Update();
				Contract.Update();
				Chat.Update();
			}
		}

		public static class Game
		{
			public static readonly OpenVRButton ToggleView;
			public static readonly OpenVRButton Pause;
			public static readonly OpenVRButton VoiceChat;
			public static readonly OpenVRButton SignalMode;
			public static readonly OpenVRButton SpectatorMode;
			public static readonly OpenVRButton Teleport;

			static Game()
			{
				ToggleView = new OpenVRButton("/actions/common/in/ToggleView");
				Pause = new OpenVRButton("/actions/common/in/Pause");
				VoiceChat = new OpenVRButton("/actions/common/in/VoiceChat");
				SignalMode = new OpenVRButton("/actions/common/in/SignalMode");
				SpectatorMode = new OpenVRButton("/actions/common/in/SpectatorMode");
				Teleport = new OpenVRButton("/actions/common/in/Teleport");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static void Update()
			{
				ToggleView.Update();
				Pause.Update();
				VoiceChat.Update();
				SignalMode.Update();
				SpectatorMode.Update();
				Teleport.Update();
			}
		}

		static Controls()
		{
			OpenVR.Input.SetActionManifestPath(Common.ActionJsonPath);
		}

		private static void UpdateCommon()
		{
			Tool.Update();
			System.Update();
			Placement.Update();
			WristTablet.Update();
			Game.Update();
		}
	}
}
