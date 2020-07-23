using System;

namespace VioletUI {
	public class Bail : Exception {
		public Bail() : base("bail") {}
		public Bail(string message) : base(message) { }
	}

	public class VioletException : Exception {
		public VioletException(string message) : base(message) { }
	}

	public class VioletEnumException : VioletException {
		public VioletEnumException(string message) : base(message) { }
	}
}