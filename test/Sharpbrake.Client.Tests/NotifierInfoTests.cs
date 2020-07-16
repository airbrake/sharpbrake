using Sharpbrake.Client.Model;
using Xunit;

namespace Sharpbrake.Client.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="NotifierInfo"/> class.
    /// </summary>
    public class NotifierInfoTests
    {
        [Fact]
        public void Name_ShouldReturnSharpbrake()
        {
            var notifierInfo = new NotifierInfo();

            Assert.Equal("sharpbrake", notifierInfo.Name);
        }

        [Fact]
        public void Version_ShouldReturnVersionSpecifiedInCsproj()
        {
            var notifierInfo = new NotifierInfo();

            Assert.Equal("5.1.0", notifierInfo.Version);
        }

        [Fact]
        public void Url_ShouldReturnGithubRepoUrl()
        {
            var notifierInfo = new NotifierInfo();

            Assert.Equal("https://github.com/airbrake/sharpbrake", notifierInfo.Url);
        }
    }
}
