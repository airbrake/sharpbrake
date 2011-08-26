using System;
using System.Threading;


using NUnit.Framework;

using SharpBrake;
using SharpBrake.Serialization;

namespace Tests
{
   [TestFixture]
   public class AirbrakeClientTests
   {
      #region Setup/Teardown

      [SetUp]
      public void SetUp()
      {
         this.client = new AirbrakeClient();
      }

      #endregion

      private AirbrakeClient client;


      [Test]
      public void Send_EndRequestEventIsInvoked_And_ResponseOnlyContainsApiError()
      {
         bool requestEndInvoked = false;
         AirbrakeResponseError[] errors = null;
         int i = 0;

         this.client.RequestEnd += (sender, e) =>
         {
            requestEndInvoked = true;
            errors = e.Response.Errors;
         };

         var configuration = new AirbrakeConfiguration
         {
            ApiKey = Guid.NewGuid().ToString("N"),
            EnvironmentName = "test",
         };

         var builder = new AirbrakeNoticeBuilder(configuration);

         AirbrakeNotice notice = builder.Notice(new Exception("Test"));

         notice.Request = new AirbrakeRequest("http://example.com", "Test")
         {
            Params = new[]
            {
               new AirbrakeVar("TestKey", "TestValue")
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