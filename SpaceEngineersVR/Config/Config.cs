using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VRage.Utils;

namespace SpaceEngineersVR.Config
{
	public class Config
	{
		public Value<bool> enableKeyboardAndMouseControls = new Value<bool>(true,
			_ => "Enable Keyboard And Mouse Controls",
			"Enables keyboard and mouse controls.");
		public Value<bool> enableCharacterRendering = new Value<bool>(true,
			_ => "Enable Character Rendering",
			"When unchecked the player's character will not be visible.");

		public Value<bool> useHeadRotationForCharacter = new Value<bool>(true,
			_ => "Use Head Rotation For Character",
			"Character turns when you turn your head, otherwise they always face your SteamVR forward direction.");

		public Range<int> bodyScalingModeIndex = new Range<int>(0, VRBodyComponent.ScalingModes.Count - 1, VRBodyComponent.DefaultScalingMode,
			_ => "Character Scaling Mode",
			"Changes if/how your real-world motions are scaled in-game.");


		public Slider<float> playerHeight = new Slider<float>(0.1f, 2.5f, 1.69f, 0.01f,
			v => $"Player height ({v:0.00} meters)",
			"The real-world height to your headset in meters. Press Numpad-0 in game to calibrate this (measures over a 5 second period).");
		public Slider<float> playerArmSpan = new Slider<float>(0.1f, 2.5f, 1.66f, 0.01f,
			v => $"Player arm span ({v:0.00} meters)",
			"Your real-world arm span in meters. Press Numpad-0 in game and then T pose to calibrate this (measures over a 5 second period).");


		public Slider<float> resolutionScale = new Slider<float>(0.1f, 2.0f, 1.0f, 0.05f,
				v => $"Resolution Scale ({v:P0})",
				"Changes the ingame resolution to be higher or lower than the headsets to increase image quality at the cost of performance, and vice versa.");


