using System.IO;
using System.Text;

namespace Sharpbrake.Client.Tests.Mocks
{
    /// <summary>
    /// Implementation of MemoryStream that exposes its content to <see cref="StringBuilder"/> instance.
    ///
    /// The proper usage of stream-based object involves wrapping it in "using" block that makes
    /// instance unusable outside of that block (stream becomes eligible for disposing).
    ///
    /// We have such case in the first continuation (GetRequestStreamAsync) of NotifyAsync method:
    ///
    /// <code>
    /// using (var requestStream = requestStreamTask.Result)
    /// using (var requestWriter = new StreamWriter(requestStream))
    ///     requestWriter.Write(noticeBuilder.ToJsonString());
    /// </code>
    ///
    /// In the code above "requestStream" should NOT be used anymore after "Write" operation.
    ///
    /// But we want to have the content of "requestStream" so we can make test against that content.
    /// Class is aimed to expose the content that is passed to "Write" method of underlying MemoryStream object.
    /// </summary>
    public class CapturedMemoryStream : MemoryStream
    {
        private readonly StringBuilder builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturedMemoryStream"/> class.
        /// </summary>
        /// <param name="builder">An instance of <see cref="StringBuilder"/> for capturing content to.</param>
        public CapturedMemoryStream(StringBuilder builder)
        {
            this.builder = builder;
        }

        /// <summary>
        /// Writes buffer to underlying stream. Before writing content is captured to instance of <see cref="StringBuilder"/>.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // capture content that is going to be written to underlying memory stream
            using (var writer = new StringWriter(builder))
                writer.Write(Encoding.UTF8.GetString(buffer).ToCharArray(), offset, count);

            // continue with base MemoryStream "write" logic
            base.Write(buffer, offset, count);
        }
    }
}
