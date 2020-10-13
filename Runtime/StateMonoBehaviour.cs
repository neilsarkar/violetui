using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dispatch;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.ShortcutManagement;
#endif

namespace VioletUI {
	[ExecuteAlways]
	public abstract class StateMonobehaviour<TState> : TidyBehaviour where TState : class, IState {
		public static StateMonobehaviour<TState> Singleton;

		public TState State;
		[NonSerialized, HideInInspector] public TState LastState;
		protected abstract void CopyState();

		Dispatcher<TState> dispatcher;
		public Dispatcher<TState> Dispatcher {
			get {
				if (dispatcher == null) {
					dispatcher = new Dispatcher<TState>(State, LastState);
				}
				return dispatcher;
			}
		}

		public class View : View<TState> {
			protected override TState State => Singleton?.State;
			protected override TState LastState => Singleton?.LastState;
			protected override Dispatcher<TState> Dispatcher => Singleton?.Dispatcher;
		}

		public abstract class RepeatView<T> : RepeatView<TState, T> {
			protected override TState State => Singleton?.State;
			protected override TState LastState => Singleton?.LastState;
			protected override Dispatcher<TState> Dispatcher => Singleton?.Dispatcher;
		}

		public abstract class ChildView<T> : ChildView<TState, T> {
			protected override TState State => Singleton?.State;
			protected override TState LastState => Singleton?.LastState;
			protected override Dispatcher<TState> Dispatcher => Singleton?.Dispatcher;
		}

		protected virtual void Awake() {
			Singleton = this;
			CopyState();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate() {
			Render();
		}

		protected virtual void Update() {
			RenderShortcut.OnPress -= Render;
			RenderShortcut.OnPress += Render;
			Singleton = this;
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			RenderShortcut.OnPress -= Render;
			Singleton = null;
		}

		[Button("Render (shortcut: cmd+;)"), GUIColor(Violet.r, Violet.g, Violet.b)]
		protected void Render() {
			if (State == null) {
				Violet.LogWarning($"state is null for {name}");
				return;
			}
			View.ForceRender = true;
			State.TriggerChange();
			View.ForceRender = false;
			CopyState();
		}

#endif
	}
}
