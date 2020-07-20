using System;
using Dispatch;
using Sirenix.OdinInspector;

namespace VioletUI {
	public abstract class ChildView<TState, T> : View<TState> where TState : IState {
		protected int Index => transform.GetSiblingIndex();
		protected T Item => parent?.Items == null || parent.Items.Count <= Index ? default(T) : parent.Items[Index];
		protected T LastItem => LastState == null || parent?.LastItems == null || parent.LastItems.Count <= Index ? default(T) : parent.LastItems[Index];

		RepeatView<TState, T> parent;

		internal override void OnShowInternal() {
			parent = gameObject.GetComponentInParent<RepeatView<TState, T>>();
			if (parent == null) {
				throw new VioletException($"ChildView {gameObject.name} does not have a RepeatView as a parent. Make sure the parent has a component attached that inherits from RepeatView.");
			}
		}

		internal override void RenderInternal(TState state, TState lastState) {
			if (parent == null || parent.Items == null) {
				return;
			}

			base.RenderInternal(state, lastState);
		}
	}
}