using Dispatch;

namespace VioletUI {
	public interface IView<TState> where TState : IState {
		TState State { get; }
		TState LastState { get; }
		void Render(TState state, TState lastState);
	}
}
