namespace SharpBrake.Serialization
{
   public class AirbrakeResponseError
   {
      private readonly string message;


      public AirbrakeResponseError(string message)
      {
         this.message = message;
      }


      public string Message
      {
         get { return this.message; }
      }
   }
}