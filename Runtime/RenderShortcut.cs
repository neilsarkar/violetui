#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using System;
using System.Reflection;

namespace VioletUI {
	public static class RenderShortcut {
		public static Action OnPress;

		[Shortcut("VioletRenderShortcut", KeyCode.Semicolon, ShortcutModifiers.Action)]
		public static void Press() {
			OnPress?.Invoke();
		}
	}
}
#endif