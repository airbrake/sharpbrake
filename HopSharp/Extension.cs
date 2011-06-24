using System;

namespace HopSharp
{
    public static class Extension
    {
       /// <summary>
       /// Sends the <paramref name="exception"/> to hoptoad.
       /// </summary>
       /// <param name="exception">The exception.</param>
        public static void SendToHoptoad(this Exception exception)
        {
            var client = new HoptoadClient();
            client.Send(exception);
        }
    }
}