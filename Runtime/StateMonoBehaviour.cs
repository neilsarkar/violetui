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
			protected override Dispatcher<TState> Dispatcher => Dispatcher;
		}

		public abstract class RepeatView<T> : RepeatView<TState, T> {
			protected override TState State => Singleton?.State;
			protected override TState LastState => Singleton?.LastState;
			protected override Dispatcher<TState> Dispatcher => Dispatcher;
		}

		public abstract class ChildView<T> : ChildView<TState, T> {
			protected override TState State => Singleton?.State;
			protected override TState LastState => Singleton?.LastState;
			protected override Dispatcher<TState> Dispatcher => Dispatcher;
		}

		void Awake() {
			Singleton = this;
			CopyState();
		}

#if UNITY_EDITOR
		void OnValidate() {
			Render();
		}

		void Update() {
			if (Application.isPlaying) { return; }

			RenderShortcut.OnPress -= Render;
			RenderShortcut.OnPress += Render;
			Singleton = this;
		}

		[Button("Render (shortcut: cmd+;)"), GUIColor(Violet.r, Violet.g, Violet.b)]
		void Render() {
			if (State == null) { Violet.LogWarning($"state is null for {name}");}
			State.TriggerChange();
			CopyState();
		}

#endif
	}
}
