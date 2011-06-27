using System;

namespace HopSharp
{
   /// <summary>
   /// Occurs when a request ends.
   /// </summary>
   /// <param name="sender">The sender.</param>
   /// <param name="e">The <see cref="HopSharp.RequestEndEventArgs"/> instance containing the event data.</param>
   [Serializable]
   public delegate void RequestEndEventHandler(object sender, RequestEndEventArgs e);
}