using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VRageMath;

namespace SpaceEngineersVR.Utils
{
	public struct AngleF : IComparable<AngleF>, IEquatable<AngleF>, IXmlSerializable
	{
		public const double Rad2Deg = 180.0 / Math.PI;
		public const float Rad2DegF = 180f / (float)Math.PI;
		public const double Deg2Rad = Math.PI / 180.0;
		public const float Deg2RadF = (float)Math.PI / 180f;

		public static AngleF Radian(float radians) => new AngleF(radians);
		public static AngleF Degrees(float degrees) => new AngleF(degrees * Deg2RadF);

		private AngleF(float radians)
		{
			this.radians = radians;
		}

		public float radians { get; set; }
		[XmlIgnore]
		public float degrees
		{
			get => radians * Rad2DegF;
			set => radians = value * Deg2RadF;
		}

		public static AngleF operator +(AngleF lhs, AngleF rhs) => Radian(lhs.radians + rhs.radians);
		public static AngleF operator -(AngleF lhs, AngleF rhs) => Radian(lhs.radians - rhs.radians);

		public static float operator /(AngleF lhs, AngleF rhs) => lhs.radians / rhs.radians;

		public static AngleF operator *(AngleF lhs, float rhs) => Radian(lhs.radians * rhs);
		public static AngleF operator /(AngleF lhs, float rhs) => Radian(lhs.radians / rhs);


		public int CompareTo(AngleF other) => radians.CompareTo(other.radians);
		public bool Equals(AngleF other) => radians.Equals(other.radians);

		public XmlSchema GetSchema() => null;
		public void ReadXml(XmlReader reader)
		{
			radians = reader.ReadElementContentAsFloat();
		}
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(radians);
		}
	}
}
