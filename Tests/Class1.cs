using System;
using HopSharp;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class Class1
	{
		[Test]
		public void Testthrow()
		{
			try
			{
				throw new NotImplementedException("Booo");
			}
			catch (Exception e)
			{
				HoptoadClient a = new HoptoadClient();
				a.Send(e);
			}
		}

		[Test]
		public void json()
		{
			HoptoadNotice notice = new HoptoadNotice();

			notice.ApiKey = "12345678";
			notice.ErrorMessage = "sdlfds";
			notice.ErrorClass = "sdflshs";
			notice.Backtrace = "blah1\npoop2";
			notice.Environment = Environment.GetEnvironmentVariables();

			Console.WriteLine(notice.Serialize());

			HoptoadClient client = new HoptoadClient();
			client.Send(notice);
		}
	}
}