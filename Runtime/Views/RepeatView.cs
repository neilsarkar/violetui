using System;
using System.Collections.Generic;
using Dispatch;
using UnityEngine;

namespace VioletUI {
	public abstract class RepeatView<TState, T> : View<TState> where TState : class, IState {
		public GameObject ViewPrefab;

		public abstract IList<T> Items { get; }
		public abstract IList<T> LastItems { get; }

		internal override void RenderInternal(TState state, TState lastState) {
			base.RenderInternal(state, lastState);

			if (!IsDirtyInternal(state, lastState)) { return; }
			RenderChildren();
		}

		internal override bool IsDirtyInternal(TState state, TState lastState) {
			base.IsDirtyInternal(state, lastState);
			if (lastState != null && Items.Count == LastItems?.Count) { throw new Bail(); }
			if (ViewPrefab == null) { throw new Bail(); }
			return true;
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

			for (int i = 0; i < Items.Count; i++) {
				var child = Instantiate(ViewPrefab, transform);
				var view = child.GetComponent<ChildView<TState, T>>();
				if (view == null) {continue;}
				view.RenderInternal(State, default(TState));
			}
		}
	}
}
