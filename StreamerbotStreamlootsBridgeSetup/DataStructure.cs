using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerbotStreamlootsBridge
{
	public class StreamlootsReply
	{
		public List<CardPack> data { get; set; }
	}

	public class CardPack
	{
		public string _id { get; set; }
		public string name { get; set; }
	}

	public class Settings
	{
		public string authToken { get; set; }
		public string packId { get; set; }
	}

	public class RequestStructure
	{
		public string packId { get; set; }
		public string user { get; set; }
		public int cardAmount { get; set; }
		public int packAmount { get; set; }
	}

	public static class SettingsSaveLoader
	{
		public static Settings ReadSettings()
		{
			string settingsPath = Directory.GetCurrentDirectory() + "\\config.json";
			if (!File.Exists(settingsPath))
				SaveSettings(new Settings());
			return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));
		}

		public static void SaveSettings(Settings settings)
		{
			string settingsPath = Directory.GetCurrentDirectory() + "\\config.json";
			File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
		}
	}
}
