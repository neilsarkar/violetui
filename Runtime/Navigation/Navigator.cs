using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx.Async;
using UnityEngine.UI;
#if UNITY_EDITOR
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
		[NonSerialized] Dictionary<ScreenId, VioletScreen> screens = new Dictionary<ScreenId, VioletScreen>();
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

		ScreenId ScreenToScreenId(VioletScreen screen) {
			ScreenId ret;
			var slug = VioletScreen.Sanitize(screen.name);
			var ok = Enum.TryParse<ScreenId>(slug, out ret);

			if (!ok) {
				throw new VioletEnumException($"{slug} does not exist in ScreenId. This should be set up automatically by navigator but you can add manually as a workaround.");
			}
			return ret;
		}


		void LoadScreens() {
			if (transform.childCount == 0) { return; }

			ScreenId screenId = ScreenId.None;
			screens.Clear();
			foreach (VioletScreen screen in GetComponentsInChildren<VioletScreen>(true)) {
				var isValid = Enum.TryParse(VioletScreen.Sanitize(screen.name), out screenId);
				if (!isValid) {
					RegenerateEnums();
					Violet.LogWarning($"Couldn't find {screen.name}, regenerating. Try pressing play again. ScreenId contains {string.Join(", ", Enum.GetNames(typeof(ScreenId)))}");
#if UNITY_EDITOR
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
		[HideInInspector] public VioletScreen EditingScreen;
		[SerializeField, HideInInspector] ScreenId originalHomeScreen;

		public void Edit(VioletScreen screen) {
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

		public void FinishEditing(VioletScreen screen = null) {
			if (EditingScreen == null) { EditingScreen = gameObject.GetComponentInChildren<VioletScreen>(); }
			if (screen == null) { screen = EditingScreen; }
			screen.PackPrefab();
			screen.gameObject.SetActive(false);
			if (screen == EditingScreen) { EditingScreen = null; };
			homeScreen = originalHomeScreen;
		}

		public void AddScreen() {
			var gameObject = new GameObject("Rename Me");
			gameObject.layer = 5;
			var screen = gameObject.AddComponent<VioletScreen>();
			var canvas = gameObject.AddComponent<Canvas>();
			gameObject.AddComponent<CanvasRenderer>();
			gameObject.AddComponent<GraphicRaycaster>();
			gameObject.transform.SetParent(transform, false);
			gameObject.transform.position = new Vector3(0, 0, 0);
			gameObject.transform.localScale = new Vector3(1, 1, 1);

			if (hasCamera) {
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = worldCamera;
			} else {
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			}
			OnScreenAdded(gameObject);
			EditingScreen = screen;
		}

		protected virtual void OnScreenAdded(GameObject gameObject) { }

		float lastUpdate;
		int childCount;
		void Update() {
			if (Application.isPlaying) { return; }
			if (transform.childCount == 0 || transform.childCount != childCount) { return; }
			if (Time.time - lastUpdate <= .5f) { return; }
			lastUpdate = Time.time;
			childCount = transform.childCount;
			Violet.LogVerbose($"{name} reloading screens and regenerating enums. childCount={transform.childCount} screensCount={screens.Count}");
			LoadScreens();
			RegenerateEnums();
		}

		[NonSerialized, ShowInInspector] public bool Advanced;
		[Title("Advanced")]
		[ShowIf("Advanced"), Button, GUIColor(Violet.r, Violet.g, Violet.b)]
		void Regenerate() {
			Violet.LogVerbose($"Regenerating enums...");
			RegenerateEnums();
			Violet.LogVerbose($"Reloading screens...");
			LoadScreens();
			Violet.LogVerbose($"Done reloading screens.");
		}

		// TODO: move all of this to the editor assembly
		public static Action<VioletScreen[]> WantsRegenerate;
		void RegenerateEnums() {
			WantsRegenerate?.Invoke(GetComponentsInChildren<VioletScreen>(true));
		}
#endif
	}
}
