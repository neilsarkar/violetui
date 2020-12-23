using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace VioletUI {
	// https://answers.unity.com/questions/1304097/subclassing-button-public-variable-wont-show-up-in.html
	[CustomEditor(typeof(VioletButton))]
	public class VioletButtonEditor : ButtonEditor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			VioletButton button = (VioletButton)target;
			button.visitScreen = (ScreenId)EditorGUILayout.EnumPopup("Visit Screen", button.visitScreen);
		}
	}
}
