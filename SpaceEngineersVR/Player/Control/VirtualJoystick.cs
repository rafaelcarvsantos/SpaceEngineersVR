using Sandbox.Game.World;
using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using System;
using VRageMath;

namespace SpaceEngineersVR.Player.Control
{
	public sealed class VirtualJoystick : IAnalog
	{
		public enum ControlType
		{
			Translation,
			Rotation,
		}
		[Flags]
		public enum Invert
		{
			None = 0,
			InvertX = 1 << 0,
			InvertY = 1 << 1,
			InvertZ = 1 << 2,
		}

		public ControlType controlType;
		public Invert invert;

		public readonly OpenVRButton enableButton;
		public readonly TrackedDevice device;

		public float sensitivity = 1f;

		private Util.MatrixAndInvert startPosToAbsolute;
		private Util.MatrixAndInvert lastFramePosToStart;

		public bool active => enableButton.active;
		public Vector3 position { get; private set; }
		public Vector3 delta { get; private set; }


		public VirtualJoystick(OpenVRButton enableButton, TrackedDevice device, ControlType controlType, Invert invert, float sensitivity = 1f)
		{
			this.enableButton = enableButton;
			this.device = device;
			this.controlType = controlType;
			this.invert = invert;
			this.sensitivity = sensitivity;
		}

		public void Update()
		{
			enableButton.Update();
			VRBodyComponent vrBody = null;
			if (Main.Config.debug.value)
			{
				vrBody = MySession.Static.LocalCharacter?.Components.Get<VRBodyComponent>();
				if(vrBody != null)
					Util.Util.DrawDebugMatrix(startPosToAbsolute.matrix * Player.PlayerToAbsolute.inverted * vrBody.playerToCharacter.matrix * vrBody.Character.WorldMatrix, "Virtual Joystick");
			}

			Matrix stick = device.pose.deviceToAbsolute.matrix;
			(stick.Forward, stick.Up) = (stick.Down, stick.Forward);
			stick.Translation += stick.Down * 0.1f;

			if (enableButton.hasPressed)
			{
				startPosToAbsolute = new Util.MatrixAndInvert(stick);
				lastFramePosToStart = Util.MatrixAndInvert.Identity;

				position = Vector3.Zero;
				delta = Vector3.Zero;
			}
			else if (enableButton.isPressed)
			{
				Matrix stickToStart = stick * startPosToAbsolute.inverted;
				Matrix stickDelta = stickToStart * lastFramePosToStart.inverted;
				lastFramePosToStart = new Util.MatrixAndInvert(stickToStart);
				
				if (vrBody != null)
					Util.Util.DrawDebugMatrix(stick * Player.PlayerToAbsolute.inverted * vrBody.playerToCharacter.matrix * vrBody.Character.WorldMatrix);

				Vector3 newPosition = Vector3.Zero;
				Vector3 newDelta = Vector3.Zero;

				switch (controlType)
				{
					case ControlType.Translation:
						newPosition = stickToStart.Translation;
						newDelta = stickDelta.Translation;
						break;
					case ControlType.Rotation:
						newPosition = GetAngles(stickToStart);
						newDelta = GetAngles(stickDelta);
						break;
				}
				if (invert.HasFlag(Invert.InvertX))
				{
					newPosition.X = -newPosition.X;
					newDelta.X = -newDelta.X;
				}
				if (invert.HasFlag(Invert.InvertY))
				{
					newPosition.Y = -newPosition.Y;
					newDelta.Y = -newDelta.Y;
				}
				if (invert.HasFlag(Invert.InvertZ))
				{
					newPosition.Z = -newPosition.Z;
					newDelta.Z = -newDelta.Z;
				}

				position = newPosition * sensitivity;
				delta = newDelta * sensitivity;
			}
			else if (enableButton.hasReleased)
			{
				position = Vector3.Zero;
				delta = Vector3.Zero;
			}

			Vector3 GetAngles(in Matrix mat)
			{
				//return new(
				//	MyMath.ArcTanAngle(mat.M32, mat.M33),
				//	MyMath.ArcTanAngle(-mat.M31, (float)Math.Sqrt(mat.M32 * mat.M32 + mat.M33 * mat.M33)),
				//	MyMath.ArcTanAngle(mat.M21, mat.M11));

				//return new(
				//	(float)Math.Asin(mat.M21),
				//	MyMath.ArcTanAngle(-mat.M23, mat.M22),
				//	MyMath.ArcTanAngle(-mat.M31, mat.M11));

				Quaternion quat = Quaternion.CreateFromRotationMatrix(mat);
				quat.GetAxisAngle(out Vector3 axis, out float angle);

				return -axis * angle;
			}
		}
	}
}
