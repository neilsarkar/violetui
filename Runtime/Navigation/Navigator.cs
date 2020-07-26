using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx.Async;
#if UNITY_EDITOR
using UnityEngine.UI;
using UnityEditor;
#endif

namespace VioletUI {
	/// <summary>
	/// Navigator maintains a map of <see cref="ScreenId"/> enum to menu screens.
	///
	/// It is used to navigate between screens and exposes <see cref="OnWillLeave"/>, <see cref="OnDidLeave" />, <see cref="OnWillVisit"/>,  and <see cref="OnDidVisit"/>.
	/// </summary>
	[ExecuteAlways]
	public class Navigator : TidyBehaviour {
		#region inspector values
		public ScreenId homeScreen = ScreenId.None;
		public bool hasCamera;
		[ShowIf("hasCamera")]
		public Camera worldCamera;
		#endregion

		#region actions
		public Action OnReady;

		public Action<ScreenId> OnWillVisit;
		public Action<ScreenId> OnWillLeave;
		public Action<ScreenId> OnDidVisit;
		public Action<ScreenId> OnDidLeave;

		public Action<ScreenId> OnModalShow;
		public Action<ScreenId> OnModalHide;
		#endregion

		#region local
		[NonSerialized] Dictionary<ScreenId, Screen> screens = new Dictionary<ScreenId, Screen>();
		[NonSerialized] ScreenId lastScreen = ScreenId.None;
		[NonSerialized] ScreenId currentScreen = ScreenId.None;
		[NonSerialized] ScreenId currentModal = ScreenId.None;
		[NonSerialized] CancellationTokenSource canceler = null;
		#endregion

		void Start() {
			if (!Application.isPlaying) { return; }
			LoadScreens();
			VisitFirstScreen();
		}

		/// <summary>
		/// Visit takes a <see cref="ScreenId"/> and transitions the menu to that scene.
		/// </summary>
		/// <param name="screenId"></param>
		public async void Visit(ScreenId screenId) {
			if (!screens.ContainsKey(screenId)) {
				throw new VioletException($"Tried to visit {screenId} but it doesn't exist in the current scene. You'll want to add the {screenId} prefab to this scene or to the MenuBuilder prefab. Or change the Home Screen to the screen you want.");
			}

			// if we're currently in a transition, cancel the transition and run OnHide/OnShow immediately
			if (canceler != null) {
				canceler.Cancel();
				canceler.Dispose();
			}
			canceler = new CancellationTokenSource();

			// current screen exits
			bool ok = true;
			var hidePromise = UniTask.CompletedTask;
			if (currentScreen != ScreenId.None) {
				lastScreen = currentScreen;
				OnWillLeave?.Invoke(lastScreen);
				hidePromise = screens[lastScreen].Hide(canceler.Token).ContinueWith((x) => {
					ok = x;
					OnDidLeave?.Invoke(lastScreen);
				});
			}
			await hidePromise;

			// new screen enters
			OnWillVisit?.Invoke(screenId);
			ok &= await screens[screenId].Show(canceler.Token);
			OnDidVisit?.Invoke(screenId);

			currentScreen = screenId;
			if (ok) {
				canceler.Dispose();
				canceler = null;
			}
		}

		public void ShowModal(ScreenId screenId) {
			// we have to call this before setting things to active because
			// it causes all input listeners to unsubscribe
			OnModalShow?.Invoke(screenId);
			screens[screenId].gameObject.SetActive(true);
			currentModal = screenId;
		}

		public void CloseModal() {
			if (currentModal == ScreenId.None) { return; }
			screens[currentModal].gameObject.SetActive(false);
			OnModalHide?.Invoke(currentModal);
			currentModal = ScreenId.None;
		}

		// Sigh.
		// https://forum.unity.com/threads/ability-to-add-enum-argument-to-button-functions.270817/
		/// <summary>
		/// Visit by string is for use in UnityEvents, since the editor doesn't show functions with enum arguments
		/// </summary>
		/// <param name="screenIdString"></param>
		[Obsolete("This is for us in UnityEvents only since they can't accept an enum. Use `Visit(ScreenId screenId)` instead")]
		public void Visit(string screenIdString) {
			ScreenId screenId;
			var isValid = Enum.TryParse<ScreenId>(screenIdString.Replace(" ", ""), out screenId);
			if (!isValid) {
				throw new VioletException($"Couldn't find a screen with the id {screenIdString.Replace(" ", "")}. Please check the spelling.");
			}
			Visit(screenId);
		}

		public void ShowModal(string screenIdString) {
			ScreenId screenId;
			var isValid = Enum.TryParse<ScreenId>(screenIdString.Replace(" ", ""), out screenId);
			if (!isValid) {
				throw new VioletException($"Couldn't find a screen with the id {screenIdString.Replace(" ", "")}. Please check the spelling.");
			}

			ShowModal(screenId);
		}

