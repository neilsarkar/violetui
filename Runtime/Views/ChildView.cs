using System;
using Dispatch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VioletUI {
	public abstract class ChildView<TState, T> : View<TState> where TState : class, IState {
		protected int Index => transform.GetSiblingIndex();
		protected T Item => parent?.Items == null || parent.Items.Count <= Index ? default(T) : parent.Items[Index];
		protected T LastItem => LastState == null || parent?.LastItems == null || parent.LastItems.Count <= Index ? default(T) : parent.LastItems[Index];

		protected virtual void Render(T item, int index, TState state) {}
		protected virtual bool IsDirty(T item, T lastItem) { return true; }

		RepeatView<TState, T> parent;

		internal override void OnShowInternal() {
			parent = gameObject.GetComponentInParent<RepeatView<TState, T>>();
			if (parent == null) {
				throw new VioletException($"ChildView {gameObject.name} does not have a RepeatView as a parent. Make sure the parent has a component attached that inherits from RepeatView.");
			}
		}

		internal override void RenderInternal(TState state, TState lastState) {
			base.RenderInternal(state, lastState);
			Render(Item, Index, state);
		}

		internal override bool IsDirtyInternal(TState state, TState lastState) {
			base.IsDirtyInternal(state, lastState);
			if (parent == null || parent.Items == null) { throw new Bail(); }
			if (!IsDirty(Item, LastItem)) { throw new Bail(); }

			return true;
		}

#if UNITY_EDITOR
		public override void Update() {
			base.Update();
			if (Application.isPlaying) { return; }

			parent = gameObject.GetComponentInParent<RepeatView<TState, T>>();
		}
#endif
	}
}