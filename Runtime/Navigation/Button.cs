using UnityEngine;
using UnityEngine.EventSystems;

namespace VioletUI{
	[ExecuteAlways]
	public class Button : UnityEngine.UI.Button {
		public ScreenId visitScreen;

		public override void OnSubmit(BaseEventData eventData) {
			base.OnSubmit(eventData);
			Submit();
		}

		public override void OnPointerClick(PointerEventData eventData) {
			base.OnPointerClick(eventData);
			Submit();
		}

		Navigator navigator;
		protected override void Awake() {
			base.Awake();
			navigator = gameObject.GetComponentInParent<Navigator>();
		}

		protected virtual void Submit() {
			Violet.Log($"Button {name} clicked");
			if (visitScreen != ScreenId.None) {
				navigator.Visit(visitScreen);
			}
		}
	}
}