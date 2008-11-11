using System;

namespace HopSharp
{
	public static class Extension
	{
		public static void SentToHoptoad(this Exception exception)
		{
			HoptoadClient client = new HoptoadClient();
			client.Send(exception);
		}
	}
}