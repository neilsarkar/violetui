using System;
using Dispatch;

namespace VioletUI {
	public class View<TState> : TidyBehaviour, IView<TState> where TState : IState {
		public virtual TState State {get; set;}
		public virtual TState LastState {get; set;}

		public void OnEnable() {
			State.OnChange += State_OnChange;
			OnShow();
			try {
				Render(State, default(TState));
			} catch(NullReferenceException e)  {
				// e4bdee
				var color = new UnityEngine.Color(0.898f, 0.745f, 0.935f);
				UnityEngine.Debug.LogError($"VioletUI: Failed OnShow render of <color=#8d27a3>{name}</color>. Make sure you use <color=green>lastState?.foo</color> and not <color=red>lastState.foo</color>)");
				throw e;
			}
		}

		public void OnDisable() {
			State.OnChange -= State_OnChange;
			OnHide();
		}

		void State_OnChange() {
			Render(State, LastState);
		}

		public virtual void OnShow() {}
		public virtual void OnHide() {}
		public virtual void Render(TState state, TState lastState) {}
	}
}
