using SpaceEngineersVR.Player.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRageMath;

namespace SpaceEngineersVR.Config
{
	public class PluginConfig : INotifyPropertyChanged
	{
		public struct SliderData
		{
			public SliderData(float min, float max, float initial)
			{
				this.min = min;
				this.max = max;
				this.initial = initial;
			}

			public float min;
			public float max;
			public float initial;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void SetValue<T>(ref T field, T value, [CallerMemberName] string propName = "")
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return;

			field = value;

			OnPropertyChanged(propName);
		}

		private void OnPropertyChanged([CallerMemberName] string propName = "")
		{
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged == null)
				return;

			propertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		private bool enableKeyboardAndMouseControlsValue = true;
		private bool enableCharacterRenderingValue = true;

		private bool useHeadRotationForCharacterValue = true;

		private int bodyScalingModeIndexValue = VRBodyComponent.DefaultScalingMode;


		private float playerHeightValue = 1.69f;
		private float playerArmSpanValue = 1.66f;


		public static readonly SliderData ResolutionScaleData = new SliderData(0.1f, 2.0f, 1.0f);
		private float resolutionScaleValue = ResolutionScaleData.initial;


		public static readonly SliderData HandActivationPitchData = new SliderData(-180f, 180f, -90f);
		private float handActivationPitchValue = MathHelper.ToRadians(HandActivationPitchData.initial);
		public static readonly SliderData HandActivationYawData = new SliderData(-180f, 180f, 0f);
		private float handActivationYawValue = MathHelper.ToRadians(HandActivationYawData.initial);

		public static readonly SliderData HandAimPitchData = new SliderData(-180f, 180f, 0f);
		private float handAimPitchValue = MathHelper.ToRadians(HandAimPitchData.initial);
		public static readonly SliderData HandAimYawData = new SliderData(-180f, 180f, 0f);
		private float handAimYawValue = MathHelper.ToRadians(HandAimYawData.initial);


		public bool enableKeyboardAndMouseControls
		{
			get => enableKeyboardAndMouseControlsValue;
			set => SetValue(ref enableKeyboardAndMouseControlsValue, value);
		}

		public bool enableCharacterRendering
		{
			get => enableCharacterRenderingValue;
			set => SetValue(ref enableCharacterRenderingValue, value);
		}

		public bool useHeadRotationForCharacter
		{
			get => useHeadRotationForCharacterValue;
			set => SetValue(ref useHeadRotationForCharacterValue, value);
		}

		public float playerHeight
		{
			get => playerHeightValue;
			set => SetValue(ref playerHeightValue, value);
		}
		public float playerArmSpan
		{
			get => playerArmSpanValue;
			set => SetValue(ref playerArmSpanValue, value);
		}

		public int bodyScalingModeIndex
		{
			get => bodyScalingModeIndexValue;
			set => SetValue(ref bodyScalingModeIndexValue, value);
		}

		public float resolutionScale
		{
			get => resolutionScaleValue;
			set => SetValue(ref resolutionScaleValue, value);
		}

		public float handActivationPitch
		{
			get => handActivationPitchValue;
			set => SetValue(ref handActivationPitchValue, value);
		}
		public float handActivationYaw
		{
			get => handActivationYawValue;
			set => SetValue(ref handActivationYawValue, value);
		}

		public float handAimPitch
		{
			get => handAimPitchValue;
			set => SetValue(ref handAimPitchValue, value);
		}
		public float handAimYaw
		{
			get => handAimYawValue;
			set => SetValue(ref handAimYawValue, value);
		}
	}
}
