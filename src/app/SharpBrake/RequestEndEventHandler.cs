using System;

namespace SharpBrake
{
    /// <summary>
    /// Occurs when a request ends.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RequestEndEventArgs"/> instance containing the event data.</param>
    [Serializable]
    public delegate void RequestEndEventHandler(object sender, RequestEndEventArgs e);
}