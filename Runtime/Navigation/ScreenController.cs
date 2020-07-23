using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx.Async;

namespace VioletUI {

	/// <summary>
	/// ScreenController maintains a map of <see cref="ScreenId"/> enum to menu screens.
	///
	/// It is primarily used to navigate between screens, and also exposes the <see cref="OnWillVisit"/> and <seealso cref="OnDidVisit"/> lifecycle events.
	/// </summary>
	public class ScreenController : TidyBehaviour {
		Dictionary<ScreenId, Screen> screens = new Dictionary<ScreenId, Screen>();

		public ScreenId homeScreen = ScreenId.None;

		public Action OnReady;

		public Action<ScreenId> OnWillVisit;
		public Action<ScreenId> OnWillLeave;
		public Action<ScreenId> OnDidVisit;
		public Action<ScreenId> OnDidLeave;

		public Action<ScreenId> OnModalShow;
		public Action<ScreenId> OnModalHide;

		public Camera worldCamera;

		ScreenId lastScreen = ScreenId.None;
		ScreenId currentScreen = ScreenId.None;
		ScreenId currentModal = ScreenId.None;

		[NonSerialized]
		CancellationTokenSource canceler = null;

		void Awake() {
			LoadScreens();
		}
		void LoadScreens() {
			if (transform.childCount == 0) {
				throw new Exception($"Tried to create a NavigationController with no children - try adding some NavigationScreens to {gameObject.name}");
			}

			ScreenId screenId = ScreenId.None;
			foreach (Screen screen in GetComponentsInChildren<Screen>()) {
				var isValid = Enum.TryParse(name, out screenId);
				if (!isValid) {
					throw new VioletException($"{name} does not have a valid ScreenId - make sure this screen is added to MenuBuilder.");
				}

				var canvas = screen.gameObject.GetComponent<Canvas>();
				if (canvas == null) {
					throw new VioletException($"{name} does not have a Canvas component - you need to add a canvas component or set a manual exception in NavigationController.cs");
				}
				canvas.worldCamera = worldCamera;

				screens[screenId] = screen;
				screen.gameObject.SetActive(false);
			}

			OnReady?.Invoke();

			if (Application.isPlaying) {
				VisitFirstScreen();
			}
		}

		/// <summary>
		/// Visit takes a <see cref="ScreenId"/> and transitions the menu to that scene.
		/// </summary>
		/// <param name="screenId"></param>
		public async void Visit(ScreenId screenId, int delayMs = 0) {
			// TODO: @null allow double Visit without this bool
			// the issue here is that when the first visit successfully completes it
			// is clearing the canceler for the next one.
			bool ok = true;
			if (!screens.ContainsKey(screenId)) {
				throw new Exception($"Tried to visit {screenId} but it doesn't exist in the current scene. You'll want to add the {screenId} prefab to this scene or to the MenuBuilder prefab. Or change the Home Screen to the screen you want.");
			}

			// if we're currently in a transition, cancel the transition and run OnHide/OnShow immediately
			if (canceler != null) {
				canceler.Cancel();
				canceler.Dispose();
			}
			canceler = new CancellationTokenSource();

			// current screen exits
			var hidePromise = UniTask.CompletedTask;
			if (currentScreen != ScreenId.None) {
				lastScreen = currentScreen;
				OnWillLeave?.Invoke(lastScreen);
				hidePromise = screens[lastScreen].Hide(canceler.Token).ContinueWith((x) => {
					ok = x;
					OnDidLeave?.Invoke(lastScreen);
				});
			}

			UIDispatcher.Run(new LockInputAction(true));

			int frame = Contexts.sharedInstance.time.tick.value;
			// if running in series, wait for hide animation to complete
			if (chainOrder == ChainOrder.Series) {
				await hidePromise;
			}
			UIDispatcher.Run(new SelectButtonAction(null));

			// if there's a delay, wait for delay before starting show animation
			if (delayMs > 0) {
				await UniTask.Delay(delayMs);
			}

			// new screen enters
			OnWillVisit?.Invoke(screenId);
			var success = await screens[screenId].Show(canceler.Token);
			if (!success) { ok = false;}
			OnDidVisit?.Invoke(screenId);

			UIDispatcher.Run(new SetScreen(screenId));
			UIDispatcher.Run(new LockInputAction(false));

			currentScreen = screenId;
			if (ok) {
				canceler.Dispose();
				canceler = null;
			}
		}

