using Sandbox;
using Sandbox.Graphics.GUI;
using SpaceEngineersVR.Config;
using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Utils;
using System;
using System.Text;
using VRage;
using VRage.Utils;
using VRageMath;

namespace SpaceEngineersVR.GUI
{
	public class MyPluginConfigDialog : MyGuiScreenBase
	{
		private const string Caption = "Space Engineers VR Configuration";
		public override string GetFriendlyName() => "MyPluginConfigDialog";

		private MyLayoutTable layoutTable;

		private MyGuiControlLabel enableKeyboardAndMouseControlsLabel;
		private MyGuiControlCheckbox enableKeyboardAndMouseControlsCheckbox;

		private MyGuiControlLabel enableCharacterRenderingLabel;
		private MyGuiControlCheckbox enableCharacterRenderingCheckbox;

		private MyGuiControlLabel useHeadRotationForCharacterLabel;
		private MyGuiControlCheckbox useHeadRotationForCharacterCheckbox;

		private MyGuiControlLabel characterScalingModeLabel;
		private MyGuiControlListbox characterScalingModeListBox;

		private MyGuiControlLabel resolutionScaleLabel;
		private MyGuiControlSlider resolutionScaleSlider;

		private MyGuiControlLabel handActivationPitchLabel;
		private MyGuiControlSlider handActivationPitchSlider;

		private MyGuiControlLabel handActivationYawLabel;
		private MyGuiControlSlider handActivationYawSlider;

		private MyGuiControlLabel handAimPitchLabel;
		private MyGuiControlSlider handAimPitchSlider;

		private MyGuiControlLabel handAimYawLabel;
		private MyGuiControlSlider handAimYawSlider;

		private MyGuiControlLabel uiDepthLabel;
		private MyGuiControlSlider uiDepthSlider;

		private MyGuiControlMultilineText infoText;
		private MyGuiControlButton closeButton;

		public MyPluginConfigDialog() : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.7f, 0.7f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
		{
			EnabledBackgroundFade = true;
			m_closeOnEsc = true;
			m_drawEvenWithoutFocus = true;
			CanHideOthers = true;
			CanBeHidden = true;
			CloseButtonEnabled = true;
		}

		public override void LoadContent()
		{
			base.LoadContent();
			RecreateControls(true);
		}

		public override void RecreateControls(bool constructor)
		{
			base.RecreateControls(constructor);

			CreateControls();
			LayoutControls();
		}

		private void CreateControls()
		{
			AddCaption(Caption);

			var config = Common.Config;
			CreateCheckbox(out enableKeyboardAndMouseControlsLabel, out enableKeyboardAndMouseControlsCheckbox, config.enableKeyboardAndMouseControls.value, value => config.enableKeyboardAndMouseControls.value = value, "Enable Keyboard And Mouse Controls", "Enables keyboard and mouse controls.");
			CreateCheckbox(out enableCharacterRenderingLabel, out enableCharacterRenderingCheckbox, config.enableCharacterRendering.value, value => config.enableCharacterRendering.value = value, "Enable Character Rendering", "Enables rendering the character.");
			CreateCheckbox(out useHeadRotationForCharacterLabel, out useHeadRotationForCharacterCheckbox, config.useHeadRotationForCharacter.value, value => config.useHeadRotationForCharacter.value = value, "Use Head Rotation For Character", "Character turns when you turn your head, otherwise they always face your SteamVR forward direction.");

			characterScalingModeLabel = new MyGuiControlLabel()
			{
				Text = "Character Scaling Mode",
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
			};
			characterScalingModeListBox = new MyGuiControlListbox()
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				ItemSize = new Vector2(400f, 45f),
			};
			for (int i = 0; i < VRBodyComponent.ScalingModes.Count; ++i)
			{
				VRBodyComponent.ScalingMode mode = VRBodyComponent.ScalingModes[i];
				characterScalingModeListBox.Add(new MyGuiControlListbox.Item(new StringBuilder(mode.name), mode.tooltip, userData: mode));
			}
			characterScalingModeListBox.ItemClicked += listBox =>
			{
				listBox.SelectSingleItem(listBox.FocusedItem);
				config.bodyScalingModeIndex.value = (int)listBox.FocusedItem.UserData;
			};


			string ResolutionLabelStr() => $"Resolution Scale ({config.resolutionScale:P0})";
			resolutionScaleLabel = new MyGuiControlLabel
			{
				Text = ResolutionLabelStr(),
			};
			resolutionScaleSlider = new MyGuiControlSlider
			{
				MinValue = config.resolutionScale.min,
				MaxValue = config.resolutionScale.max,
				DefaultValue = config.resolutionScale.defaultValue,
				Value = config.resolutionScale.value,
				ValueChanged = (slider) =>
				{
					config.resolutionScale.value = slider.Value;
					resolutionScaleLabel.Text = ResolutionLabelStr();
				},
			};


			string HandActivationPitchLabelStr() => $"Hand Activation Pitch ({config.handActivationPitch:0} degrees)";
			handActivationPitchLabel = new MyGuiControlLabel
			{
				Text = HandActivationPitchLabelStr(),
			};
			handActivationPitchSlider = new MyGuiControlSlider
			{
				MinValue = config.handActivationPitch.min.degrees,
				MaxValue = config.handActivationPitch.max.degrees,
				DefaultValue = config.handActivationPitch.defaultValue.degrees,
				Value = config.handActivationPitch.value.degrees,
				ValueChanged = (slider) =>
				{
					config.handActivationPitch.value = AngleF.Degrees(slider.Value);
					handActivationPitchLabel.Text = HandActivationPitchLabelStr();
				},
			};

