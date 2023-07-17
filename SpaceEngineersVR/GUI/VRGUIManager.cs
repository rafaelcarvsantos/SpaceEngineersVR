using Sandbox.Graphics.GUI;
using SharpDX.Direct3D11;
using SpaceEngineersVR.Config;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Util;
using SpaceEngineersVR.Wrappers;
using Valve.VR;
using VRageMath;

namespace SpaceEngineersVR.GUI
{
	public static class VRGUIManager
	{
		public static bool IsDebugHUDEnabled = true;

		private static readonly ulong OverlayHandle = 0uL;

		static VRGUIManager()
		{
			OpenVR.Overlay.CreateOverlay("SEVR_DEBUG_OVERLAY", "SEVR_DEBUG_OVERLAY", ref OverlayHandle);
			OpenVR.Overlay.ShowOverlay(OverlayHandle);

			//TODO: reset the overlay to these values when a game is exited
			OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, 3f);

			HmdMatrix34_t transform = new HmdMatrix34_t
			{
				m0 = 1f, m1 = 0f, m2 = 0f, m3 = 0f,
				m4 = 0f, m5 = 1f, m6 = 0f, m7 = 1f,
				m8 = 0f, m9 = 0f, m10 = 1f, m11 = -2f
			};
			OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);

			OpenVR.Overlay.SetOverlayCurvature(OverlayHandle, 0.25f);

			MyGuiSandbox.AddScreen(new MouseOverlay());

			Player.Player.OnPlayerFloorChanged += RepositionOverlay;
			Common.Config.onUIDepthChanged += _ => RepositionOverlay();
		}

		private static void RepositionOverlay()
		{
			Matrix mat = Player.Player.NeutralHeadToAbsolute.matrix;
			mat.Forward = Vector3.Forward;
			mat.Up = Vector3.Up;
			mat.Right = Vector3.Right;
			mat.Translation += mat.Forward * Common.Config.uiDepth;
			HmdMatrix34_t transform = mat.ToHMDMatrix34();
			OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);

			OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, 2f * Common.Config.uiDepth);

			OpenVR.Overlay.SetOverlayCurvature(OverlayHandle, 0.35f);
		}

		public static void Draw()
		{
			Texture2D guiTexture = (Texture2D)MyRender11.Backbuffer.resource;
			Texture_t textureUI = new Texture_t
			{
				eColorSpace = EColorSpace.Auto,
				eType = ETextureType.DirectX,
				handle = guiTexture.NativePointer
			};

			OpenVR.Overlay.SetOverlayTexture(OverlayHandle, ref textureUI);
			OpenVR.Overlay.ShowOverlay(OverlayHandle);
		}
	}
}
