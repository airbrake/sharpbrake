using System;
using System.Globalization;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Functionality for tracing AirbrakeNotifier internal behavior.
    /// </summary>
    public class InternalLogger
    {
        private static Action<string> traceAction;
        private static readonly object locker = new object();

        private readonly string traceId;

        /// <summary>
        /// Enables the internal logging.
        /// </summary>
        /// <param name="action">An action to invoke for writing diagnostic messages.
        /// <example>Use <code>msg => Debug.WriteLine(msg)</code> to write to the Debug console.</example>
        /// </param>
        public static void Enable(Action<string> action)
        {
            if (action != null)
            {
                lock (locker)
                {
                    traceAction = action;
                }
            }
            else
                throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Disables the internal logging.
        /// </summary>
        public static void Disable()
        {
            lock (locker)
            {
                traceAction = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalLogger"/> class.
        /// </summary>
        /// <param name="traceId">Id to correlate trace output from different method calls.</param>
        private InternalLogger(string traceId)
        {
            this.traceId = traceId;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InternalLogger"/> class with traceId set.
        /// </summary>
        internal static InternalLogger CreateInstance()
        {
            return new InternalLogger(GenerateId());
        }

        /// <summary>
        /// Writes formatted diagnostic message to the specified output if provided.
        /// </summary>
        internal void Trace(string format, params object[] args)
        {
            traceAction?.Invoke(string.Format(CultureInfo.InvariantCulture,
                string.Format(CultureInfo.InvariantCulture, "[{0}] ({1}) {2}", DateTime.UtcNow.ToString("o"),
                    traceId, format), args));
        }

        /// <summary>
        /// Generates a 16 character string that is quite unique to be used as Id.
        /// </summary>
        private static string GenerateId()
        {
            long i = 1;

            foreach (var b in Guid.NewGuid().ToByteArray())
                i *= b + 1;

            return string.Format(CultureInfo.InvariantCulture, "{0:x}", i - DateTime.Now.Ticks);
        }
    }
}
