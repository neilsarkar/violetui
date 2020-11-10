using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VioletUI {
	[InitializeOnLoad]
	public class ScreenIdGenerator {
		static StringBuilder sb = new StringBuilder();

		static ScreenIdGenerator() {
			// when unity starts up, add hook to allow Navigator to call this
			Navigator.WantsAddScreens -= AddScreens;
			Navigator.WantsAddScreens += AddScreens;
			Navigator.WantsReplaceScreens -= ReplaceScreens;
			Navigator.WantsReplaceScreens += ReplaceScreens;

			// read screens from .json file in case we lost references
			var screenIds = ScreenIdSerializer.Deserialize();
			if (screenIds == null) { return; }
			Violet.LogVerbose($"Deserialized {screenIds.Count} screens");
			bool hasNewScreens = screenIds.Exists((screenId) =>
				!Enum.TryParse<ScreenId>(screenId.name, out _)
			);
			Violet.LogVerbose($"hasNewScreens={hasNewScreens}");
			if (!hasNewScreens) { return; }
			WriteScreenIds(screenIds);
		}

		public static void ReplaceScreens(VioletScreen[] screens) {
			var screenIds = new List<ScreenIdJson>() { new ScreenIdJson("None", 0) };
			var nextId = 1;
			foreach (var screen in screens) {
				var name = VioletScreen.Sanitize(screen.name);
				var success = Enum.TryParse<ScreenId>(name, out ScreenId screenId);
				// var id = ScreenId.TryParse
				screenIds.Add(new ScreenIdJson(
					name, success ? (int)screenId : nextId++
				));
			}
			WriteScreenIds(screenIds);
		}

		public static void AddScreens(VioletScreen[] screens) {
			var screenStrings = new List<string>(screens.Length);
			foreach (var screen in screens) {
				screenStrings.Add(screen.name);
			}
			var newVioletScreens = Filter(screenStrings);
			if (newVioletScreens.Count == 0) { return; }
			AddVioletScreens(newVioletScreens);
		}

		static List<string> Filter(List<string> screens) {
			var ret = new List<string>();
			foreach (var screen in screens.Distinct()) {
				if (screen == null) { continue; }
				var name = VioletScreen.Sanitize(screen);
				if (Enum.TryParse<ScreenId>(name, out _)) { continue; }
				ret.Add(name);
			}
			return ret;
		}

		static void AddVioletScreens(List<string> screens) {
			// always include None as the default ScreenId
			var screenIds = new List<ScreenIdJson>() { };
			// write all existing Enum values - note that unused screens
			// will have to be deleted manually. this is to avoid losing references.
			foreach (ScreenId screenId in Enum.GetValues(typeof(ScreenId))) {
				screenIds.Add(new ScreenIdJson(
					Enum.GetName(typeof(ScreenId), screenId), (int)screenId
				));
			}
			var nextId = Enum.GetValues(typeof(ScreenId)).Cast<int>().Max() + 1;

			// write new screen names with incrementing ids
			foreach (var screen in screens) {
				screenIds.Add(new ScreenIdJson(
					screen, nextId++
				));
			}

			WriteScreenIds(screenIds);
		}

		static void WriteScreenIds(List<ScreenIdJson> screenIds) {
			sb.Clear();
			sb.AppendLine("//Names are automatically added through ScreenIdGenerator.cs, deletions are done manually in Assets/Plugins/VioletUI/ScreenIds.json :)");
			sb.AppendLine("public enum ScreenId {");
			foreach (var screenId in screenIds) {
				sb.AppendLine($"\t{screenId.name} = {screenId.id},");
			}
			sb.AppendLine("}");
			File.WriteAllText(packagePath(), sb.ToString());
			Violet.Log("Regenerating the ScreenId enum, this may take a second to recompile.");
			AssetDatabase.Refresh();
			ScreenIdSerializer.Serialize(screenIds);
		}

		static string packagePath() {
			return $"{packageBasePath()}/Runtime/Navigation/ScreenId.cs";
		}

		static string scriptPath() {
			return "Assets/Plugins/VioletUI/ScreenId.cs";
		}
		static string packageBasePath() {
			foreach (var path in packagePaths) {
				var paths = Directory.GetDirectories(path, "*violetui*", SearchOption.TopDirectoryOnly);
				if (paths.Length > 0) { return paths[0]; }
			}
			throw new VioletException("Can't find violetui in Library/PackageCache or Packages. Please report a bug.");
		}
		static string[] packagePaths = new string[] {
			Path.GetFullPath("Library/PackageCache"),
			Path.GetFullPath("Packages")
		};
	}
}


