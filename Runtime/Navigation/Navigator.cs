using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading;
using Cysharp.Threading.Tasks;
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

		public Action<ScreenId> OnEditStart;
		public Action<ScreenId> OnEditFinish;
		#endregion

		#region local
		[NonSerialized] protected Stack<ScreenId> lastScreens = new Stack<ScreenId>();
		[NonSerialized] protected Stack<ScreenId> modals = new Stack<ScreenId>();
		[NonSerialized] protected Dictionary<ScreenId, VioletScreen> screens = new Dictionary<ScreenId, VioletScreen>();
		[NonSerialized] protected ScreenId lastScreen = ScreenId.None;
		[NonSerialized] protected ScreenId currentScreen = ScreenId.None;
		[NonSerialized] protected ScreenId currentModal = ScreenId.None;
		[NonSerialized] CancellationTokenSource canceler = null;
		#endregion

		protected virtual void Start() {
			if (!Application.isPlaying) { return; }
			LoadScreens();
			VisitFirstScreen();
		}

		void OnDisable() {
			if (canceler != null) {
				canceler.Cancel();
				canceler.Dispose();
			}
			canceler = null;
		}

		/// <summary>
		/// GoBack must be called after at least one Visit call and will revisit the previous screen
		/// </summary>
		/// <returns>visit success status</returns>
		public async UniTask<bool> GoBack() {
			if (lastScreens.Count == 0) {
				UnityEngine.Debug.LogError($"Tried to go back but we haven't visited a screen yet.");
				return false;
			}

			var screen = lastScreens.Pop();
			return await Visit(lastScreen, false);
		}

		public async UniTask<bool> DebugVisit(ScreenId screenId) {
			if (currentScreen != ScreenId.None) {
				screens[currentScreen].gameObject.SetActive(false);
			}
			await UniTask.DelayFrame(1);
			screens[screenId].gameObject.SetActive(true);
			return true;
		}

		/// <summary>
		/// Visit takes a <see cref="ScreenId"/> and transitions the menu to that scene.
		/// </summary>
		/// <param name="screenId"></param>
		public async UniTask<bool> Visit(ScreenId screenId, bool isForward = true) {
			Violet.LogVerbose($"Visiting {screenId}");
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
					if (isForward) {
						lastScreens.Push(lastScreen);
					}
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

			return ok;
		}

		/// <summary>
		/// Show another screen in addition to the current screen and disable input listeners on the current screen.
		///
		/// It fires <see cref="OnModalShow" /> prior to setting the screen to active.
		/// </summary>
		/// <param name="screenId">Auto-generated id of screen to show as modal</param>
		public async void ShowModal(ScreenId screenId) {
			// we have to call this before setting things to active because
			// it causes all input listeners to unsubscribe
			OnModalShow?.Invoke(screenId);
			await screens[screenId].Show(default);
			modals.Push(screenId);
			currentModal = screenId;
		}

		/// <summary>
		/// Hide the currently shown modal and re-enables input listeners on the current screen
		///
		/// It fires <see cref="OnModalHide" /> after setting the modal to inactive.
		/// </summary>
		public async virtual UniTask HideModal() {
			if (currentModal == ScreenId.None) { return; }
			var screenId = modals.Pop();
			await screens[screenId].Hide();
			currentModal = modals.Count > 0 ? modals.Peek() : ScreenId.None;
			OnModalHide?.Invoke(screenId);
		}

		public async virtual UniTask CloseAllModals() {
			int attempts = 10;
			while (currentModal != ScreenId.None) {
				if (attempts-- < 0) {
					throw new Exception($"Couldn't CloseAllModals. Stuck on {currentModal}");
				}
				await HideModal();
			}
		}

		/// <summary>
		/// Show an overlay screen in addition to the current screen. Triggers no events and allows input on main screen.
		/// </summary>
		/// <param name="screenId">id of screen to set active</param>
		public void ShowOverlay(ScreenId screenId) {
			try {
				screens[screenId].gameObject.SetActive(true);
			} catch(KeyNotFoundException) {
				// when editing screens in editor the screens dictionary may be empty
				LoadScreens();
				screens[screenId].gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Show an overlay screen in addition to the current screen. Triggers no events.
		/// </summary>
		/// <param name="screenId">id of screen to set inactive</param>
		public void HideOverlay(ScreenId screenId) {
			try {
				screens[screenId].gameObject.SetActive(false);
			} catch(KeyNotFoundException) {
				// when editing screens in editor the screens dictionary may be empty
				LoadScreens();
				screens[screenId].gameObject.SetActive(false);
			}
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
			_ = Visit(screenId);
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
#if !UNITY_EDITOR
			homeScreen = originalHomeScreen;
#endif
			_ = Visit(homeScreen);
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
					AddScreens();
					Violet.LogWarning($"Couldn't find {screen.name}, regenerating. Try pressing play again. ScreenId contains {string.Join(", ", Enum.GetNames(typeof(ScreenId)))}");
#if UNITY_EDITOR
					EditorApplication.ExitPlaymode();
#else
					throw new VioletException($"{screen.name} does not have a valid ScreenId. ScreenId contains {string.Join(", ", Enum.GetNames(typeof(ScreenId)))}");
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

#if UNITY_EDITOR
				screen.gameObject.SetActive(screen == EditingScreen);
#else
				screen.gameObject.SetActive(false);
#endif
			}

			OnReady?.Invoke();
		}

		protected virtual void OnScreenAdded(GameObject gameObject) { }

		[SerializeField, HideInInspector] ScreenId originalHomeScreen;
#if UNITY_EDITOR
		[HideInInspector] public VioletScreen EditingScreen;

		public void Edit(VioletScreen screen) {
			originalHomeScreen = homeScreen;
			try {
				homeScreen = ScreenToScreenId(screen);
			} catch (VioletEnumException) {
				Violet.LogWarning($"Couldn't find {screen.name} in the ScreenId enum. This should be fixed if you hit edit again. If not, please report a bug.");
				AddScreens();
				return;
			}

			screen.UnpackPrefab();
			screen.gameObject.SetActive(true);
			EditingScreen = screen;
			currentScreen = ScreenToScreenId(screen);
			OnEditStart?.Invoke(currentScreen);
		}

		public void FinishEditing(VioletScreen screen = null) {
			if (EditingScreen == null) { EditingScreen = gameObject.GetComponentInChildren<VioletScreen>(); }
			if (screen == null) { screen = EditingScreen; }
			screen.PackPrefab();
			screen.gameObject.SetActive(false);
			if (screen == EditingScreen) { EditingScreen = null; };
			homeScreen = originalHomeScreen;
			OnEditFinish?.Invoke(ScreenToScreenId(screen));
		}

		public void DiscardEdits() {
			if (EditingScreen == null) { EditingScreen = gameObject.GetComponentInChildren<VioletScreen>(); }
			OnEditFinish?.Invoke(ScreenToScreenId(EditingScreen));
			EditingScreen.gameObject.SetActive(false);
			EditingScreen.RevertPrefab();
			EditingScreen = null;
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
			AddScreens();
		}

		[Title("Advanced")]
		[Button, GUIColor(Violet.r * 2, 0, 0)]
		void DeleteUnusedScreens() {
			ReplaceScreens();
		}

		[Button, GUIColor(Violet.r, Violet.g, Violet.b)]
		void ReloadScreensFromJson() {
			WantsReloadScreens?.Invoke();
		}

		// TODO: move all of this to the editor assembly
		public static Action<VioletScreen[]> WantsAddScreens;
		public static Action<VioletScreen[]> WantsReplaceScreens;
		public static Action WantsReloadScreens;
		void AddScreens() {
			WantsAddScreens?.Invoke(GetComponentsInChildren<VioletScreen>(true));
		}
		void ReplaceScreens() {
			WantsReplaceScreens?.Invoke(GetComponentsInChildren<VioletScreen>(true));
		}
#else
		void AddScreens() {}
		void ReplaceScreens() {}
#endif
	}
}
