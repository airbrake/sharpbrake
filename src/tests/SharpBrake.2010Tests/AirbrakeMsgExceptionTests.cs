using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpBrake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBrake.Tests {
	[TestClass()]
	public class AirbrakeMsgExceptionTests {
		[TestMethod()]
		public void AirbrakeMsgExceptionTest() {
			AirbrakeMsgException ex = new AirbrakeMsgException();
		}

		[TestMethod()]
		public void AirbrakeMsgExceptionTest1() {
			AirbrakeMsgException ex = new AirbrakeMsgException("My Msg");
			Assert.AreEqual("My Msg", ex.Message);
		}

		[TestMethod()]
		public void AirbrakeMsgExceptionTest2() {
			AirbrakeMsgException ex = new AirbrakeMsgException("My Msg", new ApplicationException("My Msg2"));
			Assert.AreEqual("My Msg", ex.Message);
			Assert.IsNotNull(ex.InnerException);
			Assert.AreEqual("My Msg2", ex.InnerException.Message);
		}
	}
}