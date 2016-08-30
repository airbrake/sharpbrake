using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;

namespace SharpBrake._2010Tests {
	/// <summary>
	/// Summary description for Log4NetAirbrakeAppenderTests
	/// </summary>
	[TestClass]
	public class Log4NetAirbrakeAppenderTests {
		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		internal static readonly ILog Logger = LogManager.GetLogger(typeof(Log4NetAirbrakeAppenderTests));
		[TestMethod]
		public void TestMethod1() {
			Logger.Error("Error as a string");
			try {
				Method2();
			} catch (Exception ex) {
				Logger.Warn("My custom Error message", ex);
			}
		}

		private void Method2() {
			string s = "SampleString";
			throw new ApplicationException("Error in an object:" + s);
		}
	}
}
