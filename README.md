# Violet UI

State-based rendering with live updates in the Unity Editor

## State

```csharp
using Dispatch;

// State triggers events when actions are dispatched to it.
[System.Serializable]
public class UIState : State {
	public int nice;
	public List<string> bugs = new List<string> () { "wasp", "ant" } ;

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

// Attaching this to a game object allows you to edit state values in Editor and reference the state via a singleton
public class UIStateMB : StateMonoBehaviour<UIState> {
	protected override void CopyState() {
		if (LastState == null) LastState = new UIState();
		LastState.nice = State.nice;
	}
}

// Create a base view letting it know where to find your state
public class MyBaseView : View<UIState> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}

// Render from your base view when the state changes in edit mode or play mode
public class AddView : MyBaseView {
	public override void Render(UIState state, UIState lastState) {
		// only render when the part of the state u care about changes
		if (state.nice == lastState.nice) { return; }

		print($"State of nice changed to {state.nice}");
	}
}

// Dispatch actions from your base view to change state and trigger a re-render
public class AddButtonView : MyBaseView {
	public void Increment() {
		dispatcher.Run(UIActions.Increment);
	}

	public void Set() {
		dispatcher.Run(UIActions.Set(0));
	}
}
```

## RepeatView

```csharp
public abstract class BaseRepeatView<T> : RepeatView<UIState, T> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}

public abstract class BaseChildView<T> : ChildView<UIState, T> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}

public class BugsView : BaseRepeatView<string> () {
	public override List<string> Items => State?.bugs;
	public override List<string> LastItems => LastState?.bugs;
}

public class BugView : BaseView {
	public override void Render(UIState state, UIState lastState) {
		if (lastState != null && Item == LastItem) { return; }

		print($"I am bug number {Index} and my name changed from {LastItem} to {Item}");
	}
}

```