		public void ShowModal (string screenNameString) {
			ScreenId screenName;
			var isValid = Enum.TryParse<ScreenId>(screenNameString.Replace(" ", ""), out screenName);
			if (!isValid) {
				throw new Exception($"Couldn't find a screen with the id {screenNameString.Replace(" ", "")}. Please check the spelling.");
			}

			ShowModal(screenName);
		}

		public void ShowModal(ScreenId screenName) {
			UIDispatcher.Run(new SelectButtonAction(null));
			// we have to call this before setting things to active because
			// it causes all input listeners to unsubscribe
			OnModalShow?.Invoke(screenName);
			screens[screenName].gameObject.SetActive(true);
			UIDispatcher.Send(new LockCursorsAction(!screens[screenName].useCursors));
			currentModal = screenName;
		}

		public void CloseModal() {
			if (currentModal == ScreenId.None) { return; }
			screens[currentModal].gameObject.SetActive(false);
			OnModalHide?.Invoke(currentModal);
			currentModal = ScreenId.None;

			UIDispatcher.Send(new LockCursorsAction(!screens[currentScreen].useCursors));
		}

		// Sigh.
		// https://forum.unity.com/threads/ability-to-add-enum-argument-to-button-functions.270817/
		/// <summary>
		/// Visit by string is for use in UnityEvents only. In code, please use Visit by enum
		/// </summary>
		/// <param name="screenIdString"></param>
		public void Visit(string screenIdString) {
			ScreenId screenId;
			var isValid = Enum.TryParse<ScreenId>(screenIdString.Replace(" ", ""), out screenId);
			if (!isValid) {
				throw new Exception($"Couldn't find a screen with the id {screenIdString.Replace(" ", "")}. Please check the spelling.");
			}
			Visit(screenId);
		}


		string onboardingKey = "Letter";
		string patchNotesKey {
			get {
				return "patch" + GameConstants.VERSION_NUMBER;
			}
		}

		void VisitFirstScreen() {
			if (!PlayerPrefs.HasKey(onboardingKey)) {
				PlayerPrefs.SetInt(onboardingKey, 1);
				PlayerPrefs.SetInt(patchNotesKey, 1);
				Visit(ScreenId.IntroLetter);
				return;
			}

			if (!PlayerPrefs.HasKey(patchNotesKey)) {
				PlayerPrefs.SetInt(patchNotesKey, 1);
				Visit(ScreenId.IntroPatchNotes);
				return;
			}

			if (Release.Flags[gate]) {
				Visit(gatedHomeScreen);
				return;
			}

			Visit(homeScreen);
		}

		Camera GetCamera() {
			var cam = GetComponent<Camera>();
			if (cam == null) {
				cam = GameObject.Find("UI Camera")?.GetComponent<Camera>();
			}
			if (cam == null) {
				throw new Exception($"Could not find UI Camera. Add a camera component to NavigationController or Add the UI Camera prefab to the scene.");
			}
			return cam;
		}

		[Button, GUIColor(0.898f, 0.745f, 0.935f)]
		public void ResetOnboarding() {
			PlayerPrefs.DeleteKey(onboardingKey);
			PlayerPrefs.DeleteKey(patchNotesKey);
		}

		[Button, GUIColor(0.898f, 0.745f, 0.935f)]
		public void ResetPatchNotes() {
			PlayerPrefs.DeleteKey(patchNotesKey);
		}

#if UNITY_EDITOR
		[Title("Editor Only")]
		public ScreenId previewScreen = ScreenId.None;

		void OnValidate() {
			return;
			if (Application.isPlaying) { return; }
			if (Instance == null) {
				Instance = this;
				LoadScreens();
			}

			if (previewScreen != ScreenId.None && currentScreen != previewScreen) {
				LoadScreens();
#pragma warning disable
				Visit(previewScreen);
#pragma warning restore
			}
		}
#endif
	}
}
