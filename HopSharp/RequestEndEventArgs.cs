using System;
using System.Net;

using HopSharp.Serialization;

namespace HopSharp
{
   /// <summary>
   /// The event arguments passed to <see cref="RequestEndEventHandler"/>.
   /// </summary>
   [Serializable]
   public class RequestEndEventArgs : EventArgs
   {
      private readonly WebRequest request;
      private readonly HoptoadResponse response;


      /// <summary>
      /// Initializes a new instance of the <see cref="RequestEndEventArgs"/> class.
      /// </summary>
      /// <param name="request">The request.</param>
      /// <param name="response">The response.</param>
      /// <param name="content">The body of the response.</param>
      public RequestEndEventArgs(WebRequest request, WebResponse response, string content)
      {
         this.request = request;
         this.response = new HoptoadResponse(response, content);
      }


      /// <summary>
      /// Gets the request.
      /// </summary>
      public WebRequest Request
      {
         get { return this.request; }
      }

      /// <summary>
      /// Gets the response.
      /// </summary>
      public HoptoadResponse Response
      {
         get { return this.response; }
      }
   }
}