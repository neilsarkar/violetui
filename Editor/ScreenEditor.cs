using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VioletUI {
	//run this class when unity opens
	[InitializeOnLoad]
	public static class ScreenEditor {

		//where to show the buttons
		private static GUIStyle singleStyle, leftStyle, rightStyle;

		static Navigator navigator;

		static ScreenEditor() {
			EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
			EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
		}

		static void DrawHierarchyItem(int instanceID, Rect rect) {
			var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (gameObject == null) { return; }

			navigator = gameObject.GetComponent<Navigator>();
			if (navigator != null) {
				DrawNavigator(navigator, rect);
				return;
			}

			var screen = gameObject.GetComponent<VioletScreen>();
			if (screen != null) {
				DrawScreen(screen, rect);
				return;
			}
		}

		static void DrawNavigator(Navigator navigator, Rect rect) {
			if (navigator.EditingScreen == null) {
				if (Button(rect, "Add")) {
					navigator.AddScreen();
				}
			} else if (navigator.transform.childCount > 1) {
				if (Button(rect, "Save", true)) {
					navigator.FinishEditing();
				}
			}
		}

		static void DrawScreen(VioletScreen screen, Rect rect) {
			// prefab mode
			if (screen.transform.parent == null) { return; }
			var navigator = screen.transform.parent.GetComponent<Navigator>();
			if (navigator == null) {
				Violet.LogWarning($"Unable to find Navigator for {screen.name} - make sure {screen.transform.parent.name} has a Navigator component");
				return;
			}

			if (navigator.EditingScreen != null && screen != navigator.EditingScreen) { return; }

			if (Button(rect, screen.isActiveAndEnabled ? "Save" : "Edit", screen.isActiveAndEnabled)) {
				if (navigator == null) {
					throw new VioletException($"Tried to edit {screen.name} without a Navigator. Try adding a Navigator component to {screen.transform.parent.name}");
				}
				if (screen.isActiveAndEnabled) {
					navigator.FinishEditing(screen);
				} else {
					navigator.Edit(screen);
				}
			}
		}

		static bool Button(Rect rect, string label, bool isActive = false) {
			// by default the button is 100% width
			// we move the left edge to make button fixed width, right aligned
			var buttonWidth = 36;
			rect.xMin = rect.xMax - buttonWidth;

			// extend unity mini button style with small tweaks
			var style = new GUIStyle(EditorStyles.miniButton);
			style.padding = new RectOffset(0, 2, 1, 1);
			style.fixedHeight -= 2;
			style.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
			style.alignment = TextAnchor.MiddleCenter;

			// set color to violet if active
			var originalColor = GUI.color;
			GUI.color = isActive ? Violet.Hue : originalColor;
			var response = GUI.Button(rect, new GUIContent(label), style);
			GUI.color = originalColor;

			return response;
		}
	}
}