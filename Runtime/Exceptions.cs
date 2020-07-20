using System;

namespace VioletUI {
	public class Bail : Exception {
		public Bail() : base("bail") {}
		public Bail(string message) : base(message) { }
	}
}