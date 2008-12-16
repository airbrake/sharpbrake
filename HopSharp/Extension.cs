using System;

namespace HopSharp
{
	public static class Extension
	{
		public static void SendToHoptoad(this Exception exception)
		{
			HoptoadClient client = new HoptoadClient();
			client.Send(exception);
		}
	}
}