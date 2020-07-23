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
		static Color violet = new Color(0.898f, 0.745f, 0.935f);
		static Color saturatedViolet;

		static ScreenEditor() {
			EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
			EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
			float h,s,v;
			Color.RGBToHSV(violet, out h, out s, out v);
			saturatedViolet = Color.HSVToRGB(h, 1f, 1f);
		}

		[MenuItem("GameObject/Create Screen", false, 0)]
		static void CreateScreen() {
			var gameObject = UnityEditor.Selection.activeGameObject;
			if (gameObject.GetComponent<Navigator>() == null)  {
				throw new VioletException($"Unable to create screen child of {gameObject.name} - try adding a Navigator component to {gameObject.name} first.");
			}
			Debug.Log("nice");
		}

		static void DrawHierarchyItem(int instanceID, Rect rect) {
			var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (gameObject == null) { return; }

			navigator = gameObject.GetComponent<Navigator>();
			if (navigator != null) {
				DrawNavigator(navigator, rect);
				return;
			}

			var screen = gameObject.GetComponent<Screen>();
			if (screen != null) {
				DrawScreen(screen, rect);
				return;
			}
		}

		static void DrawNavigator(Navigator navigator, Rect rect) {
			if (navigator.EditingScreen == null) {return;}

			if (Button(rect, "Save", true)) {
				navigator.FinishEditing();
			}
		}

		static void DrawScreen(Screen screen, Rect rect) {
			var navigator = screen.transform.parent.GetComponent<Navigator>();
			if (navigator.EditingScreen != null && screen != navigator.EditingScreen) { return; }

			if (Button(rect, screen.isActiveAndEnabled ? "Save" : "Edit", screen.isActiveAndEnabled)) {
				if (navigator == null) {
					throw new VioletException($"Tried to edit {screen.name} without a Navigator. Try adding a Navigator component to {screen.transform.parent.name}");
				}
				if (screen.isActiveAndEnabled) {
					navigator.FinishEditing();
				} else {
					navigator.Edit(screen);
				}
			}
		}

		static bool Button(Rect rect, string label, bool isActive = false) {
			// by default the button is 100% width
			// we move the left edge to make button 60 pixels wide, right aligned
			var buttonWidth = 60;
			rect.xMin = rect.xMax - buttonWidth;

			// extend unity mini button style with small tweaks
			var style = new GUIStyle(EditorStyles.miniButton);
			style.padding = new RectOffset(3, 0, 1, 1);
			style.fixedHeight -= 2;
			style.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
			style.alignment = TextAnchor.MiddleLeft;

			// set color to violet if active
			var originalColor = GUI.color;
			GUI.color = isActive ? violet : originalColor;
			var response = GUI.Button(rect, new GUIContent(label), style);
			GUI.color = originalColor;

			return response;
		}
	}
}