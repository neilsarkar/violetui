﻿using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VioletUI {
	//run this class when unity opens
	[InitializeOnLoad]
	public class ScreenEditor {

		//width of inspector buttons
		private const float BUTTON_WIDTH = 60f;

		//where to show the buttons
		private static GUIStyle singleStyle, leftStyle, rightStyle;

		//hash set of all menus in scene
		private static HashSet<Screen> menus = new HashSet<Screen>();

		//menu screens to remove
		private static List<Screen> garbage = new List<Screen>();

		//constructor
		static ScreenEditor() {

			//only show while !playing
			if (!EditorApplication.isPlaying) {

				//actual editor event functions
				EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
				EditorApplication.update += Update;

			}

		}

		//remove all menu screens that are no longer used to avoid memory leak
		private static void Clean(HashSet<Screen> menus) {

			//clear the unused menus
			garbage.Clear();


			//add all menus that no longer exist to the garbage list
			foreach (var menu in menus) {
				if (menu == null) {
					garbage.Add(menu);
				}
			}

			//remove the garbage from the menus list
			foreach (var menu in garbage) {
				menus.Remove(menu);
			}
		}


		//update visuals
		private static void Update() {
			//remove the garbage from the menu list
			Clean(menus);

			foreach (var menu in Object.FindObjectsOfType<Screen>()) {
				menus.Add(menu);
			}
		}

		private static GUIStyle InitStyle(ref GUIStyle style, GUIStyle original, TextAnchor alignment) {
			if (style == null) {
				style = new GUIStyle(original);
				style.alignment = alignment;
				style.richText = true;
			}

			return style;
		}
		private static bool Button(Rect rect, string label, string tooltip, Color color, FontStyle fontStyle = FontStyle.Normal, int dot = 0, int i = 0, int n = 1) {
			rect = rect.VioletBackfillRight(BUTTON_WIDTH);

			GUIStyle style = null;

			if (n == 1) {
				style = InitStyle(ref singleStyle, EditorStyles.miniButton, TextAnchor.MiddleLeft);
			} else if (n == 2) {
				switch (i) {
					case 0:
						style = InitStyle(ref leftStyle, EditorStyles.miniButtonLeft, TextAnchor.MiddleLeft);
						break;
					case 1:
						style = InitStyle(ref rightStyle, EditorStyles.miniButtonRight, TextAnchor.MiddleRight);
						break;
				}
			}

			var content = new GUIContent(" " + label, null, tooltip);

			style.fontStyle = fontStyle;

			var response = GUI.Button(rect, content, singleStyle);

			style.fontStyle = FontStyle.Normal;

			var shadowColor = Color.Lerp(color, Color.black, 0.5f);
			shadowColor.a = 0.8f;

			if (dot > 0) {
				var dotRect = rect.VioletBackfillRight(rect.height).VioletDisplaceX(1f);
				var dotColor = dot == 2 ? color : Color.grey;

				dotColor.a = 0.5f;

				GUI.Box(dotRect, GUIContent.none);
				EditorGUI.DrawRect(dotRect.VioletPad(3f).VioletDisplaceX(-1f), dotColor);

			}

			rect = rect.VioletSetWidth(3f).VioletDisplaceX(4f).VioletPadVertical(1f);

			EditorGUI.DrawRect(rect.VioletExtrudeLeft(1f), shadowColor);
			EditorGUI.DrawRect(rect.VioletExtrudeRight(1f), shadowColor);

			color.a = 0.5f;

			EditorGUI.DrawRect(rect, color);

			return response;
		}

		private static bool TryGetManagerColor(int instanceID, out Color color) {
			color = new Color(0.898f, 0.745f, 0.935f);
			return true;
		}

		private static void DrawHierarchyItem(int instanceID, Rect rect) {
			var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

			Color violet = new Color(0.898f, 0.745f, 0.935f);
			float h = 0f, s = 0f, v = 0f;
			Color.RGBToHSV(violet, out h, out s, out v);
			Color saturatedViolet = Color.HSVToRGB(h, 1f, 1f);
			if (gameObject == null) { return; }

			// TODO: just find the parent of any number of Navigation Screens
			if (gameObject.GetComponent<NavigationController>() != null) {
				if (Button(rect, "Save", "", saturatedViolet, FontStyle.Normal)) {
					foreach (var menu in menus) {
						menu.gameObject.GetComponent<Screen>()?.StopEditing();
					}
				}
				return;
			}

			if (gameObject.GetComponent<Screen>() != null) {
				if (Button(rect, "Edit", "", saturatedViolet, FontStyle.Normal, gameObject.activeSelf ? 2 : 1)) {
					foreach (var menu in menus) {
						menu.gameObject.GetComponent<Screen>()?.StopEditing();
					}
					gameObject.GetComponent<Screen>().StartEditing();
				}
				return;
			}
		}
	}
}