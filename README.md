# Violet UI

State-based rendering with live updates in the Unity Editor

## State

```csharp
using Dispatch;

[System.Serializable]
public class UIState : State {
	public int nice;
}
```

## Actions

```csharp
using System;

public static class UIActions {
	public static Action<UIState> IncrementNice => state => {
		state.nice++;
	}

	public static Action<UIState> SetNice(int nice) => state => {
		state.nice = nice;
	}
}
```

## View
```csharp
using VioletUI;

public class MyBaseView : View<UIState> {
	// pull state from gameObject, singleton, disk etc
	UIState state = new UIState();
	public override UIState State => state

	// provide a copy of the state to use for history
	UIState lastState = new UIState();
	public override UIState LastState => lastState;
}

public class AddView : MyBaseView {
	// Render is called every time the state changes
	public override void Render(UIState state, UIState lastState) {
		// only render when the part of the state u care about changes
		if (state.nice == lastState.nice) { return; }

		print($"State of nice changed to {state.nice}");
	}
}

public class AddButtonView : MyBaseView {
	public void Increment() {
		dispatcher.Run(UIActions.Increment);
	}

	public void Set() {
		dispatcher.Run(UIActions.Set(0));
	}
}
```