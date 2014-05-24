using System;
using Common.Logging;
using NUnit.Framework;

namespace SharpBrake.Tests
{
    [TestFixture]
    public class BacktraceBuilderTests
    {
        [Test]
        public void When_building_a_backtrace_then_the_message_contains_messages_from_the_topmost_exception_and_all_inner_exceptions()
        {
            // Arrange
            var log = new Moq.Mock<ILog>();

            var exception = GetDeepException();

            var builder = new BacktraceBuilder(log.Object);

            // Act
            var response = builder.Build(exception);

            // Assert
            Assert.That(response.Message, Is.EqualTo("Exception: last thing wrong | ApplicationException: level two message | DivideByZeroException: Attempted to divide by zero."));
        }

        [Test]
        public void When_building_a_backtrace_then_the_trace_contains_the_full_stack_trace()
        {
            // Arrange
            var log = new Moq.Mock<ILog>();

            var exception = GetDeepException();

            var builder = new BacktraceBuilder(log.Object);

            // Act
            var response = builder.Build(exception);

            // Assert
            Assert.That(response.Trace, Has.Count.EqualTo(5));
            Assert.That(response.Trace[0].Method, Is.EqualTo("GetException"));
        }

        [Test]
        public void When_building_a_backtrace_then_the_catching_method_is_set()
        {
            // Arrange
            var log = new Moq.Mock<ILog>();

            var exception = GetDeepException();

            var builder = new BacktraceBuilder(log.Object);

            // Act
            var response = builder.Build(exception);

            // Assert
            Assert.That(response.CatchingMethod, Is.Not.Null);
        } 

        private Exception GetDeepException()
        {
            try
            {
                InnerException1();

                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }

        }

        private static void InnerException1()
        {
            try
            {
                InnerException2();
            }
            catch (Exception secondException)
            {
                var thirdException = new Exception("last thing wrong", secondException);
                throw thirdException;
            }
        }

        private static void InnerException2()
        {
            try
            {
                var one = 1;
                var zero = 0;

                var result = one/zero;
            }
            catch (Exception firstException)
            {
                var wrappedException = new ApplicationException("level two message", firstException);
                throw wrappedException;
            }
        }
    }
}