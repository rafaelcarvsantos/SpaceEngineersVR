using Valve.VR;

namespace SpaceEngineersVR.Player.Control;

public class ActionSets
{
	private readonly unsafe uint VRActiveActionSet_t_size = (uint)sizeof(VRActiveActionSet_t);

	private readonly VRActiveActionSet_t[] sets;

	public ActionSets(params string[] names)
	{
		sets = new VRActiveActionSet_t[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			OpenVR.Input.GetActionSetHandle(names[i], ref sets[i].ulActionSet);
			sets[i].ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle;
		}
	}

	public void Update()
	{
		OpenVR.Input.UpdateActionState(sets, VRActiveActionSet_t_size);
	}
}