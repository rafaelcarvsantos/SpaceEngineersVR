using Sandbox;
using Sandbox.Graphics.GUI;
using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Utils;
using VRageMath;

namespace SpaceEngineersVR.GUI
{
	public class ConfigDialog : MyGuiScreenBase
	{
		private const string Caption = "Space Engineers VR Configuration";
		public override string GetFriendlyName() => "MyPluginConfigDialog";

		private MyLayoutTable layoutTable;

		private struct LabelControlPair
		{
			public MyGuiControlLabel label;
			public MyGuiControlBase control;
		}

		private readonly List<LabelControlPair> configControls = new List<LabelControlPair>();

		public ConfigDialog() : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.7f, 0.8f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
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
		}

		private void CreateControls()
		{
			Config.Config config = Main.Config;

			configControls.Clear();

			CreateCheckbox(config.enableKeyboardAndMouseControls);
			CreateCheckbox(config.enableCharacterRendering);
			CreateCheckbox(config.useHeadRotationForCharacter);

			CreateScalingModeListBox(config.bodyScalingModeIndex);

			CreateSlider(config.playerHeight, v => v, v => v);
			CreateSlider(config.playerArmSpan, v => v, v => v);

			CreateSlider(config.resolutionScale, v => v, v => v);

			CreateSlider(config.handActivationPitch,
				v => v.degrees, v => AngleF.Degrees(v));
			CreateSlider(config.handActivationYaw,
				v => v.degrees, v => AngleF.Degrees(v));

			CreateSlider(config.handAimPitch,
				v => v.degrees, v => AngleF.Degrees(v));
			CreateSlider(config.handAimYaw,
				v => v.degrees, v => AngleF.Degrees(v));

			CreateSlider(config.uiDepth,
				v => v, v => v);
			CreateSlider(config.uiWidth,
				v => v, v => v);


			AddCaption(Caption);

			Vector2 size = Size ?? Vector2.One;
			layoutTable = new MyLayoutTable(this, -0.3f * size, 0.6f * size);
			layoutTable.SetColumnWidths(400f, 600f);

			float[] rowHeights = new float[configControls.Count + 1];
			for (int i = 0; i < configControls.Count; ++i)
			{
				rowHeights[i] = 50f;
			}
			rowHeights[configControls.Count + 0] = 80f;
			layoutTable.SetRowHeights(rowHeights);

			int row = 0;
			for (; row < configControls.Count; ++row)
			{
				LabelControlPair pair = configControls[row];
				layoutTable.Add(pair.label, MyAlignH.Left, MyAlignV.Center, row, 0);
				layoutTable.Add(pair.control, MyAlignH.Left, MyAlignV.Center, row, 1);
			}

			MyGuiControlButton closeButton = new MyGuiControlButton(originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, text: MyTexts.Get(MyCommonTexts.Ok), onButtonClick: OnOk);
			layoutTable.Add(closeButton, MyAlignH.Center, MyAlignV.Center, row, 0, colSpan: 2);
			row++;
		}

		private void OnOk(MyGuiControlButton _) => CloseScreen();

		private void CreateCheckbox(Config.Config.Value<bool> value)
		{
			MyGuiControlLabel labelControl = new MyGuiControlLabel
			{
				Text = value.labelGenerator(value.value),
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
			};

			MyGuiControlCheckbox checkboxControl = new MyGuiControlCheckbox(toolTip: value.tooltip)
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				Enabled = true,
				IsChecked = value.value,
			};
			checkboxControl.IsCheckedChanged += control =>
			{
				value.value = control.IsChecked;
			};

			configControls.Add(new LabelControlPair() { label = labelControl, control = checkboxControl });
		}
		private void CreateSlider<T>(Config.Config.Range<T> range, Func<T, float> get, Func<float, T> set) where T : IComparable<T>
		{
			MyGuiControlLabel labelControl = new MyGuiControlLabel()
			{
				Text = range.labelGenerator(range.value),
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
			};

			MyGuiControlSlider sliderControl = new MyGuiControlSlider(toolTip: range.tooltip)
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
					labelControl.Text = range.labelGenerator(range.value);
				},
			};

			if (range is Config.Config.Slider<T> slider)
			{
				sliderControl.SnapSliderToSteps = true;
				sliderControl.StepLength = get(slider.snap) / (sliderControl.MaxValue - sliderControl.MinValue);
			}

			configControls.Add(new LabelControlPair() { label = labelControl, control = sliderControl });
		}

		private void CreateScalingModeListBox(Config.Config.Range<int> range)
		{
			MyGuiControlLabel label = new MyGuiControlLabel()
			{
				Text = range.labelGenerator(range.value),
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
			};
			MyGuiControlListbox listBox = new MyGuiControlListbox()
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
				Enabled = true,
			};
			for (int i = 0; i < VRBodyComponent.ScalingModes.Count; ++i)
			{
				VRBodyComponent.ScalingMode mode = VRBodyComponent.ScalingModes[i];
				listBox.Add(new MyGuiControlListbox.Item(new StringBuilder(mode.name), mode.tooltip, userData: mode));
			}
			listBox.ItemClicked += c =>
			{
				c.SelectSingleItem(c.FocusedItem);
				range.value = (int)c.FocusedItem.UserData;
				label.Text = range.labelGenerator(range.value);
			};
			configControls.Add(new LabelControlPair() { label = label, control = listBox });
		}
	}
}
