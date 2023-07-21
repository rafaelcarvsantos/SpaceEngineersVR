using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace SpaceEngineersVR.Plugin
{
	public static class Assets
	{
		public static string Folder { get; private set; }

		public static Icon Icon { get; private set; }
		public static string IconPngPath { get; private set; }
		public static string IconIcoPath { get; private set; }
		public static string ActionJsonPath { get; private set; }

		public static readonly string ConfigFileName = $"{Main.Name}.cfg";

		static Assets()
		{
			SetFolder(GetDefaultFolder());
		}

		public static void SetFolder(string folder)
		{
			if (string.IsNullOrEmpty(folder))
				throw new ArgumentException("Folder must not be null", folder);
			if (!Directory.Exists(folder))
				throw new DirectoryNotFoundException("Asset folder not found");
			Folder = folder;
			Icon = new Icon(Path.Combine(folder, "icon.ico"));
			IconPngPath = Path.Combine(folder, "logo.png");
			IconIcoPath = Path.Combine(folder, "logo.ico");
			ActionJsonPath = Path.Combine(folder, "Controls", "actions.json");
		}

		public static string GetDefaultFolder()
		{
			string assemblyLocation = Assembly.GetExecutingAssembly().Location;
			if (string.IsNullOrEmpty(assemblyLocation))
				return null;
			return Path.Combine(Path.GetDirectoryName(assemblyLocation), "SEVRAssets");
		}

		public static string GetPluginsFolder()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