		protected virtual void VisitFirstScreen() {
			Visit(homeScreen);
		}

		ScreenId ScreenToScreenId(Screen screen) {
			ScreenId ret;
			var slug = ScreenIdGenerator.Sanitize(screen.name);
			var ok = Enum.TryParse<ScreenId>(slug, out ret);

			if (!ok) {
				throw new VioletEnumException($"{slug} does not exist in ScreenId. This should be set up automatically by navigator but you can add manually as a workaround.");
			}
			return ret;
		}

		void LoadScreens() {
			if (transform.childCount == 0) { return; }

			ScreenId screenId = ScreenId.None;
			foreach (Screen screen in GetComponentsInChildren<Screen>(true)) {
				var isValid = Enum.TryParse(ScreenIdGenerator.Sanitize(screen.name), out screenId);
				if (!isValid) {
#if UNITY_EDITOR
					Violet.LogWarning($"Couldn't find {screen.name}, regenerating. Try pressing play again. ScreenId contains {string.Join(", ", Enum.GetNames(typeof(ScreenId)))}");
					RegenerateEnums();
					EditorApplication.ExitPlaymode();
#else
					throw new VioletException($"{screen.name} does not have a valid ScreenId. ScreenId contains {string.Join(", ", Enum.GetNames(typeof(ScreenId)))}"");
#endif
				}

				if (hasCamera) {
					if (worldCamera == null) {
						throw new VioletException($"{name} does not have a camera attached. Either set hasCamera to false or attach a camera.");
					}
					var canvas = screen.gameObject.GetComponent<Canvas>();
					if (canvas == null) {
						throw new VioletException($"{screen.name} does not have a Canvas component to attach {worldCamera.name} to. Either make {screen.name} a canvas or remove the camera from {name}");
					}
					canvas.renderMode = RenderMode.ScreenSpaceCamera;
					canvas.worldCamera = worldCamera;
				}

				screens[screenId] = screen;
				screen.gameObject.SetActive(false);
			}

			OnReady?.Invoke();
		}


#if UNITY_EDITOR
		[HideInInspector] public Screen EditingScreen;
		[SerializeField, HideInInspector] ScreenId originalHomeScreen;

		public void Edit(Screen screen) {
			originalHomeScreen = homeScreen;
			try {
				homeScreen = ScreenToScreenId(screen);
			} catch (VioletEnumException) {
				Violet.LogWarning($"Couldn't find {screen.name} in the ScreenId enum. This should be fixed if you hit edit again. If not, please report a bug.");
				RegenerateEnums();
				return;
			}

			screen.UnpackPrefab();
			screen.gameObject.SetActive(true);
			EditingScreen = screen;
			currentScreen = ScreenToScreenId(screen);
		}

		public void FinishEditing(Screen screen = null) {
			if (EditingScreen == null) { EditingScreen = gameObject.GetComponentInChildren<Screen>(); }
			if (screen == null) { screen = EditingScreen; }
			screen.PackPrefab();
			screen.gameObject.SetActive(false);
			if (screen == EditingScreen) { EditingScreen = null; };
			homeScreen = originalHomeScreen;
		}

		public void AddScreen() {
			var gameObject = new GameObject("Rename Me");
			var screen = gameObject.AddComponent<Screen>();
			var canvas = gameObject.AddComponent<Canvas>();
			gameObject.AddComponent<CanvasRenderer>();
			gameObject.AddComponent<GraphicRaycaster>();

			gameObject.transform.SetParent(transform, false);
			gameObject.transform.position = new Vector3(0, 0, 0);
			canvas.renderMode = worldCamera == null ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
			EditingScreen = screen;
		}

		float lastUpdate;
		void Update() {
			if (Application.isPlaying) { return; }
			if (transform.childCount == 0) { return; }
			if (transform.childCount == screens.Count) { return; }
			if (Time.time - lastUpdate <= .5f) { return; }
			lastUpdate = Time.time;
			Violet.LogVerbose($"{name} reloading screens and regenerating enums");
			LoadScreens();
			RegenerateEnums();
		}

		[NonSerialized, ShowInInspector] public bool Advanced;
		[Title("Advanced")]
		[ShowIf("Advanced"), Button, GUIColor(Violet.r, Violet.g, Violet.b)]
		void Regenerate() {
			Violet.Log($"Regenerating enums...");
			RegenerateEnums();
			Violet.Log($"Reloading screens...");
			LoadScreens();
			Violet.Log($"Done.");
		}

		void RegenerateEnums() {
			var screens = GetComponentsInChildren<Screen>(true);
			ScreenIdGenerator.Generate(screens);
			Violet.Log($"Reloading screens...");
			Violet.Log($"Done.");
		}
#endif
	}
}
