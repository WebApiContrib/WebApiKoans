using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    [Koan(Sort = 4)]
    public static partial class AboutMediaTypeFormatters
    {
        // JSON formatter

        // XML formatter

        // FormUrlEncoded formatter

        // Custom Formatter
        [Koan]
        public static void PlainTextFormatter()
        {
            var value = "This is a text/plain string.";
            var formatter = new PlainTextBufferedFormatter();
            var content = new ObjectContent<string>(value, formatter);

            var result = content.ReadAsStringAsync().Result;

            Helpers.AssertEquality(Helpers.__, result);
            Helpers.AssertEquality("text/plain", content.Headers.ContentType.MediaType);
            Helpers.AssertEquality(Helpers.__, content.Headers.ContentLength);
        }
    }

    /// <summary>
    /// This sample formatter illustrates how to use the BufferedMediaTypeFormatter base class for
    /// writing a MediaTypeFormatter. The BufferedMediaTypeFormatter is useful when you either want
    /// to aggregate many small reads or writes or when you are writing synchronously to the underlying
    /// stream.
    /// </summary>
    public class PlainTextBufferedFormatter : BufferedMediaTypeFormatter
    {
        public PlainTextBufferedFormatter()
        {
            // Set supported media type for this media type formatter
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(Helpers.__);

            // Set default supported character encodings for this media type formatter (UTF-8 and UTF-16)
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string);
        }

        public override object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            // Get the preferred character encoding based on information in the request
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            // Create a stream reader and read the content synchronously
            using (StreamReader reader = new StreamReader(readStream, effectiveEncoding))
                return reader.ReadToEnd();
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            // Get the preferred character encoding based on information in the request and what we support
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            // Create a stream writer and write the content synchronously
            using (StreamWriter writer = new StreamWriter(writeStream, effectiveEncoding))
                writer.Write(value);
        }
    }

    public static partial class AboutMediaTypeFormatters
    {
        // Adding / Removing formatters
    }

    // Razor?
}
