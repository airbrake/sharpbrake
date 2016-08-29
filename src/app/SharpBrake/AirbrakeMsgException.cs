using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpBrake {
	public class AirbrakeMsgException: Exception {
		public AirbrakeMsgException() {
		}

		public AirbrakeMsgException(string message) : base(message) {
		}

		public AirbrakeMsgException(string message, Exception inner) : base(message, inner) {
		}
	}
}
