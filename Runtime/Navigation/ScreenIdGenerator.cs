using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VioletUI {
	public class ScreenIdGenerator {
		static StringBuilder sb = new StringBuilder();

		public static void Generate(Screen screen) {
			Generate(new Screen[] {screen});
		}

		public static void Generate(Screen[] screens) {
			var screenStrings = new List<string>(screens.Length);
			foreach (var screen in screens) {
				screenStrings.Add(screen.name);
			}
			Generate(screenStrings);
		}

		public static void Generate(List<string> screens) {
			var newScreens = Filter(screens);
			if (newScreens.Count == 0) { return; }
			AddScreens(newScreens);
		}

		static List<string> Filter(List<string> screens) {
			var ret = new List<string>();
			foreach (var screen in screens) {
				if (screen == null) { continue; }
				var name = sanitize(screen);
				if (Enum.TryParse<ScreenId>(screen, out _)) { continue; }
				ret.Add(name);
			}
			return ret;
		}

		static void AddScreens(List<string> screens) {
			sb.Clear();
			sb.AppendLine("//Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)");
			sb.AppendLine("namespace VioletUI {");
			sb.AppendLine("\tpublic enum ScreenId {");
			sb.AppendLine("\t\tNone = 0,");

			// write all existing Enum values - note that unused screens
			// will have to be deleted manually. this is to avoid losing references.
			foreach (ScreenId screenId in Enum.GetValues(typeof(ScreenId))) {
				if (screenId == ScreenId.None) { continue; }
				sb.AppendLine($"\t\t{Enum.GetName(typeof(ScreenId), screenId)} = {(int)screenId},");
			}

			// write any new screen names with incrementing ids
			var nextId = Enum.GetValues(typeof(ScreenId)).Cast<int>().Max() + 1;
			foreach (var screen in screens) {
				sb.AppendLine($"\t\t{screen} = {nextId++},");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			string path = Path.GetFullPath("Packages/violetui/Runtime/Navigation/ScreenId.cs");
			File.WriteAllText(path, sb.ToString());
			AssetDatabase.Refresh();
		}

		static string sanitize(string s) {
			return s.Replace(" ", "");
		}
	}
}
