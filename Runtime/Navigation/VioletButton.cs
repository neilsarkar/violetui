using UnityEngine;
using UnityEngine.EventSystems;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VioletUI {
	[ExecuteAlways]
	public class VioletButton : UnityEngine.UI.Button {
		public ScreenId visitScreen;
		public ScreenId showModal;
		public bool closeModal;

		[NonSerialized]
		public bool isSelected;

		protected Navigator navigator;
		protected override void Awake() {
			base.Awake();
			navigator = gameObject.GetComponentInParent<Navigator>();
			Violet.LogVerbose($"Button awoken. navigator={navigator}");
		}

		protected virtual void Submit() {
			Violet.LogVerbose($"Button {name} clicked");
			if (visitScreen != ScreenId.None) {
				Violet.LogVerbose($"Visiting {visitScreen} navigator={navigator}");
				_ = navigator.Visit(visitScreen);
			}
			if (closeModal) {
				Violet.LogVerbose($"Hiding modal navigator={navigator}");
				_ = navigator.HideModal();
			}
			if (showModal != ScreenId.None) {
				Violet.LogVerbose($"Showing modal {showModal} navigator={navigator}");
				navigator.ShowModal(showModal);
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			this.onClick.AddListener(Submit);
		}

		protected override void OnDisable() {
			base.OnDisable();
			this.onClick.RemoveListener(Submit);
			isSelected = false;
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			this.onClick.RemoveListener(Submit);
		}

		public override void OnSelect(BaseEventData eventData) {
			base.OnSelect(eventData);
			isSelected = true;
		}

		public override void OnDeselect(BaseEventData eventData) {
			base.OnDeselect(eventData);
			isSelected = false;
		}

#if UNITY_EDITOR
		void OnGUI() {
			if (Application.isPlaying) {return;}
			if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint) {
				EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
				return;
			}
			if(Event.current.type != EventType.MouseDown) { return; }

			var mousePosition = Event.current.mousePosition;
			mousePosition.y = UnityEngine.Screen.height - mousePosition.y;

			var rect = GetComponent<RectTransform>().rect;
			var camera = GetComponentInParent<Canvas>().worldCamera;
			if (camera != null) {
				mousePosition = camera.ScreenToWorldPoint(mousePosition);
			}
			var topLeft = transform.position + new Vector3(rect.xMin, rect.yMin, 0);
			var bottomRight = transform.position + new Vector3(rect.xMax, rect.yMax, 0);
			if (topLeft.x <= mousePosition.x && mousePosition.x <= bottomRight.x &&
					topLeft.y <= mousePosition.y && mousePosition.y <= bottomRight.y ) {
				Submit();
			} else {
				Violet.LogVerbose($"transform.position={transform.position} topLeft={topLeft} bottomRight={bottomRight} mousePosition={mousePosition}");
			}
		}

#endif
	}

}