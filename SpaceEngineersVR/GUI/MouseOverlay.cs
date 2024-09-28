
using Sandbox.Game.Gui;
using Sandbox.Graphics;
using SpaceEngineersVR.Plugin;
using VRage.Input;
using VRageMath;
using VRageRender;

namespace SpaceEngineersVR.GUI
{
	public class MouseOverlay : MyGuiScreenDebugBase
	{
		public MouseOverlay() : base(new Vector2(0.5f, 0.5f), default(Vector2), null, isTopMostScreen: true)
		{
			Logger.Debug("Creating mouse overlay.");
			m_isTopMostScreen = true;
			m_drawEvenWithoutFocus = true;
			CanHaveFocus = false;
			m_canShareInput = false;
		}

		~MouseOverlay()
		{
			Logger.Debug("Destructing mouse overlay.");
		}

		public override bool Draw()
		{
			if (!base.Draw())
			{
				return false;
			}

			Vector2 mouseLocation = MyInput.Static.GetMousePosition();

			RectangleF destination = new RectangleF(mouseLocation.X - 32, mouseLocation.Y - 32, 64, 64);

			MyRenderProxy.DrawSprite(MyGuiManager.GetMouseCursorTexture(), ref destination, null, Color.White, 0f, false, true);

			return true;
		}

		public override string GetFriendlyName()
		{
			return "MouseOverlay";
		}
	}
}