			string HandActivationYawLabelStr() => $"Hand Activation Yaw ({config.handActivationYaw.value.degrees:0} degrees)";
			handActivationYawLabel = new MyGuiControlLabel
			{
				Text = HandActivationYawLabelStr(),
			};
			handActivationYawSlider = new MyGuiControlSlider
			{
				MinValue = config.handActivationYaw.min.degrees,
				MaxValue = config.handActivationYaw.max.degrees,
				DefaultValue = config.handActivationYaw.defaultValue.degrees,
				Value = config.handActivationYaw.value.degrees,
				ValueChanged = (slider) =>
				{
					config.handActivationYaw.value = AngleF.Degrees(slider.Value);
					handActivationYawLabel.Text = HandActivationYawLabelStr();
				},
			};


			string HandAimPitchLabelStr() => $"Hand Aim Pitch ({config.handAimPitch:0} degrees)";
			handAimPitchLabel = new MyGuiControlLabel
			{
				Text = HandAimPitchLabelStr(),
			};
			handAimPitchSlider = new MyGuiControlSlider
			{
				MinValue = config.handAimPitch.min.degrees,
				MaxValue = config.handAimPitch.max.degrees,
				DefaultValue = config.handAimPitch.defaultValue.degrees,
				Value = config.handAimPitch.value.degrees,
				ValueChanged = (slider) =>
				{
					config.handAimPitch.value = AngleF.Degrees(slider.Value);
					handAimPitchLabel.Text = HandAimPitchLabelStr();
				},
			};

			string HandAimYawLabelStr() => $"Hand Aim Yaw ({config.handAimYaw.value.degrees:0} degrees)";
			handAimYawLabel = new MyGuiControlLabel
			{
				Text = HandAimYawLabelStr(),
			};
			handAimYawSlider = new MyGuiControlSlider
			{
				MinValue = config.handAimYaw.min.degrees,
				MaxValue = config.handAimYaw.max.degrees,
				DefaultValue = config.handAimYaw.defaultValue.degrees,
				Value = config.handAimYaw.value.degrees,
				ValueChanged = (slider) =>
				{
					config.handAimYaw.value = AngleF.Degrees(slider.Value);
					handAimYawLabel.Text = HandAimYawLabelStr();
				},
			};


			string UIDepthLabelStr() => $"UI Depth ({config.uiDepth:0.00} meters)";
			uiDepthLabel = new MyGuiControlLabel
			{
				Text = UIDepthLabelStr(),
			};
			uiDepthSlider = new MyGuiControlSlider
			{
				MinValue = config.uiDepth.min,
				MaxValue = config.uiDepth.max,
				DefaultValue = config.uiDepth.defaultValue,
				Value = config.uiDepth.value,
				ValueChanged = (slider) =>
				{
					config.uiDepth.value = slider.Value;
					uiDepthLabel.Text = UIDepthLabelStr();
				},
			};


			infoText = new MyGuiControlMultilineText
			{
				Name = "InfoText",
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
				TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
				// TODO: Add 2 short lines of text here if the player needs to know something. Ask for feedback here. Etc.
				Text = new StringBuilder("\r\nThis plugin adds VR for Space Engineers.")
			};

			closeButton = new MyGuiControlButton(originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, text: MyTexts.Get(MyCommonTexts.Ok), onButtonClick: OnOk);
		}

		private void OnOk(MyGuiControlButton _) => CloseScreen();

		private void CreateCheckbox(out MyGuiControlLabel labelControl, out MyGuiControlCheckbox checkboxControl, bool value, Action<bool> store, string label, string tooltip)
		{
			labelControl = new MyGuiControlLabel
			{
				Text = label,
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
			};

			checkboxControl = new MyGuiControlCheckbox(toolTip: tooltip)
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				Enabled = true,
				IsChecked = value
			};
			checkboxControl.IsCheckedChanged += cb => store(cb.IsChecked);
		}

		private void LayoutControls()
		{
			var size = Size ?? Vector2.One;
			layoutTable = new MyLayoutTable(this, -0.3f * size, 0.6f * size);
			layoutTable.SetColumnWidths(400f, 600f);
			layoutTable.SetRowHeights(50f, 50f, 50f, 50f, 50f, 50f, 50f, 50f, 50f, 50f, 80f, 80f);

			var row = 0;

			layoutTable.Add(enableKeyboardAndMouseControlsLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(enableKeyboardAndMouseControlsCheckbox, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(enableCharacterRenderingLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(enableCharacterRenderingCheckbox, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(useHeadRotationForCharacterLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(useHeadRotationForCharacterCheckbox, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(characterScalingModeLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(characterScalingModeListBox, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(resolutionScaleLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(resolutionScaleSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(handActivationPitchLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(handActivationPitchSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(handActivationYawLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(handActivationYawSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(handAimPitchLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(handAimPitchSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(handAimYawLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(handAimYawSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(uiDepthLabel, MyAlignH.Left, MyAlignV.Center, row, 0);
			layoutTable.Add(uiDepthSlider, MyAlignH.Left, MyAlignV.Center, row, 1);
			row++;

			layoutTable.Add(infoText, MyAlignH.Center, MyAlignV.Bottom, row, 0, colSpan: 2);
			row++;

			layoutTable.Add(closeButton, MyAlignH.Center, MyAlignV.Center, row, 0, colSpan: 2);
			// row++;
		}
	}
}
