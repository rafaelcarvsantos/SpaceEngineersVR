using System;
using VRageMath;

namespace SpaceEngineersVR.Player.Control
{
	public sealed class VirtualJoystick : IAnalog
	{
		public enum ControlType : byte
		{
			Translation,
			Rotation,
		}
		[Flags]
		public enum Invert : byte
		{
			None = 0,
			InvertX = 1 << 0,
			InvertY = 1 << 1,
			InvertZ = 1 << 2,
		}

		private readonly ControlType controlType;
		private readonly Invert invert;

		private readonly OpenVRButton enable;
		private readonly TrackedDevice device;

		private Util.MatrixAndInvert startPosToAbsolute;
		private Util.MatrixAndInvert lastFramePosToStart;

		public bool active => enable.active;
		public Vector3 position { get; private set; }
		public Vector3 delta { get; private set; }


		public VirtualJoystick(OpenVRButton enable, TrackedDevice device, ControlType controlType, Invert invert)
		{
			this.enable = enable;
			this.device = device;
			this.controlType = controlType;
			this.invert = invert;
		}

		public void Update()
		{
			enable.Update();

			if (enable.hasPressed)
			{
				startPosToAbsolute = device.pose.deviceToAbsolute;
				lastFramePosToStart = Util.MatrixAndInvert.Identity;

				position = Vector3.Zero;
				delta = Vector3.Zero;
			}
			else if (enable.isPressed)
			{
				Matrix pose = device.pose.deviceToAbsolute.matrix * startPosToAbsolute.inverted;
				Matrix relPose = pose * lastFramePosToStart.inverted;
				lastFramePosToStart = new Util.MatrixAndInvert(pose);

				Vector3 newPosition = Vector3.Zero;
				Vector3 newDelta = Vector3.Zero;

				switch (controlType)
				{
					case ControlType.Translation:
						newPosition = pose.Translation;
						newDelta = relPose.Translation;
						break;
					case ControlType.Rotation:
						newPosition = GetAngles(pose);
						newDelta = GetAngles(relPose);
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

				position = newPosition;
				delta = newDelta;
			}
			else if (enable.hasReleased)
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

				return axis * angle;
			}
		}
	}
}
