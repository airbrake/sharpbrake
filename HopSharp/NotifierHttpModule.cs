using System;
using System.Web;

namespace HopSharp
{
    public class NotifierHttpModule : IHttpModule
    {
        #region IHttpModule Members

        public void Init(HttpApplication context)
        {
            context.Error += ContextError;
        }

        public void Dispose()
        {
        }

        #endregion

        private static void ContextError(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;

            Exception exception = application.Server.GetLastError();
            if (!(exception is HttpException) || ((HttpException) exception).GetHttpCode() != 404)
                exception.SendToHoptoad();
        }
    }
}