using System.Collections.Generic;
using Dispatch;

namespace VioletUI {
	public abstract class RepeatChildView<TState, T> : View<TState> where TState : IState {
		protected T Item => (Items == null || Items.Count <= Index) ? default(T) : Items[Index];
		protected T LastItem => (LastItems == null || LastItems.Count <= Index) ? default(T) : LastItems[Index];

		protected abstract List<T> Items { get; }
		protected abstract List<T> LastItems { get; }
	}
}