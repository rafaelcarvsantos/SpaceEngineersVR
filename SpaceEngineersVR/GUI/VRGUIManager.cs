using Sandbox.Graphics.GUI;
using SharpDX.Direct3D11;
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
			OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, 0.5f);

			OpenVR.Overlay.SetOverlayCurvature(OverlayHandle, 0.35f);
			OpenVR.Overlay.ShowOverlay(OverlayHandle);

			MyGuiSandbox.AddScreen(new MouseOverlay());

			Player.Player.OnPlayerFloorChanged += Player_OnPlayerFloorChanged;
		}

		private static void Player_OnPlayerFloorChanged()
		{
			Matrix mat = Player.Player.NeutralHeadToAbsolute.matrix;
			mat.Forward = Vector3.Forward;
			mat.Up = Vector3.Up;
			mat.Right = Vector3.Right;
			mat.Translation += mat.Forward * 0.25f;
			HmdMatrix34_t transform = mat.ToHMDMatrix34();
			OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);
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