		public Slider<AngleF> handActivationPitch = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(-90f), AngleF.Degrees(1f),
				v => $"Hand Activation Pitch ({v.degrees:0} degrees)",
				"Adjusts the pitch of the ray from the primary hand used to interact with things like button.");
		public Slider<AngleF> handActivationYaw = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(0f), AngleF.Degrees(1f),
				v => $"Hand Activation Yaw ({v.degrees:0} degrees)",
				"Adjusts the yaw of the ray from the primary hand used to interact with things like button.");

		public Slider<AngleF> handAimPitch = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(-60f), AngleF.Degrees(1f),
				v => $"Hand Aim Pitch ({v.degrees:0} degrees)",
				"Adjusts the pitch of tools and weapons when held.");
		public Slider<AngleF> handAimYaw = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(0f), AngleF.Degrees(1f),
				v => $"Hand Aim Yaw ({v.degrees:0} degrees)",
				"Adjusts the yaw of tools and weapons when held.");

		public Slider<float> uiDepth = new Slider<float>(0.1f, 10f, 0.5f, 0.01f,
				v => $"UI Depth ({v:0.00} meters)",
				"Changes the distance the UI is from your head position.");
		public Slider<float> uiWidth = new Slider<float>(0.1f, 2.5f, 2f, 0.01f,
				v => $"UI Width ({v:0.00} meters)",
				"Changes the size of the UI.");


		private IEnumerable<IValue> values()
		{
			return typeof(Config).GetFields().Where(f => f.FieldType.GetInterfaces().Contains(typeof(IValue))).Select(f => f.GetValue(this)).Cast<IValue>();
		}

		private interface IValue
		{
			void Load(IValue from);
		}

		public class ValueBase<T> : IXmlSerializable, IValue
		{
			public ValueBase() //needed for deserialization
			{
			}
			public ValueBase(T defaultValue, Func<T, string> labelGenerator, string tooltip)
			{
				this.defaultValue = defaultValue;
				internalValue = defaultValue;
				this.labelGenerator = labelGenerator;
				this.tooltip = tooltip;
			}

			public readonly T defaultValue;
			protected T internalValue;
			public event Action<T> onValueChanged;

			public readonly Func<T, string> labelGenerator;
			public readonly string tooltip;

			protected void ValueChanged(T value) { onValueChanged.InvokeIfNotNull(value); }

			public XmlSchema GetSchema() => null;
			public void ReadXml(XmlReader reader)
			{
				if (internalValue is IXmlSerializable serializable)
				{
					serializable.ReadXml(reader);
					return;
				}

				internalValue = (T)reader.ReadElementContentAs(typeof(T), null);
			}
			public void WriteXml(XmlWriter writer)
			{
				if (internalValue is IXmlSerializable serializable)
				{
					serializable.WriteXml(writer);
					return;
				}

				writer.WriteValue(internalValue);
			}

			void IValue.Load(IValue from)
			{
				internalValue = ((ValueBase<T>)from).internalValue;
			}
		}

		public class Value<T> : ValueBase<T>
		{
			public Value() : base()
			{
			}
			public Value(T defaultValue, Func<T, string> labelGenerator, string tooltip) : base(defaultValue, labelGenerator, tooltip)
			{
			}

			public T value
			{
				get => internalValue;
				set
				{
					internalValue = value;
					ValueChanged(internalValue);
					Main.Config.SaveLater();
				}
			}
		}

		public class Range<T> : ValueBase<T> where T : IComparable<T>
		{
			public Range() : base()
			{
			}
			public Range(T min, T max, T defaultValue, Func<T, string> labelGenerator, string tooltip) : base(defaultValue, labelGenerator, tooltip)
			{
				this.min = min;
				this.max = max;
			}

			public readonly T min;
			public readonly T max;

			public T value
			{
				get => internalValue;
				set
				{
					if (value.CompareTo(min) < 0)
						value = min;
					else if (value.CompareTo(max) > 0)
						value = max;

					internalValue = value;
					ValueChanged(internalValue);
					Main.Config.SaveLater();
				}
			}
		}

		public class Slider<T> : Range<T> where T : IComparable<T>
		{
			public Slider() : base()
			{
			}
			public Slider(T min, T max, T defaultValue, T snap, Func<T, string> labelGenerator, string tooltip) : base(min, max, defaultValue, labelGenerator, tooltip)
			{
				this.snap = snap;
			}

			public readonly T snap;
		}


		private readonly Timer saveConfigTimer;
		private const int SaveDelay = 500;

		private string path;

		public Config()
		{
			saveConfigTimer = new Timer(x => Save());
		}

		public void SaveLater()
		{
			saveConfigTimer.Change(SaveDelay, -1);
		}

		public void Save()
		{
			using (StreamWriter text = File.CreateText(path))
				new XmlSerializer(typeof(Config)).Serialize(text, this);
		}

		public static Config Load(string path)
		{
			Config config = new Config
			{
				path = path
			};
			try
			{
				if (File.Exists(path))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
					using (StreamReader streamReader = File.OpenText(path))
					{
						//XmlSerializer overwrites each field/property with default constructed objects, so all our values lose the defaultValue, min, max, etc.
						//so we load a dummy config, then extract the loaded values into the real config
						Config loaded = (Config)xmlSerializer.Deserialize(streamReader);
						foreach((IValue value, IValue load) in config.values().Zip(loaded.values(), (value, load) => (value, load)))
						{
							value.Load(load);
						}
						return config;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "Failed to load configuration file: {0}", path);
				try
				{
					if (File.Exists(path))
					{
						string timestamp = DateTime.Now.ToString("yyyyMMdd-hhmmss");
						string corruptedPath = $"{path}.corrupted.{timestamp}.txt";
						Logger.Info("Moving corrupted configuration file: {0} => {1}", path, corruptedPath);
						File.Move(path, corruptedPath);
					}
				}
				catch (Exception)
				{
					// Ignored
				}
			}

			{
				MyLog.Default.WriteLine($"SpaceEngineersVR: Writing default configuration file: {path}");
				config.Save();
				return config;
			}
		}

	}
}
