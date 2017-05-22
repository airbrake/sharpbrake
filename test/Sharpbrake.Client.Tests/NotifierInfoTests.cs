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

            Assert.True(notifierInfo.Name.Equals("sharpbrake"));
        }

        [Fact]
        public void Version_ShouldReturnThreeZeroZero()
        {
            var notifierInfo = new NotifierInfo();

            Assert.True(notifierInfo.Version.Equals("3.1.1"));
        }

        [Fact]
        public void Url_ShouldReturnGithubRepoUrl()
        {
            var notifierInfo = new NotifierInfo();

            Assert.True(notifierInfo.Url.Equals("https://github.com/airbrake/sharpbrake"));
        }
    }
}
