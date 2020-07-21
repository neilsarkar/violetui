# Violet UI

State-based rendering with live updates in the Unity Editor.

## States

Use states to represent the data of your UI.

```csharp
[System.Serializable]
public class UIState : Dispatch.State {
	public int hello;
	public List<string> animals = new List<string> () { "tiger", "otter" } ;
}

// Attach this to a game object to get a singleton state reference that allows you to trigger renders by editing values in Editor
public class UIStateMB : VioletUI.StateMonoBehaviour<UIState> {
	protected override void CopyState() {
		if (LastState == null) LastState = new UIState();
		LastState.hello = State.hello;
	}
}

// Add this to a game object anywhere in the hierarchy to render when state changes
public class HelloView : VioletUI.View<UIState> {
	// Tell the view where to find your state
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;

	public override void Render(UIState state, UIState lastState) {
		print($"hello is now {state.hello}");
	}
}

```

## Views

Views are subscribed to state changes and will render by default on each state change.

I recommend creating base views to avoid specifying the state every time.

```csharp
public class BaseView : VioletUI.View<UIState> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}
```

### Performant Rendering

You can choose which state changes will cause a re-render.

```csharp
public class HelloView : BaseView {
	public override void Render(UIState state, UIState lastState) {
		// some expensive rendering
	}

	// Render code is not called unless state.hello changes
	public override bool ShouldRender(UIState state, UIState lastState) {
		return state.hello != lastState.hello;
	}
}
```

## Actions

Define actions to update your state. Firing an action triggers re-rendering.

```csharp
using System;

public static class UIActions {
	public static Action<UIState> IncrementHello => state => {
		state.hello++;
	}

	public static Action<UIState> SetHello(int hello) => state => {
		state.hello = hello;
	}
}
```

### Dispatching actions

You can dispatch actions from any method in a View.

```csharp
public class IncrementView : BaseView {
	public override void OnShow() {
		dispatcher.Run(UIActions.Increment);
	}

	public override void OnHide() {
		dispatcher.Run(UIActions.SetHello(0));
	}
}
```

Outside of a View you can use the singleton's dispatcher.

```csharp
public class MyButton : UnityEngine.UI.Button {
	void Start() {
		UIStateMB.Singleton.Dispatcher.Run(UIActions.Increment);
	}
}
```

## RepeatView

To render a list of items (for e.g. a player's inventory) you can use `RepeatView` and `ChildView`.

First you'll want to create the base views

```csharp
public abstract class BaseRepeatView<T> : RepeatView<UIState, T> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}

public abstract class BaseChildView<T> : ChildView<UIState, T> {
	protected override UIState State => UIStateMB.Singleton.State;
	protected override UIState LastState => UIStateMB.Singleton.LastState;
}
```

In your `RepeatView`, expose public properties for an `IList` to render the items from.

```csharp
public class AnimalsView : BaseRepeatView<string> () {
	public override List<string> Items => State?.animals;
	public override List<string> LastItems => LastState?.animals;
}
```

In your `ChildView`, you can now reference the `Index`, `Item` and `LastItem` that you are rendering.

```csharp
public class AnimalView : BaseChildView<string> {
	public override void Render(UIState state, UIState lastState) {
		print($"I am animal number {Index} and my name changed from {LastItem} to {Item}");
	}

	public override bool ShouldRender(string animal, string lastAnimal) {
		return animal != lastAnimal;
	}
}
```