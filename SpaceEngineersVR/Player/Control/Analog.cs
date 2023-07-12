using System.Runtime.CompilerServices;
using Valve.VR;
using VRageMath;

namespace SpaceEngineersVR.Player.Control;

public interface IAnalog : IControl
{
	Vector3 position { get; }
	Vector3 delta { get; }
}
public sealed class OpenVRAnalog : IAnalog
{
	private static readonly unsafe uint InputAnalogActionData_t_size = (uint)sizeof(InputAnalogActionData_t);

	private InputAnalogActionData_t data;
	private readonly ulong handle;

	public bool active => data.bActive;
	public Vector3 position => new(data.x, data.y, data.z);
	public Vector3 delta => new(data.deltaX, data.deltaY, data.deltaZ);

	public OpenVRAnalog(string actionName)
	{
		OpenVR.Input.GetActionHandle(actionName, ref handle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Update()
	{
		OpenVR.Input.GetAnalogActionData(handle, ref data, InputAnalogActionData_t_size, OpenVR.k_ulInvalidInputValueHandle);
	}
}
