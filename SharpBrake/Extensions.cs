using System;

namespace SharpBrake
{
    public static class Extensions
    {
       /// <summary>
       /// Sends the <paramref name="exception"/> to Airbrake.
       /// </summary>
       /// <param name="exception">The exception.</param>
        public static void SendToAirbrake(this Exception exception)
        {
            var client = new AirbrakeClient();
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