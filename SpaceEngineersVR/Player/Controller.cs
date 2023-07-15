using Sandbox;
using Sandbox.Game.World;
using SpaceEngineersVR.Patches;
using SpaceEngineersVR.Plugin;
using System;
using Valve.VR;
using VRageMath;

namespace SpaceEngineersVR.Player
{
	public class Controller : TrackedDevice
	{
		[Flags]
		public enum Button : ulong
		{
			System = 1ul << EVRButtonId.k_EButton_System,
			ApplicationMenu = 1ul << EVRButtonId.k_EButton_ApplicationMenu,
			Grip = 1ul << EVRButtonId.k_EButton_Grip,
			Touchpad = 1ul << EVRButtonId.k_EButton_SteamVR_Touchpad,
			Trigger = 1ul << EVRButtonId.k_EButton_SteamVR_Trigger,

			A = 1ul << EVRButtonId.k_EButton_A,
			B = 1ul << EVRButtonId.k_EButton_ApplicationMenu,

			DPadDown = 1ul << EVRButtonId.k_EButton_DPad_Down,
		}
		/*
		public enum Axis
		{
			Joystick = 0,
			Trigger = 1,
			Grip = 2,
			Axis3 = 3,
			Axis4 = 4,
		}
		*/

		private const int RollingVelocityFrames = 10;
		public Vector3 RollingVelocity
		{
			get
			{
				Vector3 result = Vector3.Zero;
				for (int i = 0; i < RollingVelocityFrames; i++)
					result += rollingVelocity[i];
				return (result / RollingVelocityFrames);
			}
		}
		private Vector3[] rollingVelocity = new Vector3[RollingVelocityFrames];
		private int rollingVelocityUpdate = 0;

		public Controller(string actionName, string hapticsName)
			: base(actionName, hapticsName)
		{
			SimulationUpdater.UpdateBeforeSim += UpdateBeforeSimulation;
		}


		private void UpdateBeforeSimulation()
		{
			rollingVelocity[rollingVelocityUpdate++ % RollingVelocityFrames] = pose.velocity;
		}

		public void TriggerHapticPulse(ushort duration = 500, Button button = Button.Touchpad)
		{
			OpenVR.System.TriggerHapticPulse(deviceId, (uint)button, (char)duration);
		}
	}
}
