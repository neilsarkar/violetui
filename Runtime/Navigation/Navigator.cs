using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading;
using UniRx.Async;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VioletUI {
	/// <summary>
	/// Screens maintains a map of <see cref="ScreenId"/> enum to menu screens.
	///
	/// It is primarily used to navigate between screens, and also exposes the <see cref="OnWillVisit"/> and <seealso cref="OnDidVisit"/> lifecycle events.
	/// </summary>
	public class Navigator : TidyBehaviour {
		public ScreenId homeScreen = ScreenId.None;
		public bool hasCamera;
		[ShowIf("hasCamera")]
		public Camera worldCamera;

		public Action OnReady;

		public Action<ScreenId> OnWillVisit;
		public Action<ScreenId> OnWillLeave;
		public Action<ScreenId> OnDidVisit;
		public Action<ScreenId> OnDidLeave;

		public Action<ScreenId> OnModalShow;
		public Action<ScreenId> OnModalHide;

		Dictionary<ScreenId, Screen> screens = new Dictionary<ScreenId, Screen>();
		ScreenId lastScreen = ScreenId.None;
		ScreenId currentScreen = ScreenId.None;
		ScreenId currentModal = ScreenId.None;

		[NonSerialized]
		CancellationTokenSource canceler = null;

		void Awake() {
			LoadScreens();
			VisitFirstScreen();
		}
		void LoadScreens() {
			if (transform.childCount == 0) {
				throw new Exception($"Tried to create a NavigationController with no children - try adding some NavigationScreens to {gameObject.name}");
			}

			ScreenId screenId = ScreenId.None;
			foreach (Screen screen in GetComponentsInChildren<Screen>()) {
				var isValid = Enum.TryParse(screen.name, out screenId);
				if (!isValid) {
					throw new VioletException($"{screen.name} does not have a valid ScreenId. Make sure this screen is added to MenuBuilder.");
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

		/// <summary>
		/// Visit takes a <see cref="ScreenId"/> and transitions the menu to that scene.
		/// </summary>
		/// <param name="screenId"></param>
		public async void Visit(ScreenId screenId) {
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

		public void ShowModal (string screenIdString) {
			ScreenId screenId;
			var isValid = Enum.TryParse<ScreenId>(screenIdString.Replace(" ", ""), out screenId);
			if (!isValid) {
				throw new Exception($"Couldn't find a screen with the id {screenIdString.Replace(" ", "")}. Please check the spelling.");
			}

			ShowModal(screenId);
		}

		protected virtual void VisitFirstScreen() {
			Visit(homeScreen);
		}

		ScreenId ScreenToScreenId(Screen screen) {
			ScreenId ret;
			var slug = screen.name.Replace(" ", "");
			var ok = Enum.TryParse<ScreenId>(screen.name.Replace(" ", ""), out ret);
			if (!ok) {
				throw new VioletEnumException($"{slug} does not exist in ScreenId. This should be set up automatically by navigator but you can add manually as a workaround.");
			}
			return ret;
		}

#if UNITY_EDITOR
		[HideInInspector] public Screen EditingScreen;
		[SerializeField, HideInInspector] ScreenId originalHomeScreen;

		public void Edit(Screen screen) {
			try {
				PrefabUtility.UnpackPrefabInstance(screen.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
			} catch (ArgumentException) {}

			screen.gameObject.SetActive(true);
			EditingScreen = screen;
			originalHomeScreen = homeScreen;

			try {
				homeScreen = ScreenToScreenId(screen);
			} catch(VioletEnumException) {
				print($"Do enum");
			}
		}

		public void FinishEditing() {
			PrefabUtility.SaveAsPrefabAssetAndConnect(EditingScreen.gameObject, $"Assets/Menus/{EditingScreen.name}.prefab", InteractionMode.AutomatedAction);

			EditingScreen.gameObject.SetActive(false);
			EditingScreen = null;
			homeScreen = originalHomeScreen;
		}
#endif
	}
}
