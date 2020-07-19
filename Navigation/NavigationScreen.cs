#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavigationScreen : TidyBehaviour {

#if UNITY_EDITOR
	public void StartEditing() {
		PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
		gameObject.SetActive(true);
	}

	public void StopEditing() {
		gameObject.SetActive(false);
	}
#endif
}