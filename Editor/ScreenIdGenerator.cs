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
	public class ScreenIdGenerator : AssetPostprocessor {
		static StringBuilder sb = new StringBuilder();

		static ScreenIdGenerator() {
			UnityEngine.Debug.Log($"INITIALIZEONLOAD STATIC CONSTRUCTOR");
			// when unity starts up, add hook to allow Navigator to call this
			Navigator.WantsRegenerate -= Generate;
			Navigator.WantsRegenerate += Generate;


			// read screens from .bytes file in case we lost references
			var screenIds = ScreenIdSerializer.Deserialize();
			if (screenIds == null) { return; }
			Violet.LogVerbose($"Deserialized {screenIds.Count} screens");
			bool hasNewScreens = screenIds.Exists((screenId) =>
				!Enum.TryParse<ScreenId>(screenId.Item1, out _)
			);
			Violet.LogVerbose($"hasNewScreens={hasNewScreens}");
			if (!hasNewScreens) { return; }
			WriteScreenIds(screenIds);
		}

		public static void OnPostProcessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			UnityEngine.Debug.Log($"POSTPROCESS ALL ASSETS");
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded() {
			UnityEngine.Debug.Log($"ONSCRIPTSRELOADED");
		}

		[InitializeOnLoadMethod]
		public static void Poopersnatch() {
			UnityEngine.Debug.Log("POOPERSNATCH");
		}

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
				var name = VioletScreen.Sanitize(screen);
				if (Enum.TryParse<ScreenId>(name, out _)) { continue; }
				ret.Add(name);
			}
			return ret;
		}

		static void AddVioletScreens(List<string> screens) {
			// always include None as the default ScreenId
			var screenIds = new List<Tuple<string, int>>() {};

			// write all existing Enum values - note that unused screens
			// will have to be deleted manually. this is to avoid losing references.
			foreach (ScreenId screenId in Enum.GetValues(typeof(ScreenId))) {
				screenIds.Add(new Tuple<string, int>(
					Enum.GetName(typeof(ScreenId), screenId), (int)screenId
				));
			}

			// write new screen names with incrementing ids
			var nextId = Enum.GetValues(typeof(ScreenId)).Cast<int>().Max() + 1;
			foreach (var screen in screens) {
				screenIds.Add(new Tuple<string, int>(
					screen, nextId++
				));
			}

			WriteScreenIds(screenIds);
		}

		static void WriteScreenIds(List<Tuple<string, int>> screenIds) {
			sb.Clear();
			sb.AppendLine("//Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)");
			sb.AppendLine("namespace VioletUI {");
			sb.AppendLine("\tpublic enum ScreenId {");
			foreach (var screenId in screenIds) {
				sb.AppendLine($"\t\t{screenId.Item1} = {screenId.Item2},");
			}
			sb.AppendLine("\t}");
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


