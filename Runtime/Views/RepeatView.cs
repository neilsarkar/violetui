using System;
using System.Collections.Generic;
using Dispatch;
using UnityEngine;

namespace VioletUI {
	public abstract class RepeatView<TState> : View<TState> where TState : IState {
		protected abstract int Count {get;}
		public GameObject ViewPrefab;

		static int lastCount;

		internal override void OnShowInternal() {
			base.OnShowInternal();
			lastCount = 0;
		}

		internal override void RenderInternal(TState state, TState lastState) {
			if (Count == lastCount) { return; }
			if (ViewPrefab == null) { return; }

			lastCount = Count;

			RenderChildren();
			base.RenderInternal(state, lastState);
		}

		void RenderChildren() {
			// can't use foreach or for loop because it updates the array in place
			try {
				while(transform.childCount > 0) {
					DestroyImmediate(transform.GetChild(0).gameObject);
				}
			} catch (InvalidOperationException) {
				return;
			}

			for(int i = 0; i < Count; i++) {
				var child = Instantiate(ViewPrefab, transform);
				var view = child.GetComponent<View<TState>>();
				if (view == null) {
					Debug.LogWarning($"VioletUI: child {child.name} of {name} does not have a component that inherits from View. Make sure a View component is added.");
					return;
				}
				view.Index = i;
				view.RenderInternal(State, default(TState));
			}
		}
	}
}
