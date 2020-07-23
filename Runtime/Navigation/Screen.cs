using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UniRx.Async;

namespace VioletUI {
	public class Screen : TidyBehaviour {

		internal async UniTask<bool> Show(CancellationToken token) {
			gameObject.SetActive(true);
			await UniTask.DelayFrame(1);
			return true;
		}

		internal async UniTask<bool> Hide(CancellationToken token) {
			gameObject.SetActive(false);
			await UniTask.DelayFrame(1);
			return true;
		}

	}
}