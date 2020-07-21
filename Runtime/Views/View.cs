using System;
using System.Runtime.ExceptionServices;
using Dispatch;
using UnityEngine;

namespace VioletUI {
	[ExecuteAlways]
	public abstract class View<TState> : TidyBehaviour where TState : class, IState {
		protected abstract TState State { get; }
		protected abstract TState LastState { get; }

		/// <summary>
		/// OnShow is called when the gameObject becomes active in editor or playmode
		/// </summary>
		protected virtual void OnShow() { }
		/// <summary>
		/// OnHide is called when the gameObject becomes inactive in editor or playmode
		/// </summary>
		protected virtual void OnHide() { }
		/// <summary>
		/// ShouldRender is used to short circuit expensive render calls by focusing on parts of the state you care about.
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

		Dispatcher<TState> m_dispatcher;
		protected Dispatcher<TState> dispatcher {
			get {
				if (m_dispatcher == null) { m_dispatcher = new Dispatcher<TState>(State, LastState); }
				return m_dispatcher;
			}
		}

		// Internal methods are so that callers don't have to remember to call base. at the beginning of their implementations
		internal virtual void OnShowInternal() {}
		internal virtual void OnHideInternal() {}
		internal virtual bool IsDirtyInternal(TState state, TState lastState) {
			if (!IsDirty(state, lastState)) { throw new Bail(); }
			return true;
		}
		internal virtual void RenderInternal(TState state, TState lastState) {
			try {
				if (gameObject == null) {
					Warn($"RenderInternal | gameObject was null");
					throw new Bail();
				}
			} catch(MissingReferenceException) {
				Warn($"RenderInternal | MissingReferenceException when trying to access gameObject");
				throw new Bail();
			}

			if (lastState != null) {
				if (!IsDirtyInternal(state, lastState) ) { throw new Bail(); }
			}

			Render(state);
		}

		void RenderWrapper(TState state, TState lastState) {
			try {
				RenderInternal(state, lastState);
			} catch (Bail) {
				try {
					Verbose($"{gameObject.name} bailed from render");
				} catch (MissingReferenceException) {}
			}
		}

		void State_OnChange() {
			RenderWrapper(State, LastState);
		}

		void OnEnable() {
			if (State == null) { if (Application.isPlaying) { Warn($"State is null in {name} OnEnable"); } return; }
			State.OnChange += State_OnChange;
			OnShowInternal();
			OnShow();
			RenderWrapper(State, default(TState));
		}

		void OnDisable() {
			if (State == null) { if (Application.isPlaying) { Warn($"State is null in {name} OnDisable"); } return; }
			State.OnChange -= State_OnChange;
			OnHideInternal();
			OnHide();
		}

		void Warn(string msg) {
#if VIOLETDEV
			UnityEngine.Debug.LogWarning(msg);
#endif
		}

		void Verbose(string msg) {
#if VIOLETDEV && VIOLET_VERBOSE
			UnityEngine.Debug.Log(msg);
#endif
		}

#if UNITY_EDITOR
		public virtual void Update() {
			if (Application.isPlaying) { return; }
			if (State == null) { Debug.LogWarning("State is null"); return; }
			State.OnChange -= State_OnChange;
			State.OnChange += State_OnChange;
		}
#endif

	}
}
