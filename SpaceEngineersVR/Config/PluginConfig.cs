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
	public class PluginConfig
	{
		public Value<bool> enableKeyboardAndMouseControls = new Value<bool>(true);
		public Value<bool> enableCharacterRendering = new Value<bool>(true);

		public Value<bool> useHeadRotationForCharacter = new Value<bool>(true);

		public Range<int> bodyScalingModeIndex = new Range<int>(0, VRBodyComponent.ScalingModes.Count - 1, VRBodyComponent.DefaultScalingMode);


		public Value<float> playerHeight = new Value<float>(1.69f);
		public Value<float> playerArmSpan = new Value<float>(1.66f);


		public Slider<float> resolutionScale = new Slider<float>(0.1f, 2.0f, 1.0f, 0.05f);


		public Slider<AngleF> handActivationPitch = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(-90f), AngleF.Degrees(1f));
		public Slider<AngleF> handActivationYaw = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(0f), AngleF.Degrees(1f));

		public Slider<AngleF> handAimPitch = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(-60f), AngleF.Degrees(1f));
		public Slider<AngleF> handAimYaw = new Slider<AngleF>(AngleF.Degrees(-180f), AngleF.Degrees(180f), AngleF.Degrees(0f), AngleF.Degrees(1f));

		public Slider<float> uiDepth = new Slider<float>(0.1f, 10f, 0.5f, 0.1f);


		private IEnumerable<IValue> values()
		{
			return typeof(PluginConfig).GetFields().Where(f => f.FieldType.IsSubclassOf(typeof(IValue))).Cast<IValue>();
		}

		private interface IValue
		{
			void Load(IValue from);
		}

		public class ValueBase<T> : IXmlSerializable, IValue
		{
			public ValueBase()
			{
			}
			public ValueBase(T defaultValue)
			{
				this.defaultValue = defaultValue;
				internalValue = defaultValue;
			}

			public readonly T defaultValue;
			protected T internalValue;
			public event Action<T> onValueChanged;

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
			public Value(T defaultValue) : base(defaultValue)
			{
			}

			public T value
			{
				get => internalValue;
				set
				{
					internalValue = value;
					ValueChanged(internalValue);
					Assets.Config.SaveLater();
				}
			}
		}

		public class Range<T> : ValueBase<T> where T : IComparable<T>
		{
			public Range() : base()
			{
			}
			public Range(T min, T max, T defaultValue) : base(defaultValue)
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
					Assets.Config.SaveLater();
				}
			}
		}

		public class Slider<T> : Range<T> where T : IComparable<T>
		{
			public Slider() : base()
			{
			}
			public Slider(T min, T max, T defaultValue, T snap) : base(min, max, defaultValue)
			{
				this.snap = snap;
			}

			public readonly T snap;
		}


		private readonly Timer saveConfigTimer;
		private const int SaveDelay = 500;

		private string path;

		public PluginConfig()
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
				new XmlSerializer(typeof(PluginConfig)).Serialize(text, this);
		}

		public static PluginConfig Load(string path)
		{
			PluginConfig config = new PluginConfig
			{
				path = path
			};
			try
			{
				if (File.Exists(path))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(PluginConfig));
					using (StreamReader streamReader = File.OpenText(path))
					{
						//XmlSerializer overwrites each field/property with default constructed objects, so all our values lose the defaultValue, min, max, etc.
						//so we load a dummy config, then extract the loaded values into the real config
						PluginConfig loaded = (PluginConfig)xmlSerializer.Deserialize(streamReader);
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
