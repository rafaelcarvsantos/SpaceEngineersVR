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

			PluginConfig config = Main.Config;
			CreateCheckbox(
				out enableKeyboardAndMouseControlsLabel, out enableKeyboardAndMouseControlsCheckbox,
				config.enableKeyboardAndMouseControls.value, value => config.enableKeyboardAndMouseControls.value = value,
				"Enable Keyboard And Mouse Controls",
				"Enables keyboard and mouse controls.");
			CreateCheckbox(out enableCharacterRenderingLabel, out enableCharacterRenderingCheckbox,
				config.enableCharacterRendering.value, value => config.enableCharacterRendering.value = value,
				"Enable Character Rendering",
				"When unchecked the player's character will not be visible.");
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


			CreateSlider(out resolutionScaleLabel, out resolutionScaleSlider, Main.Config.resolutionScale,
				v => v, v => v,
				v => $"Resolution Scale ({v:P0})",
				"Changes the ingame resolution to be higher or lower than the headsets to increase image quality at the cost of performance, and vice versa.");

			CreateSlider(out handActivationPitchLabel, out handActivationPitchSlider, Main.Config.handActivationPitch,
				v => v.radians, v => AngleF.Radian(v),
				v => $"Hand Activation Pitch ({v.degrees:0} degrees)",
				"Adjusts the pitch of the ray from the primary hand used to interact with things like button.");
			CreateSlider(out handActivationYawLabel, out handActivationYawSlider, Main.Config.handActivationYaw,
				v => v.radians, v => AngleF.Radian(v),
				v => $"Hand Activation Yaw ({v.degrees:0} degrees)",
				"Adjusts the yaw of the ray from the primary hand used to interact with things like button.");

			CreateSlider(out handAimPitchLabel, out handAimPitchSlider, Main.Config.handAimPitch,
				v => v.radians, v => AngleF.Radian(v),
				v => $"Hand Aim Pitch ({v.degrees:0} degrees)",
				"Adjusts the pitch of tools and weapons when held.");
			CreateSlider(out handAimYawLabel, out handAimYawSlider, Main.Config.handAimYaw,
				v => v.radians, v => AngleF.Radian(v),
				v => $"Hand Aim Yaw ({v.degrees:0} degrees)",
				"Adjusts the yaw of tools and weapons when held.");

			CreateSlider(out uiDepthLabel, out uiDepthSlider, Main.Config.uiDepth,
				v => v, v => v,
				v => $"UI Depth ({config.uiDepth:0.00} meters)",
				"Changes the distance the UI is from your head position.");



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
		private void CreateSlider<T>(out MyGuiControlLabel labelControl, out MyGuiControlSlider sliderControl, PluginConfig.Range<T> range, Func<T, float> get, Func<float, T> set, Func<T, string> labelText, string tooltip) where T : IComparable<T>
		{
			labelControl = new MyGuiControlLabel()
			{
				Text = labelText(range.value),
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
			};

			MyGuiControlLabel labelCapture = labelControl;
			sliderControl = new MyGuiControlSlider(toolTip: tooltip)
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				Enabled = true,
				MinValue = get(range.min),
				MaxValue = get(range.max),
				DefaultValue = get(range.defaultValue),
				Value = get(range.value),
				ValueChanged = (control) =>
				{
					range.value = set(control.Value);
					labelCapture.Text = labelText(range.value);
				},
			};

			if(range is PluginConfig.Slider<T> slider)
			{
				sliderControl.SnapSliderToSteps = true;
				sliderControl.StepLength = sliderControl.Propeties.ValueToRatio(get(slider.snap));
			}
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
