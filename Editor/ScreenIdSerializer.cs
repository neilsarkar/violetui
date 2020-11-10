using System;
using System.Collections.Generic;
using System.IO;

namespace VioletUI {
	/// <summary>
	/// ScreenIdSerializer is used as a workaround for enum generation with package updating.
	///
	/// Since the enum is kept in the package folder and the package folder is deleted on upgrade,
	/// we need a way of storing the enum to a file and then reading it back out
	///
	/// Storing and copying a .cs file wouldn't work since it would cause compile errors when the file was duplicated.
	/// </summary>
	public class ScreenIdSerializer {
		const string basePath = "Assets/Plugins/VioletUI";
		static string path => $"{basePath}/ScreenIds.json";

		public static void Serialize(List<ScreenIdJson> screenIds) {
			if (!Directory.Exists(basePath)) { Directory.CreateDirectory(basePath); }

			var json = UnityEngine.JsonUtility.ToJson(new ScreenIdsJson(screenIds), true);
			File.WriteAllText(path, json);
		}


		public static List<ScreenIdJson> Deserialize() {
			if (!File.Exists(path)) {
				return new List<ScreenIdJson>() {
					new ScreenIdJson("None", 0)
				};
			}
			var json = UnityEngine.JsonUtility.FromJson<ScreenIdsJson>(File.ReadAllText(path));
			return json.screenIds;
		}
	}

	[Serializable]
	public struct ScreenIdJson {
		public string name;
		public int id;

		public ScreenIdJson(string name, int id) {
			this.name = name;
			this.id = id;
		}
	}

	[Serializable]
	public struct ScreenIdsJson {
		public List<ScreenIdJson> screenIds;

		public ScreenIdsJson(List<ScreenIdJson> screenIds) {
			this.screenIds = screenIds;
		}
	}
}