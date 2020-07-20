using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dispatch;
using Sirenix.OdinInspector;

namespace VioletUI {
	[ExecuteAlways]
	public abstract class StateMonoBehaviour<TState> : MonoBehaviour where TState : IState {
		public static StateMonoBehaviour<TState> Singleton;

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

		public void Awake() {
			Singleton = this;
			CopyState();
		}

#if UNITY_EDITOR
		public void OnValidate() {
			Render();
		}

		[Button, GUIColor(0.898f, 0.745f, 0.935f)]
		public void Render() {
			State.TriggerChange();
			CopyState();
		}

		public void Update() {
			if (Application.isPlaying) { return; }
			Singleton = this;
		}
#endif
	}
}
