using System;
using HopSharp;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class HoptoadTests
	{
		[Test]
		public void Can_send_an_exception()
		{
			try
			{
				throw new NotImplementedException("Booo");
			}
			catch (Exception e)
			{
				// TODO would like to mock the HttpWebRequest call... maybe dig up my TypeMock license
				HoptoadClient a = new HoptoadClient();
				a.Send(e);
			}
		}

		[Test]
		public void Can_convert_HoptoadNotice_to_json()
		{
			HoptoadNotice notice = new HoptoadNotice();

			notice.ApiKey = "12345678";	
			notice.ErrorMessage = "sdlfds";
			notice.ErrorClass = "sdflshs";
			notice.Backtrace = "blah1\npoop2";

			string json = notice.Serialize();

			Console.WriteLine(json);
				Assert.AreEqual("{\"notice\":{\"api_key\":\"12345678\",\"error_class\":\"sdflshs\",\"error_message\":\"sdlfds\",\"session\":{},\"request\":{},\"backtrace\":[\"blah1\",\"poop2\"]}}", json);
		}
	}
}