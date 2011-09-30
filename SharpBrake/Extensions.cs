using System;
using System.Collections.Generic;
using System.Xml;

using SharpBrake.Serialization;

namespace SharpBrake
{
    /// <summary>
    /// Contains the extension method to send exceptions to Airbrake.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sends the <paramref name="exception"/> to Airbrake.
        /// </summary>
        /// <param name="exception">The exception to send to Airbrake.</param>
        public static void SendToAirbrake(this Exception exception)
        {
            var client = new AirbrakeClient();
            client.Send(exception);
        }


        /// <summary>
        /// Builds the errors from the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// An <see cref="IEnumerable{AirbrakeResponseError}"/>.
        /// </returns>
        internal static IEnumerable<AirbrakeResponseError> BuildErrors(this XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.LocalName == "error")
                            yield return new AirbrakeResponseError(reader.ReadElementContentAsString());
                        break;
                }
            }
        }


        /// <summary>
        /// Builds the notice from the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// A new instance of <see cref="AirbrakeResponseNotice"/>.
        /// </returns>
        internal static AirbrakeResponseNotice BuildNotice(this XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            int id = 0;
            int errorId = 0;
            string url = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "id":
                                id = reader.ReadElementContentAsInt();
                                break;

                            case "error-id":
                                errorId = reader.ReadElementContentAsInt();
                                break;

                            case "url":
                                url = reader.ReadElementContentAsString();
                                break;
                        }
                        break;
                }
            }

            return new AirbrakeResponseNotice
            {
                Id = id,
                ErrorId = errorId,
                Url = url,
            };
        }


        /// <summary>
        /// Tries to invoke the <paramref name="getter"/>. Returns <c>default(TResult)</c>
        /// if the invocation fails.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="getter">The getter.</param>
        /// <returns>
        /// The value returned from <paramref name="getter"/> or <c>default(TResult)</c>
        /// if the invocation fails.
        /// </returns>
        internal static TResult TryGet<TObject, TResult>(this TObject instance, Func<TObject, TResult> getter)
        {
            try
            {
                return getter.Invoke(instance);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }
    }
}