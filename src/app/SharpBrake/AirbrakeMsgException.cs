using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpBrake {
	/// <summary>
	/// AirBrake Exception Class used to generate messages
	/// </summary>
	public class AirbrakeMsgException: Exception {
		/// <summary>
		/// Default Constructor
		/// </summary>
		public AirbrakeMsgException() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public AirbrakeMsgException(string message) : base(message) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public AirbrakeMsgException(string message, Exception inner) : base(message, inner) {
		}
	}
}
