using System;
using Dispatch;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

namespace VioletUI {
	public abstract class ChildView<TState, T> : View<TState> where TState : class, IState {
		/// <summary>
		/// Index returns this elements index in the parent RepeatView
		/// </summary>
		/// <returns>int</returns>
		protected int Index => index > -1 ? index : index = GetIndex();
		/// <summary>
		/// Item returns the element associated with this ChildView
		/// </summary>
		/// <returns>T Item</returns>
		protected T Item => parent?.Items == null || parent.Items.Count <= Index ? default(T) : parent.Items[Index];
		protected T LastItem => LastState == null || parent?.LastItems == null || parent.LastItems.Count <= Index ? default(T) : parent.LastItems[Index];

		protected virtual void Render(T item, int index, TState state) {}
		protected virtual bool IsDirty(T item, T lastItem) { return true; }

		RepeatView<TState, T> parent;

		internal override void OnShowInternal() {
			parent = gameObject.GetComponentInParent<RepeatView<TState, T>>();
		}

		internal override void RenderInternal(TState state, TState lastState) {
			base.RenderInternal(state, lastState);
			Render(Item, Index, state);
		}

		internal override bool IsDirtyInternal(TState state, TState lastState) {
			base.IsDirtyInternal(state, lastState);
			if (parent == null || parent.Items == null) { throw new Bail($"Parent was null - parent={parent} parent.Items={parent?.Items}"); }
			if (!IsDirty(Item, LastItem)) { throw new Bail($"IsDirty returned false - Item={Item} LastItem={LastItem}"); }

			return true;
		}

		int index = -1;
		int GetIndex() {
			var t = transform;
			while(t.parent != null) {
				if (t.parent.GetComponent<RepeatView<TState, T>>() != null) {
					// account for the first item being the prefab
					return Math.Max(0, t.GetSiblingIndex());
				}
				t = t.parent;
			}

#if UNITY_EDITOR
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage == null) {
				throw new VioletException($"ChildView {gameObject.name} does not have a RepeatView as a parent. Make sure the parent has a component attached that inherits from RepeatView.");
			}
#endif
			return -1;
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