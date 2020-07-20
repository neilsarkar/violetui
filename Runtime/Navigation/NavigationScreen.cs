using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VioletUI {

	public class NavigationScreen : TidyBehaviour {
	#if UNITY_EDITOR
		public static bool IsSavingPrefab;

		public void StartEditing() {
			IsSavingPrefab = false;
			try {
				PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
			} catch (ArgumentException e) {
				UnityEngine.Debug.LogWarning($"VioletUI: Couldn't unpack prefab for {name}. Make sure the NavigationScreen is a prefab. {e}");
			}
			gameObject.SetActive(true);
		}

		public void StopEditing() {
			IsSavingPrefab = true;
			PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, $"Assets/Implementation/Menus/{name}.prefab", InteractionMode.AutomatedAction);
			gameObject.SetActive(false);
		}
	#endif
	}
}