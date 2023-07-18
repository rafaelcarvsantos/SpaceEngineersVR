using SpaceEngineersVR.Player.Components;
using SpaceEngineersVR.Plugin;
using SpaceEngineersVR.Utils;
using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VRage.Utils;
using static System.BitStreamExtensions;

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



		public struct Value<T> : IXmlSerializable
		{
			public Value(T defaultValue)
			{
				this.defaultValue = defaultValue;
				internalValue = defaultValue;
				onValueChanged = null;
			}

			public readonly T defaultValue;

			public T value
			{
				get => internalValue;
				set
				{
					internalValue = value;
					onValueChanged.InvokeIfNotNull(internalValue);
					Common.Config.SaveLater();
				}
			}

			private T internalValue;

			public event Action<T> onValueChanged;

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
		}

		public struct Range<T> : IXmlSerializable where T : IComparable<T>
		{
			public Range(T min, T max, T defaultValue)
			{
				this.defaultValue = defaultValue;
				this.min = min;
				this.max = max;
				internalValue = defaultValue;
				onValueChanged = null;
			}

			public readonly T defaultValue;
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
					onValueChanged.InvokeIfNotNull(internalValue);
					Common.Config.SaveLater();
				}
			}

			private T internalValue;

			public event Action<T> onValueChanged;

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

				writer.WriteString(internalValue.ToString());
			}
		}

		public struct Slider<T> : IXmlSerializable where T : IComparable<T>
		{
			public Slider(T min, T max, T defaultValue, T snap)
			{
				this.min = min;
				this.max = max;
				this.defaultValue = defaultValue;
				this.snap = snap;

				internalValue = defaultValue;

				onValueChanged = null;
			}

			public readonly T min;
			public readonly T max;
			public readonly T defaultValue;
			public readonly T snap;

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
					onValueChanged.InvokeIfNotNull(internalValue);
					Common.Config.SaveLater();
				}
			}

			private T internalValue;

			public event Action<T> onValueChanged;

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

				writer.WriteString(internalValue.ToString());
			}
		}


		private Timer saveConfigTimer;
		private const int SaveDelay = 500;

		private string path;

		private void SaveLater()
		{
			if (saveConfigTimer == null)
				saveConfigTimer = new Timer(x => Save());

			saveConfigTimer.Change(SaveDelay, -1);
		}

		private void Save()
		{
			using (StreamWriter text = File.CreateText(path))
				new XmlSerializer(typeof(PluginConfig)).Serialize(text, this);
		}

		public static PluginConfig Load(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(PluginConfig));
					using (StreamReader streamReader = File.OpenText(path))
					{
						PluginConfig config = (PluginConfig)xmlSerializer.Deserialize(streamReader);
						config.path = path;
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
				PluginConfig config = new PluginConfig
				{
					path = path
				};
				config.Save();
				return config;
			}
		}

	}
}
