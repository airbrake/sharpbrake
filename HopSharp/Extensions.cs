using System;

namespace HopSharp
{
    public static class Extensions
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

       internal static TResult TryGet<TObject, TResult>(this TObject instance, Func<TObject, TResult> getter)
       {
          try
          {
             return getter.Invoke(instance);
          }
          catch (Exception)
          {
             return default(TResult);
          }
       }
    }
}