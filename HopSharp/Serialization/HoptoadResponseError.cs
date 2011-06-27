namespace HopSharp.Serialization
{
   public class HoptoadResponseError
   {
      private readonly string message;


      public HoptoadResponseError(string message)
      {
         this.message = message;
      }


      public string Message
      {
         get { return this.message; }
      }
   }
}