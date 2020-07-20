using System.Collections.Generic;
using Dispatch;

namespace VioletUI {
	public abstract class RepeatChildView<TState, T> : View<TState> where TState : IState {
		public T Item => throw new System.NotImplementedException();
		public T LastItem => throw new System.NotImplementedException();
		// public T Item => (Items == null || Items.Count <= Index) ? default(T) : Items[Index];
		// public T LastItem => (LastItems == null || LastItems.Count <= Index) ? default(T) : LastItems[Index];

		public abstract List<T> Items { get; }
		public abstract List<T> LastItems { get; }
	}
}