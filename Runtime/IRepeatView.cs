using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IRepeatView {
	UniTask RegenerateChildren();
	GameObject ViewPrefab { get; }
}