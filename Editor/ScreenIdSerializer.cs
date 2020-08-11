using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
		public static void Serialize() {
			// put enum into a list of strings and ints
			List<Tuple<string, int>> screenIds = new List<Tuple<string, int>>();
			foreach (ScreenId screenId in Enum.GetValues(typeof(ScreenId))) {
				screenIds.Add(new Tuple<string, int>(
					Enum.GetName(typeof(ScreenId), screenId), (int)screenId
				));
			}
			Serialize(screenIds);
		}

		public static void Serialize(List<Tuple<string, int>> screenIds) {
			// create menu if it doesn't exist
			if (!Directory.Exists("Assets/Menus")) {
				Directory.CreateDirectory("Assets/Menus");
			}

			// serialize tuples to binary file
			var formatter = new BinaryFormatter();
			using(var fs = new FileStream("Assets/Menus/ScreenIds.bytes", FileMode.Create)) {
				formatter.Serialize(fs, screenIds);
			}
		}


		public static List<Tuple<string, int>> Deserialize() {
			var formatter = new BinaryFormatter();
			List<Tuple<string, int>> screenIds;
			if (!File.Exists("Assets/Menus/ScreenIds.bytes")) { return null; }
			using(var fs = new FileStream("Assets/Menus/ScreenIds.bytes", FileMode.Open)) {
				screenIds = formatter.Deserialize(fs) as List<Tuple<string, int>>;
			}
			return screenIds;
		}
	}
}