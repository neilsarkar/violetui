using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UniRx.Async;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace VioletUI {
	[RequireComponent(typeof(Animator))]
	[ExecuteAlways]
	public class VioletScreen : TidyBehaviour {

		[Title("Show/Hide")]

		[ValueDropdown("TriggerNames")] public string ShowAnimation;
		[ValueDropdown("TriggerNames")] public string HideAnimation;
		public UnityEvent OnShow;
		public UnityEvent OnHide;

		Animator animator;

		protected virtual void OnEnable() {
			animator = gameObject.GetComponent<Animator>();
		}

		protected virtual void DidShow() { OnShow?.Invoke(); }
		protected virtual void DidHide() { OnHide?.Invoke(); }
		protected virtual void WillShow() {}
		protected virtual void WillHide() {}

		internal async UniTask<bool> Show(CancellationToken token = default, string triggerOverride = null) {
			WillShow();

			gameObject.SetActive(true);
			DidShow();

			var triggerName = triggerOverride == null ? ShowAnimation : triggerOverride;

			try {
				await Animate(triggerName, token);
			} catch (OperationCanceledException) {
				return false;
			}

			return true;
		}

		internal async UniTask<bool> Hide(CancellationToken token = default, string triggerOverride = null) {
			WillHide();
			var triggerName = triggerOverride == null ? HideAnimation : triggerOverride;

			bool ok = true;
			try {
				await Animate(triggerName, token);
			} catch (OperationCanceledException) {
				ok = false;
			}

			gameObject.SetActive(false);
			DidHide();

			return ok;
		}

		async UniTask<bool> Animate(string trigger, CancellationToken token = default) {
			if (animator == null || trigger == null || trigger == string.Empty || trigger == "None" || trigger == "Null") { return true; }
			if (!Application.isPlaying) { return true; }

			animator.SetTrigger(trigger);
			var state = animator.GetCurrentAnimatorStateInfo(0);
			await UniTask.Delay((int)(state.length * 1000), false, PlayerLoopTiming.Update, token);
			return true;
		}

		public static string Sanitize(string s) {
			return s.Replace(" ", "");
		}

#if UNITY_EDITOR
		List<string> TriggerNames() {
			var animator = GetComponent<Animator>();
			if (animator == null) { return new List<string>() { "(add animator component)" }; }

			var animatorController = animator.runtimeAnimatorController as AnimatorController;
			if (animatorController == null) { return new List<string>() {"(add animator controller)" }; }

			if (animatorController.parameters.Length == 0) { return new List<string>() { "(add triggers to animator controller)" }; }

			var names = new List<string>() { "None" };
			foreach (var trigger in animatorController.parameters) {
				if (trigger.type != AnimatorControllerParameterType.Trigger) { continue; }
				names.Add(trigger.name);
			}

			return names;
		}

		public void Update() {
			EditorSceneManager.sceneSaved -= EditorSceneManager_sceneSaved;
			EditorSceneManager.sceneSaved += EditorSceneManager_sceneSaved;
		}

		string prefabPath = "";
		GameObject prefab;

		public void PackPrefab() {
			var path = string.IsNullOrEmpty(prefabPath) ? $"Assets/Menus/{name}.prefab" : prefabPath;
			PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction);
		}

		public void RevertPrefab() {
			Violet.Log($"Reverting. You will lose work!");
			var revertedScreen = PrefabUtility.InstantiatePrefab(prefab, transform.parent) as GameObject;
			revertedScreen.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex());
			revertedScreen.SetActive(false);
			DestroyImmediate(gameObject);
		}

		public void UnpackPrefab() {
			if (!PrefabUtility.IsAnyPrefabInstanceRoot(gameObject)) {
				Violet.LogVerbose($"Not unpacking {name} bc it's not a prefab yet.");
				return;
			}

			prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource<GameObject>(gameObject);
			prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
			Violet.LogVerbose($"prefabPath is {prefabPath} for {name}");
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
		}

		void EditorSceneManager_sceneSaved(Scene scene) {
			try {
				if (!gameObject.activeSelf) {return;}
				PackPrefab();
				UnpackPrefab();
				Violet.Log($"Saved {name} prefab");
			} catch(MissingReferenceException) {}
		}
#else
		List<string> TriggerNames() { return null; }
#endif

	}
}