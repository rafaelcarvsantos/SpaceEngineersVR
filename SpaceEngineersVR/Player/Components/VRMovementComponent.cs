using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.Game.Screens.Helpers.RadialMenuActions;
using Sandbox.Game.SessionComponents.Clipboard;
using Sandbox.Game.World;
using System.Runtime.CompilerServices;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace SpaceEngineersVR.Player.Components
{
	internal class VRMovementComponent : MyCharacterComponent
	{
		public enum RotationType
		{
			Step,
			Continuous,
		}

		public enum MovementType
		{
			Head,
			Hand
		}

		private RotationType rotationType = RotationType.Continuous;
		private MovementType movementType = MovementType.Head;

		private Vector2 previousRotation = Vector2.Zero;
		private Vector2I hasSnapped = Vector2I.Zero;

		public static bool UsingControllerMovement;

		// TODO: Configurable rotation speed
		public static float RotationSpeed = 10;


		public override void Init(MyComponentDefinitionBase definition)
		{
			if (Character.InScene)
			{
				Init();
			}
		}

		public override void OnAddedToScene() => Init();

		public override void OnAddedToContainer()
		{
			this.NeedsUpdateBeforeSimulation = true;
		}

		private void Init()
		{
			//TODO: use InternalChangeModelAndCharacter and swap models
			//Character.ChangeModelAndColor();
		}

		public override void OnCharacterDead()
		{

		}

		public override void Simulate()
		{

		}

		public override void UpdateBeforeSimulation()
		{
			if (((IMyCharacter)Character).IsDead)
			{
				return;
			}

			if (MySession.Static.ControlledEntity is MyShipController)
			{
				ControlShip();
			}

			else if (((IMyCharacter)Character).EnabledThrusts)
			{
				ControlFlight();
			}
			else
			{
				ControlWalk();
			}
			ControlCommonFunctions();
			OrientateCharacterToHMD();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ControlShip()
		{
			Vector3 move = Vector3.Zero;
			Vector2 rotate = Vector2.Zero;
			float roll = 0f;

			Controls.Flight.Update();

			if (Controls.Flight.ThrustLRUD.active)
			{
				Vector3 v = Controls.Flight.ThrustLRUD.position;
				move.X += v.X;
				move.Y += v.Y;
			}

			if (Controls.Flight.ThrustLRFB.active)
			{
				Vector3 v = Controls.Flight.ThrustLRFB.position;
				move.X += v.X;
				move.Z -= v.Y;
			}

			if (Controls.Flight.ThrustUp.active)
				move.Y += Controls.Flight.ThrustUp.position.X;

			if (Controls.Flight.ThrustDown.active)
				move.Y -= Controls.Flight.ThrustDown.position.X;

			if (Controls.Flight.ThrustForward.active)
				move.Z -= Controls.Flight.ThrustForward.position.X;

			if (Controls.Flight.ThrustBackward.active)
				move.Z += Controls.Flight.ThrustBackward.position.X;

			if (Controls.Flight.RotateYawPitch.active)
			{
				Vector3 v = Controls.Flight.RotateYawPitch.position;

				rotate.X -= v.Y * RotationSpeed;
				rotate.Y += v.X * RotationSpeed;
			}

			if (Controls.Flight.RotateRoll.active)
			{
				roll += Controls.Flight.RotateRoll.position.X;
			}

			if (Controls.Flight.VirtualJoystickLeftThrust.active)
			{
				Vector3 pos = Controls.Flight.VirtualJoystickLeftThrust.position;
				move += new Vector3(pos.X, -pos.Z, pos.Y);
			}

			if (Controls.Flight.VirtualJoystickRightThrust.active)
			{
				Vector3 pos = Controls.Flight.VirtualJoystickRightThrust.position;
				move += new Vector3(pos.X, -pos.Z, pos.Y);
			}

			if (Controls.Flight.VirtualJoystickLeftRotate.active)
			{
				Vector3 p = Controls.Flight.VirtualJoystickLeftRotate.position;
				rotate.X += p.X * 5f;
				rotate.Y += p.Z * 5f;
				roll += p.Y;
			}

			if (Controls.Flight.VirtualJoystickRightRotate.active)
			{
				Vector3 p = Controls.Flight.VirtualJoystickRightRotate.position;
				rotate.X += p.X * 5f;
				rotate.Y += p.Z * 5f;
				roll += p.Y;
			}

			if (Controls.Flight.Dampener.hasPressed)
				MySession.Static.ControlledEntity?.SwitchDamping();

			ApplyMoveAndRotation(move, rotate, roll);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ControlWalk()
		{
			Controls.Walking.Update();

			Vector3 move = Vector3.Zero;
			float yaw = 0f;


			move.X += Controls.Walking.Move.position.X;
			move.Z -= Controls.Walking.Move.position.Y;

			//Matrix matrix = (movementType == MovementType.Head) ? Character.PositionComp.LocalMatrixRef : Controls.Static.RightHand.AbsoluteTracking;
			//Vector3.Transform(ref move, ref matrix, out move);


			// TODO: Configurable rotation speed and step by step rotation instead of continuous
			if (Controls.Walking.TurnRightLeft.active)
			{
				Vector3 v = Controls.Walking.TurnRightLeft.position;
				yaw = v.X * RotationSpeed;
			}

			if (Controls.Walking.JumpOrClimbUp.hasPressed)
				Character.Jump(Vector3.Up);

			if (Controls.Walking.CrouchOrClimbDown.hasPressed)
				Character.Crouch();

			if (Controls.Walking.JumpOrClimbUp.isPressed)
				move.Y = 1f;

			if (Controls.Walking.CrouchOrClimbDown.isPressed)
				move.Y = -1f;

			ApplyMoveAndRotation(move, new Vector2(0f, yaw), 0f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ControlFlight()
		{
			Vector3 move = Vector3.Zero;
			Vector2 rotate = Vector2.Zero;
			float roll = 0f;

			Controls.Flight.Update();

			if (Controls.Flight.ThrustLRUD.active)
			{
				var v = Controls.Flight.ThrustLRUD.position;
				move.X += v.X;
				move.Y += v.Y;
			}

			if (Controls.Flight.ThrustLRFB.active)
			{
				var v = Controls.Flight.ThrustLRFB.position;
				move.X += v.X;
				move.Z -= v.Y;
			}

			if (Controls.Flight.ThrustUp.active)
				move.Y += Controls.Flight.ThrustUp.position.X;

			if (Controls.Flight.ThrustDown.active)
				move.Y -= Controls.Flight.ThrustDown.position.X;

			if (Controls.Flight.ThrustForward.active)
				move.Z -= Controls.Flight.ThrustForward.position.X;

			if (Controls.Flight.ThrustBackward.active)
				move.Z += Controls.Flight.ThrustBackward.position.X;

			if (Controls.Flight.RotateYawPitch.active)
			{
				Vector3 v = Controls.Flight.RotateYawPitch.position;

				rotate.X -= v.Y * RotationSpeed;
				rotate.Y += v.X * RotationSpeed;
			}

			if (Controls.Flight.RotateRoll.active)
			{
				roll += Controls.Flight.RotateRoll.position.X;
			}

			if (Controls.Flight.VirtualJoystickLeftThrust.active)
				move += Controls.Flight.VirtualJoystickLeftThrust.position;

			if (Controls.Flight.VirtualJoystickRightThrust.active)
				move += Controls.Flight.VirtualJoystickRightThrust.position;

			if (Controls.Flight.VirtualJoystickLeftRotate.active)
			{
				Vector3 p = Controls.Flight.VirtualJoystickLeftRotate.position;
				rotate.X += p.X;
				rotate.Y += p.Y;
				roll += p.Z;
			}

			if (Controls.Flight.VirtualJoystickRightRotate.active)
			{
				Vector3 p = Controls.Flight.VirtualJoystickRightRotate.position;
				rotate.X += p.X;
				rotate.Y += p.Y;
				roll += p.Z;
			}

			if (Controls.Flight.Dampener.hasPressed)
				MySession.Static.ControlledEntity?.SwitchDamping();

			ApplyMoveAndRotation(move, rotate, roll);
		}


		void OrientateCharacterToHMD()
		{
			//Matrix absoluteRotation = Main.Headset.hmdAbsolute;
			//absoluteRotation *= rotationOffset;
			//
			//Matrix characterRotation = Character.WorldMatrix;
			//characterRotation.Translation = Vector3.Zero;


			//Character.MoveAndRotate();
		}


		void ApplyMoveAndRotation(Vector3 move, Vector2 rotate, float roll)
		{
			move = Vector3.Clamp(move, -Vector3.One, Vector3.One);

			if (Character.CurrentMovementState != MyCharacterMovementEnum.Flying)
			{
				//Rotate to zero out any character's head pitch
				//This wont be exact as MoveAndRotate scales the input by the mouse sensitivity, but it should be fine after a few frames
				if (MySession.Static.ControlledEntity == Character)
				{
					rotate.X += Character.HeadLocalXAngle;
				}
				else
				{
					Character.MoveAndRotate(Vector3.Zero, new Vector2(-Character.HeadLocalXAngle, 0f), 0f);
				}
			}

			// Setting this statis variable is required to prevent the game from
			// zeroing out the control input. Do not optimize this.
			// See MyGuiScreenGamePlayPatch on how this flag is used 
			UsingControllerMovement = move != Vector3.Zero || rotate != Vector2.Zero || roll != 0f;

			if (UsingControllerMovement)
				MySession.Static.ControlledEntity?.MoveAndRotate(move, rotate, roll);
			else
				MySession.Static.ControlledEntity?.MoveAndRotateStopped();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ControlCommonFunctions()
		{
			IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;

			if (Controls.Tool.Primary.hasPressed)
			{
				controlledEntity?.BeginShoot(MyShootActionEnum.PrimaryAction);
			}
			else if (Controls.Tool.Primary.hasReleased)
			{
				controlledEntity?.EndShoot(MyShootActionEnum.PrimaryAction);
			}

			if (Controls.Tool.Secondary.hasPressed)
			{
				controlledEntity?.BeginShoot(MyShootActionEnum.SecondaryAction);
			}
			else if (Controls.Tool.Secondary.hasReleased)
			{
				controlledEntity?.EndShoot(MyShootActionEnum.SecondaryAction);
			}

			if (Controls.Tool.Reload.hasPressed)
			{
				// TODO
			}

			if (Controls.Tool.Unequip.hasPressed)
			{
				controlledEntity?.SwitchToWeapon(null);
			}

			if (Controls.Tool.CutGrid.hasPressed)
			{
				new MyActionCutGrid().ExecuteAction();
			}

			if (Controls.Tool.CopyGrid.hasPressed)
			{
				new MyActionCopyGrid().ExecuteAction();
			}

			if (Controls.Tool.PasteGrid.hasPressed)
			{
				new MyActionPasteGrid().ExecuteAction();
			}

			if (Controls.System.Interact.hasPressed)
			{
				controlledEntity?.Use();
			}

			if (Controls.System.Helmet.hasPressed)
			{
				((IMyCharacter)Character).SwitchHelmet();
			}

			if (Controls.System.Jetpack.hasPressed)
			{
				((IMyCharacter)Character).SwitchThrusts();
			}

			if (Controls.System.Broadcasting.hasPressed)
			{
				controlledEntity?.SwitchBroadcasting();
			}

			if (Controls.System.Park.hasPressed)
			{
				controlledEntity?.SwitchHandbrake();
			}

			if (Controls.System.Power.hasPressed)
			{
				new MyActionTogglePower().ExecuteAction();
			}

			if (Controls.System.Lights.hasPressed)
			{
				Character.SwitchLights();
			}

			if (Controls.System.Respawn.hasPressed)
			{
				controlledEntity?.Die();
			}

			if (Controls.System.ToggleSignals.hasPressed)
			{
				new MyActionToggleSignals().ExecuteAction();
			}

			if (Controls.Placement.ToggleSymmetry.hasPressed)
			{
				new MyActionToggleSymmetry().ExecuteAction();
			}

			if (Controls.Placement.SymmetrySetup.hasPressed)
			{
				new MyActionSymmetrySetup().ExecuteAction();
			}

			if (Controls.Placement.PlacementMode.hasPressed)
			{
				MyClipboardComponent.Static.ChangeStationRotation();
				MyCubeBuilder.Static.CycleCubePlacementMode();
			}

			if (Controls.Placement.CubeSize.hasPressed)
			{
				// TODO
			}

			if (Controls.WristTablet.Terminal.hasPressed)
			{
				((IMyCharacter)Character).ShowTerminal();
			}

			if (Controls.WristTablet.Inventory.hasPressed)
			{
				((IMyCharacter)Character).ShowInventory();
			}

			if (Controls.WristTablet.ColorSelector.hasPressed)
			{
				new MyActionColorTool().ExecuteAction();
			}

			if (Controls.WristTablet.ColorPicker.hasPressed)
			{
				new MyActionColorPicker().ExecuteAction();
			}

			if (Controls.WristTablet.BuildPlanner.hasPressed)
			{
				// TODO
			}

			if (Controls.WristTablet.ToolbarConfig.hasPressed)
			{
				// TODO
			}

			if (Controls.WristTablet.BlockSelector.hasPressed)
			{
				// TODO
			}

			if (Controls.WristTablet.Contract.hasPressed)
			{
				// TODO
			}

			if (Controls.WristTablet.Chat.hasPressed)
			{
				// TODO
			}

			if (Controls.Game.ToggleView.hasPressed)
			{
				((IMyCharacter)Character).IsInFirstPersonView = !((IMyCharacter)Character).IsInFirstPersonView;
			}

			if (Controls.Game.Pause.hasPressed)
			{
				MySandboxGame.IsPaused = !MySandboxGame.IsPaused;
			}

			if (Controls.Game.VoiceChat.hasPressed)
			{
				// TODO
			}

			if (Controls.Game.SignalMode.hasPressed)
			{
				// TODO
			}

			if (Controls.Game.SpectatorMode.hasPressed)
			{
				// TODO
			}

			if (Controls.Game.Teleport.hasPressed)
			{
				// TODO: character.Teleport();
			}
		}

		public override string ComponentTypeDebugString => "VR Movement Component";

	}
}
