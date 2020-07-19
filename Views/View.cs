using System;
using Dispatch;
using UnityEngine;

namespace VioletUI {
	[ExecuteInEditMode]
	public abstract class View<TState> : TidyBehaviour, IView<TState> where TState : IState {
		public abstract TState State {get;}
		public abstract TState LastState {get;}

		public void OnEnable() {
			if (State == null) { Debug.LogWarning($"State is null in {name} OnEnable"); return; }
			State.OnChange += State_OnChange;
			OnShow();
			try {
				Render(State, default(TState));
			} catch(NullReferenceException e)  {
				UnityEngine.Debug.LogError($"VioletUI: Failed OnShow render of <color=#8d27a3>{name}</color>. Make sure you use <color=green>lastState?.foo</color> and not <color=red>lastState.foo</color>)");
				throw e;
			}
		}

		public void OnDisable() {
			if (State == null) { Debug.LogWarning($"State is {name} OnDisable"); return; }
			State.OnChange -= State_OnChange;
			OnHide();
		}

		void State_OnChange() {
			Render(State, LastState);
		}

		Dispatcher<TState> m_dispatcher;
		protected Dispatcher<TState> dispatcher {
			get {
				if (m_dispatcher == null) { m_dispatcher = new Dispatcher<TState>(State, LastState); }
				return m_dispatcher;
			}
		}


		public virtual void OnShow() {}
		public virtual void OnHide() {}
		public virtual void Render(TState state, TState lastState) {}

#if UNITY_EDITOR
    public void Update() {
				if (Application.isPlaying) { return; }
				State.OnChange -= State_OnChange;
				State.OnChange += State_OnChange;
    }
#endif

	}
}
