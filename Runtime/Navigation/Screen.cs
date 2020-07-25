using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UniRx.Async;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace VioletUI {
	[ExecuteAlways]
	public class Screen : TidyBehaviour {

		internal async UniTask<bool> Show(CancellationToken token) {
			gameObject.SetActive(true);
			await UniTask.DelayFrame(1);
			return true;
		}

		internal async UniTask<bool> Hide(CancellationToken token) {
			gameObject.SetActive(false);
			await UniTask.DelayFrame(1);
			return true;
		}

#if UNITY_EDITOR
		public void Update() {
			EditorSceneManager.sceneSaved -= EditorSceneManager_sceneSaved;
			EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;
		}

		void EditorSceneManager_sceneSaved(Scene scene) {
			Violet.Log($"Scene saved, will update the {name} fabio");
		}
#endif

	}
}