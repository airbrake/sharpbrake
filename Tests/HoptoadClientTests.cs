using System;
using System.Threading;

using HopSharp;
using HopSharp.Serialization;

using NUnit.Framework;

namespace Tests
{
   [TestFixture]
   public class HoptoadClientTests
   {
      #region Setup/Teardown

      [SetUp]
      public void SetUp()
      {
         this.client = new HoptoadClient();
      }

      #endregion

      private HoptoadClient client;


      [Test]
      public void Send_EndRequestEventIsInvoked_And_ResponseOnlyContainsApiError()
      {
         bool requestEndInvoked = false;
         HoptoadResponseError[] errors = null;
         int i = 0;

         this.client.RequestEnd += (sender, e) =>
         {
            requestEndInvoked = true;
            errors = e.Response.Errors;
         };

         var configuration = new HoptoadConfiguration
         {
            ApiKey = Guid.NewGuid().ToString("N"),
            EnvironmentName = "test",
         };

         var builder = new HoptoadNoticeBuilder(configuration);

         HoptoadNotice notice = builder.Notice(new Exception("Test"));

         notice.Request = new HoptoadRequest("http://example.com", "Test")
         {
            Params = new[]
            {
               new HoptoadVar("TestKey", "TestValue")
            }
         };

         this.client.Send(notice);

         while (!requestEndInvoked)
         {
            // Sleep for maximum 5 seconds to wait for the request to end. Can probably be done more elegantly.
            if (i++ == 50)
               break;

            Thread.Sleep(100);
         }

         Assert.That(requestEndInvoked, Is.True);
         Assert.That(errors, Is.Not.Null);
         Assert.That(errors, Has.Length.EqualTo(1));
      }
   }
}