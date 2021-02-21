using UnityEngine;

public interface IRepeatView {
	void RegenerateChildren();
	GameObject ViewPrefab { get; }
}