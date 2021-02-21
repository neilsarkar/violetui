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

		public static Color RedHue = new Color(1f, 0.2f, 0.2f);
		public static Color GreenHue = new Color(0.2f, 1f, 0.2f);

		//where to show the buttons
		private static GUIStyle singleStyle, leftStyle, rightStyle;

		static Navigator navigator;

		static ScreenEditor() {
			EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
			EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
		}

		static void DrawHierarchyItem(int instanceID, Rect rect) {
			if (Application.isPlaying) { return; }
			var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (gameObject == null) { return; }
			if (gameObject.scene == null) { return; }

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
				if (Button(rect, 36, "New")) {
					navigator.AddScreen();
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

			if (Button(rect, 36, screen.isActiveAndEnabled ? "Save" : "Edit", screen.isActiveAndEnabled)) {
				if (navigator == null) {
					throw new VioletException($"Tried to edit {screen.name} without a Navigator. Try adding a Navigator component to {screen.transform.parent.name}");
				}
				if (screen.isActiveAndEnabled) {
					navigator.FinishEditing(screen);
				} else {
					navigator.Edit(screen);
				}
			}
			if (screen.isActiveAndEnabled) {
				if (Button(rect, 18, "X", screen.isActiveAndEnabled, RedHue, -50)) {
					if (navigator == null) {
						throw new VioletException($"Can't find navigator in scene");
					}
					navigator.DiscardEdits();
				}
			}

		}

		static bool Button(Rect rect, int buttonWidth, string label, bool isActive = false, Color highlightColor = default, int xOffset = 0) {
			// by default the button is 100% width
			// we move the left edge to make button fixed width, right aligned
			rect.xMax = rect.xMax + xOffset;
			rect.xMin = rect.xMax - buttonWidth;

			// extend unity mini button style with small tweaks
			var style = new GUIStyle(EditorStyles.miniButton);
			style.padding = new RectOffset(0, 2, 1, 1);
			style.fixedHeight -= 2;
			style.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
			style.alignment = TextAnchor.MiddleCenter;

			// set color to violet if active
			var originalColor = GUI.color;
			highlightColor = highlightColor == default ? Violet.Hue : highlightColor;
			GUI.color = isActive ? highlightColor : originalColor;
			var response = GUI.Button(rect, new GUIContent(label), style);
			GUI.color = originalColor;

			return response;
		}

	}
}