using System;

namespace Subtext.TestLibrary
{
    /// <summary>
    /// This file is required in the Visual Studio 2008 project since we can't reference the real HttpSimulator as it is built against .NET 4.0.
    /// </summary>
    public class HttpSimulator : IDisposable
    {
        public HttpSimulator(string path, string physicalApplicationPath)
        {
        }


        public void Dispose()
        {
        }


        public void SetFormVariable(string key, string value)
        {
        }


        public void SetHeader(string headerKey1, string headerValue1)
        {
        }


        public void SetReferer(Uri uri)
        {
        }


        public void SimulateRequest(Uri uri)
        {
        }
    }
}