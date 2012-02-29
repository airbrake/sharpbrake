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


        public HttpSimulator SetFormVariable(string key, string value)
        {
            return this;
        }


        public HttpSimulator SetHeader(string headerKey1, string headerValue1)
        {
            return this;
        }


        public HttpSimulator SetReferer(Uri uri)
        {
            return this;
        }


        public HttpSimulator SimulateRequest(Uri uri)
        {
            return this;
        }
    }
}