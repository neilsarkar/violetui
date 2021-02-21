using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace VioletUI {
	public class PrefabListener : UnityEditor.AssetModificationProcessor {
		static string[] OnWillSaveAssets(string[] paths) {
			foreach (var path in paths) {
				if (path.Contains(".prefab")) {
					UpdateRepeatViews(path);
				}
			}
			return paths;
		}

		static void UpdateRepeatViews(string prefabPath) {
			IRepeatView repeatView;
			HashSet<GameObject> processed = new HashSet<GameObject>();

			// https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html?_ga=2.70405528.968528738.1611110066-121275311.1591173442
			foreach (var go in Resources.FindObjectsOfTypeAll<TidyBehaviour>()) {
				// check hideFlags
				try {
					if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave) {
						continue;
					}
				} catch(MissingReferenceException) { continue; }
				// ignore unity things that aren't in a scene
				if (EditorUtility.IsPersistent(go.transform.root.gameObject)) { continue; }
				// ignore TidyBehaviours without RepeatViews and RepeatViews without ViewPrefabs
				if ((repeatView = go.GetComponent<IRepeatView>())?.ViewPrefab == null) { continue; }
				// ignore gameObjects that aren't in a scene
				if (!go.gameObject.scene.path.Contains(".unity")) { continue; }
				// ignore gameObjects we've already processed
				if (processed.Contains(go.gameObject)) { continue; }

				if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(repeatView.ViewPrefab) == prefabPath) {
					repeatView.RegenerateChildren();
					processed.Add(go.gameObject);
					Violet.Log($"Updated RepeatView on {go.gameObject.name}");
				}
			}
		}
	}
}