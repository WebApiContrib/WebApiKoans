using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * Lesson 4: About MediaTypeFormatters
     * 
     * The built-in HttpContent types are handy, but they can't do everything.
     * What happens when you need to serialize data? Sure, you could instantiate
     * a serializer and do the work yourself. However, HTTP allows for content
     * negotiation, so you don't always know which serializer you'll need.
     * 
     * Rather than requiring you to build your own factory, Web API provides an
     * abstraction to select the appropriate serializer, or formatter, based
     * on the specified media type. We'll delve into the specifics of content
     * negotiation in the next section. For now, we will look at MediaTypeFormatters.
     * 
     * In order to exercise the formatters, we'll use another HttpContent: ObjectContent.
     * ObjectContent provides a generic content host that accepts a value and a
     * MediaTypeFormatter and formats the value with that formatter.
     */

    // A simple person class for use in learning about formatters.
    [DataContract]
    public class Person
    {
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
    }

    [Koan(Sort = 4)]
    public static class AboutMediaTypeFormatters
    {
        [Koan]
        public static void JsonFormatter()
        {
            var person = new Person { FirstName = "Glenn", LastName = "Block" };
            var formatter = new JsonMediaTypeFormatter();
            var content = new ObjectContent<Person>(person, formatter);

            var body = content.ReadAsStringAsync().Result;

            Helpers.AssertEquality(Helpers.__, body);
        }

        [Koan]
        public static void XmlFormatter()
        {
            var person = new Person { FirstName = "Glenn", LastName = "Block" };
            var formatter = new XmlMediaTypeFormatter();
            var content = new ObjectContent<Person>(person, formatter);

            var body = content.ReadAsStringAsync().Result;

            Helpers.AssertEquality(Helpers.__, body);
        }

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
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(Helpers.__));

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
}
