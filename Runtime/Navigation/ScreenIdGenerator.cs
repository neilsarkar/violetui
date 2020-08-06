using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace VioletUI {
	public class ScreenIdGenerator {
		static StringBuilder sb = new StringBuilder();

		public static void Generate(VioletScreen screen) {
			Generate(new VioletScreen[] {screen});
		}

		public static void Generate(VioletScreen[] screens) {
			var screenStrings = new List<string>(screens.Length);
			foreach (var screen in screens) {
				screenStrings.Add(screen.name);
			}
			Generate(screenStrings);
		}

		public static void Generate(List<string> screens) {
			var newVioletScreens = Filter(screens);
			if (newVioletScreens.Count == 0) { return; }
			AddVioletScreens(newVioletScreens);
		}

		static List<string> Filter(List<string> screens) {
			var ret = new List<string>();
			foreach (var screen in screens.Distinct()) {
				if (screen == null) { continue; }
				var name = Sanitize(screen);
				if (Enum.TryParse<ScreenId>(name, out _)) { continue; }
				ret.Add(name);
			}
			return ret;
		}

		static void AddVioletScreens(List<string> screens) {
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
			File.WriteAllText(packagePath(), sb.ToString());
			Violet.Log("Regenerating the ScreenId enum, this may take a second to recompile.");
			AssetDatabase.Refresh();
		}

		public static string Sanitize(string s) {
			return s.Replace(" ", "");
		}

		static string packagePath() {
			return $"{packageBasePath()}/Runtime/Navigation/ScreenId.cs";
		}

		static string scriptPath() {
			return "Assets/Menus/ScreenId.cs";
		}
		static string packageBasePath() {
			foreach (var path in packagePaths) {
				var paths = Directory.GetDirectories(path, "*violetui*", SearchOption.TopDirectoryOnly);
				if (paths.Length > 0) { return paths[0];}
			}
			throw new VioletException("Can't find violetui in Library/PackageCache or Packages. Please report a bug.");
		}
		static string[] packagePaths = new string[] {
			Path.GetFullPath("Library/PackageCache"),
			Path.GetFullPath("Packages")
		};
	}
}


