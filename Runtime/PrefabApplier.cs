#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.SceneManagement;
using UnityEditor;

/// <summary>
/// PrefabApplier is responsible for applying changes to all child prefabs
/// without the designer having to remember to hit the override dropdown.
///
/// It does this by hooking into the scene save event, so when the designer saves the scene
/// all the prefab overrides will be automatically applied.
/// </summary>
namespace VioletUI {

	[ExecuteAlways]
	public class PrefabApplier : TidyBehaviour {
		void OnDestroy() {
			EditorSceneManager.sceneSaved -= EditorSceneManager_sceneSaved;
		}

		// tried this in Awake and OnDestroy but apparently these get blown away
		// when the project recompiles sooo why not here!
		void Update() {
			EditorSceneManager.sceneSaved -= EditorSceneManager_sceneSaved;
			EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;
		}

		bool isAutosaving = false;

		void EditorSceneManager_sceneSaved(Scene scene) {
			if (Application.isPlaying) { return; }
			if (isAutosaving) {
				isAutosaving = false;
				return;
			}
			UpdatePrefabs();
		}

		[Button, GUIColor(0.898f, 0.745f, 0.935f)]
		public void UpdatePrefabs() {
			bool hasChanges = false;
			foreach (Transform child in transform) {
				if (PrefabUtility.HasPrefabInstanceAnyOverrides(child.gameObject, false)) {
					PrefabUtility.ApplyPrefabInstance(child.gameObject, InteractionMode.AutomatedAction);
					hasChanges = true;
				}
			}
			if (!hasChanges) { return; }


			var scene = SceneManager.GetActiveScene();
			EditorSceneManager.MarkSceneDirty(scene);
			isAutosaving = true;
			EditorSceneManager.sceneSaved -= EditorSceneManager_sceneSaved;
			EditorSceneManager.SaveScene(scene);
			EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;
			isAutosaving = false;
		}
	}
	#endif
}