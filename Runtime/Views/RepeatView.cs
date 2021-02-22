using System;
using System.Collections.Generic;
using Dispatch;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VioletUI {
	public abstract class RepeatView<TState, T> : View<TState>, IRepeatView where TState : class, IState {
		[SerializeField]
		GameObject viewPrefab;
		public GameObject ViewPrefab => viewPrefab;

		public abstract IList<T> Items { get; }
		public abstract IList<T> LastItems { get; }

		internal override void RenderInternal(TState state, TState lastState) {
			base.RenderInternal(state, lastState);

			if (!IsDirtyInternal(state, lastState)) { return; }
			RenderChildrenInternal();
		}

		internal override bool IsDirtyInternal(TState state, TState lastState) {
			if (lastState != null) {
				base.IsDirtyInternal(state, lastState);
				if (Items.Count == transform.childCount) {
					throw new Bail($"No change in item count - {Items.Count}=={LastItems?.Count} ");
				}
			}
			if (ViewPrefab == null) {
				print($"ViewPrefab b null count view={GetType().Name}");
				throw new Bail($"ViewPrefab is null");
			}
			return true;
		}

		internal void RenderChildrenInternal() {
			var childCount = transform.childCount;
			for (int i = childCount - 1; i >= Items.Count; i--) {
				if (Application.isPlaying) {
					Destroy(transform.GetChild(i).gameObject);
				} else {
					DestroyImmediate(transform.GetChild(i).gameObject);
				}
			}

			var diff = Items.Count - childCount;
			for (int i = 0; i < Math.Min(childCount, childCount - diff); i++) {
				if (!transform.GetChild(i).gameObject.activeSelf) {
					transform.GetChild(i).gameObject.SetActive(true);
				}
			}

			for (int i = 0; i < Items.Count - childCount; i++) {
				CreateChild(i);
			}
		}

		GameObject CreateChild(int index) {
			return Instantiate(ViewPrefab, transform);
		}

		/// <summary>
		/// Call RegenerateChildren in editor only to regenerate the children
		/// when the child prefab is updated
		/// </summary>
		public void RegenerateChildren() {
			StartCoroutine(RegenerateChildrenNextFrame());
		}

		IEnumerator RegenerateChildrenNextFrame() {
			// we have to wait a frame because PrefabListener is run as the asset is saving
			// and the menu prefab won't be updated until the next frame
#if UNITY_EDITOR
			yield return null;
			Transform t = transform;
			VioletScreen screen = default;
			while (t != null) {
				screen = t.GetComponent<VioletScreen>();
				if (screen != null) { break; }
				t = t.parent;
			}

			if (screen == null) {
				Violet.LogError($"Can't regenerate children bc screen is null. name={gameObject.name} parent={transform.parent}");
				yield break;
			}

			var wasEditing = screen.isEditing;
			if (!wasEditing) {
				screen.UnpackPrefab();
			}
			for (int i = transform.childCount - 1; i >= 0; i--) {
				DestroyImmediate(transform.GetChild(i).gameObject);
			}

			for (int i = 0; i < Items.Count; i++) {
				CreateChild(i);
			}

			if (!wasEditing) {
				screen.PackPrefab();
			}
#else
			throw new NotImplementedException($"RegenerateChildren not available in builds");
#endif
		}
	}
}
