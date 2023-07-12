namespace SpaceEngineersVR.Player.Control;

public interface IControl
{
	bool active { get; }
	void Update();
}
