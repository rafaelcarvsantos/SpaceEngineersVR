using Sandbox.Graphics.GUI;
using SharpDX.Direct3D11;
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

			MyGuiSandbox.AddScreen(new MouseOverlay());

			Player.Player.OnPlayerFloorChanged += RepositionOverlay;
			Main.Config.uiDepth.onValueChanged += _ => RepositionOverlay();
			Main.Config.uiWidth.onValueChanged += _ => RepositionOverlay();
			Main.Config.uiCurvature.onValueChanged += _ => RepositionOverlay();
		}

		private static void RepositionOverlay()
		{
			Matrix mat = Player.Player.NeutralHeadToAbsolute.matrix;
			mat.Forward = Vector3.Forward;
			mat.Up = Vector3.Up;
			mat.Right = Vector3.Right;
			mat.Translation += mat.Forward * Main.Config.uiDepth.value;
			HmdMatrix34_t transform = mat.ToHMDMatrix34();
			OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);

			OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, Main.Config.uiWidth.value * Main.Config.uiDepth.value);

			OpenVR.Overlay.SetOverlayCurvature(OverlayHandle, Main.Config.uiCurvature.value);
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
