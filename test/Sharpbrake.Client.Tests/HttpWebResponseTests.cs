using System;
using Sharpbrake.Client.Impl;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="HttpWebResponse"/> class.
    /// </summary>
    public class HttpWebResponseTests
    {
        [Fact]
        public void Ctor_ShouldThrowExceptionIfResponseParamIsEmpty()
        {
            var exception = Record.Exception(() => new HttpWebResponse(null));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("response", ((ArgumentNullException)exception).ParamName);
        }
    }
}
