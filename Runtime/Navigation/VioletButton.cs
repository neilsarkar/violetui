using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VioletUI {
	[ExecuteAlways]
	public class VioletButton : UnityEngine.UI.Button {
		public ScreenId visitScreen;

		Navigator navigator;
		protected override void Awake() {
			base.Awake();
			navigator = gameObject.GetComponentInParent<Navigator>();
			Violet.LogVerbose($"Button awoken. navigator={navigator}");
		}

		protected override void OnEnable() {
			this.onClick.AddListener(Submit);
		}

		protected override void OnDisable() {
			this.onClick.RemoveListener(Submit);
		}

		protected override void OnDestroy() {
			this.onClick.RemoveListener(Submit);
		}

		protected virtual void Submit() {
			Violet.LogVerbose($"Button {name} clicked");
			if (visitScreen != ScreenId.None) {
				Violet.LogVerbose($"Visiting {visitScreen} navigator={navigator}");
				navigator.Visit(visitScreen);
			}
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
			var topLeft = transform.position + new Vector3(rect.xMin, rect.yMin, 0);
			var bottomRight = transform.position + new Vector3(rect.xMax, rect.yMax, 0);
			if (topLeft.x <= mousePosition.x && mousePosition.x <= bottomRight.x &&
					topLeft.y <= mousePosition.y && mousePosition.y <= bottomRight.y ) {
				Submit();
			} else {
				Violet.LogVerbose($"topLeft={topLeft} bottomRight={bottomRight} mousePosition={mousePosition}");
			}
		}
#endif
	}
}