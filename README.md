# Violet UI

Declarative state-based rendering with live updates in the Unity Editor.

```csharp
// Represent your UI state with any serializable class
[System.Serializable]
public class UIState : Dispatch.State {
	public int hello;
	public List<string> animals = new List<string> () { "tiger", "otter" } ;
}

// Attach this to a game object and change fields in the Editor to trigger view renders
public class UIStateMB : VioletUI.StateMonoBehaviour<UIState> {
	protected override void CopyState() {
		if (LastState == null) LastState = new UIState();
		LastState.hello = State.hello;
	}
}

// Attach this to a game object anywhere in the hierarchy to render when the state changes
public class HelloView : UIStateMB.View {
	protected override void Render(UIState state) {
		print($"hello is now {state.hello}");
	}
}
```

## Views

Views are subscribed to state changes - they will render when you change a value in the Unity Editor or dispatch an  [`Action`](#actions).

### Performant Rendering

Define an `IsDirty` check to choose which state changes will cause a render.

```csharp
public class HelloView : BaseView {
	// IsDirty lets you compare the current state to the state before the most recent change
	protected override bool IsDirty(UIState state, UIState lastState) {
		return state.hello != lastState.hello;
	}

	// Render will not be called unless IsDirty returns true
	protected override void Render(UIState state) {
		ExpensiveRenderMethod();
	}
}
```

### Repeat View

To render a collection of items (for e.g. a player's inventory), you can use `RepeatView` and `ChildView`

In your `RepeatView`, define public properties an `IList` to render the items from.

```csharp
public class AnimalsView : UIStateMB.RepeatView<string> () {
	public override List<string> Items => State?.animals;
	public override List<string> LastItems => LastState?.animals;
}
```

You can now attach this to a gameObject and it will let you select a prefab. This example will instantiate that prefab once for each animal in the list.

You can add a `ChildView` to the prefab if you want.

```csharp
public class AnimalView : UIStateMB.ChildView<string> {
	protected override void Render(string animal, int index, UIState state) {
		print($"I am animal number {index} and my name is {animal}");
	}

	protected override bool IsDirty(string animal, string lastAnimal) {
		return animal != lastAnimal;
	}
}
```

## Actions

Updating the state is decoupled from the UI hierarchy using actions.

```csharp
using System;

public static class UIActions {
	// define actions quickly with this one line shorthand
	public static Action<UIState> IncrementHello => state => {
		state.hello++;
	}

	// works with any number of arguments
	public static Action<UIState> SetHello(int hello) => state => {
		state.hello = hello;
	}
}
```

### Dispatching actions

In a View, you can dispatch from any method.

```csharp
public class IncrementView : BaseView {
	protected override void OnShow() {
		dispatcher.Run(UIActions.Increment);
	}

	protected override void OnHide() {
		dispatcher.Run(UIActions.SetHello(0));
	}

	public void MyAction() {
		dispatcher.Run(UIActions.SetHello(66));
	}
}
```

Outside of a View, you can use the dispatcher on the singleton.

```csharp
public class MyButton : UnityEngine.UI.Button {
	void Start() {
		UIStateMB.Singleton.Dispatcher.Run(UIActions.Increment);
	}
}
```

