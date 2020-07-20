using System;
using Dispatch;
using UnityEngine;

namespace VioletUI {
	[ExecuteAlways]
	public abstract class View<TState> : TidyBehaviour, IView<TState> where TState : IState {
		public abstract TState State { get; }
		public abstract TState LastState { get; }
		[NonSerialized] public int Index;

		public virtual void OnShow() { }
		public virtual void OnHide() { }
		public virtual void Render(TState state, TState lastState) { }

		public void OnEnable() {
			if (State == null) { if (Application.isPlaying) { Debug.LogWarning($"State is null in {name} OnEnable"); } return; }
			State.OnChange += State_OnChange;
			OnShowInternal();
			OnShow();
			try {
				RenderInternal(State, default(TState));
			} catch (NullReferenceException e) {
				// TODO: decorate error with stack trace
				UnityEngine.Debug.LogError($"VioletUI: Failed OnShow render of <color=#8d27a3>{name}</color>. Make sure you use <color=green>lastState?.foo</color> and not <color=red>lastState.foo</color>)");
				throw e;
			}
		}

		public void OnDisable() {
			if (State == null) { if (Application.isPlaying) { Debug.LogWarning($"State is null in {name} OnDisable"); } return; }
			State.OnChange -= State_OnChange;
			OnHideInternal();
			OnHide();
		}

		void State_OnChange() {
			RenderInternal(State, LastState);
		}

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
		internal virtual void RenderInternal(TState state, TState lastState) {
			try {
				if (gameObject == null) {return;}
			} catch(MissingReferenceException) { return; }
			Render(state, lastState);
		}

#if UNITY_EDITOR
		public void Update() {
			if (Application.isPlaying) { return; }
			if (State == null) { return; }
			State.OnChange -= State_OnChange;
			State.OnChange += State_OnChange;
		}
#endif

	}
}
