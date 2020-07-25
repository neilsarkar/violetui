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

		[Button, GUIColor(0.898f, 0.745f, 0.935f)]
		[Shortcut("VioletRender", KeyCode.Period, ShortcutModifiers.Action)]
		void Render() {
			print($"Rendering!");
			State.TriggerChange();
			CopyState();
		}

		void Update() {
			if (Application.isPlaying) { return; }
			Singleton = this;
		}
#endif
	}
}
