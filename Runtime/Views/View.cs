using System;
using System.Runtime.ExceptionServices;
using Dispatch;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace VioletUI {
	[ExecuteAlways]
	public abstract class View<TState> : TidyBehaviour where TState : class, IState {
		/// <summary>
		/// A view requires a reference to State and LastState.
		/// </summary>
		/// <value></value>
		protected abstract TState State { get; }
		protected abstract TState LastState { get; }

		/// <summary>
		/// OnShow is called when the gameObject becomes active in editor or playmode, prior to first Render.
		/// </summary>
		protected virtual void OnShow() { }
		/// <summary>
		/// OnHide is called when the gameObject becomes inactive in editor or playmode, after last Render.
		/// </summary>
		protected virtual void OnHide() { }
		/// <summary>
		/// IsDirty is used to short circuit expensive render calls by focusing on parts of the state you care about.
		///
		/// return `false` to avoid rendering
		/// </summary>
		/// <param name="state">current value of state</param>
		/// <param name="lastState">value of state prior to the action that triggered this render call</param>
		/// <returns></returns>
		protected virtual bool IsDirty(TState state, TState lastState) { return true; }
		/// <summary>
		/// Override Render to update referenced gameobjects with fields of the changed state.
		/// </summary>
		/// <param name="state"></param>
		protected virtual void Render(TState state) { }

		/// <summary>
		/// Dispatcher allows you to send Actions that change the State
		/// </summary>
		protected virtual Dispatcher<TState> Dispatcher {
			get {
				if (dispatcher == null) { dispatcher = new Dispatcher<TState>(State, LastState); }
				return dispatcher;
			}
		}
		Dispatcher<TState> dispatcher;

		// convenience methods for logging in views
		protected void Log(string s) { Violet.Log(s); }
		protected void LogWarning(string s) { Violet.LogWarning(s); }
		protected void LogError(string s) { Violet.LogError(s); }

		// convenience accessor
		RectTransform rectTransform;
		protected RectTransform RectTransform => rectTransform != null ? rectTransform : rectTransform = GetComponent<RectTransform>();

		// Internal methods are so that callers don't have to remember to call base. at the beginning of their implementations
		internal virtual void OnShowInternal() {}
		internal virtual void OnHideInternal() {}
		internal virtual bool IsDirtyInternal(TState state, TState lastState) {
			if (!IsDirty(state, lastState)) { throw new Bail("IsDirty=false"); }
			return true;
		}
		internal virtual void RenderInternal(TState state, TState lastState) {
			try {
				if (gameObject == null) {
					Warn($"RenderInternal | gameObject was null");
					throw new Bail("gameObject is null");
				}
				if (!gameObject.activeSelf) {
					Verbose($"RenderInternal | bailing since {gameObject.name} is not active");
					throw new Bail($"{gameObject.name} is not active");
				}
#if UNITY_EDITOR
				var stage = PrefabStageUtility.GetCurrentPrefabStage();
				if (stage != null && stage.IsPartOfPrefabContents(gameObject)) {
					Verbose($"RenderInternal | bailing since {gameObject.name} is in a prefab");
					throw new Bail($"{gameObject.name} is in a prefab");
				}
#endif
			} catch(MissingReferenceException) {
				Warn($"RenderInternal | MissingReferenceException when trying to access gameObject");
				throw new Bail("gameObject is missing");
			} catch(InvalidOperationException e) {
				Warn($"RenderInternal | {e}");
				throw new Bail("gameObject is missing");

			}

			if (lastState != null) {
				if (!IsDirtyInternal(state, lastState) ) { throw new Bail("IsDirtyInternal=false"); }
			}

			Render(state);
		}

		void RenderWrapper(TState state, TState lastState) {
			try {
				RenderInternal(state, lastState);
			} catch (Bail e) {
				try {
					Verbose($"{gameObject.name} bailed from render - {e.Message}");
				} catch (MissingReferenceException) {}
			}
		}

		internal static bool ForceRender;
		void State_OnChange() {
			RenderWrapper(State, ForceRender ? default(TState) : LastState);
		}

		void OnEnable() {
			if (State == null) { if (Application.isPlaying) { Warn($"State is null in {name} OnEnable"); } return; }
			State.OnChange -= State_OnChange;
			State.OnChange += State_OnChange;
#if UNITY_EDITOR
			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;
#endif
			OnShowInternal();
			OnShow();
			RenderWrapper(State, default(TState));
		}

		void OnDisable() {
			if (State == null) { if (Application.isPlaying) { Warn($"State is null in {name} OnDisable"); } return; }
			State.OnChange -= State_OnChange;
#if UNITY_EDITOR
			EditorApplication.update -= EditorUpdate;
#endif
			OnHideInternal();
			OnHide();
		}

		void Warn(string msg) {
#if VIOLETDEV
			Violet.LogWarning(msg);
#endif
		}

		void Verbose(string msg) {
#if VIOLET_VERBOSE
			Violet.LogVerbose(msg);
#endif
		}

#if UNITY_EDITOR
		protected virtual void EditorUpdate() {
			if (Application.isPlaying) { return; }
			if (State == null) { Warn("State is null"); return; }
			try {
				if (gameObject == null);
			} catch(MissingReferenceException e) {
				EditorApplication.update -= EditorUpdate;
				return;
			}
			State.OnChange -= State_OnChange;
			State.OnChange += State_OnChange;
		}
#endif

	}
}
