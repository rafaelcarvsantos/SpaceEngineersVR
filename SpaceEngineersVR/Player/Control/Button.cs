using System.Runtime.CompilerServices;
using Valve.VR;

namespace SpaceEngineersVR.Player.Control
{
	public interface IButton : IControl
	{
		bool isPressed { get; }
		bool hasChanged { get; }
		bool hasPressed { get; }
		bool hasReleased { get; }
	}

	public sealed class OpenVRButton : IButton
	{
		private static readonly unsafe uint InputDigitalActionData_t_size = (uint)sizeof(InputDigitalActionData_t);

		private InputDigitalActionData_t data;
		private readonly ulong handle;

		public bool active => data.bActive;
		public bool isPressed => data.bState;
		public bool hasChanged => data.bChanged;
		public bool hasPressed => data.bState && data.bChanged;
		public bool hasReleased => !data.bState && data.bChanged;

		public OpenVRButton(string actionName)
		{
			OpenVR.Input.GetActionHandle(actionName, ref handle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update()
		{
			OpenVR.Input.GetDigitalActionData(handle, ref data, InputDigitalActionData_t_size, OpenVR.k_ulInvalidInputValueHandle);
		}
	}
}
