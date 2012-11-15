using System.IO;
using System.Net.Http;
using System.Text;
using FSharpKoans.Core;
using Newtonsoft.Json.Linq;

namespace Koans
{
    /**
     * Lesson 3: Learn about HttpContent and MediaTypeFormatters
     * 
     * The body of the request and response can take many forms.
     * `System.Net.Http` defines `HttpContent` as the base for these representations.
     * `System.Net.Http` also includes several other built-in types, such as:
     * StringContent, StreamContent, and ByteArrayContent.
     */

    [Koan(Sort = 3)]
    public static class AboutContent
    {
        [Koan]
        public static void ReadingStringContent()
        {
            // Arguably the simplest of the `HttpContent` types is `StringContent`.
            // You create a new `StringContent` in the normal way you create .NET instances.
            // Note that in F# you need to use the `new` keyword because HttpContent
            // implements `IDisposable`.
            var content = new StringContent("Hello, string!");

            // `HttpContent` contains many methods and extension methods for reading its data.
            // All the read methods are asynchronous, as they are generally intended to read
            // data from a network stream. Here we will read the string content back out
            // using `ReadAsStringAsync`.
            var body = content.ReadAsStringAsync().Result;
            
            // Verify that the data we read was the same as we submitted to the `StringContent`.
            Helpers.AssertEquality(Helpers.__, body);
        }

        [Koan]
        public static void ReadingFormData()
        {
            var content = new StringContent("a[]=1&a[]=5&a[]=333", Encoding.UTF8, "application/x-www-form-urlencoded");

            //  var body = await content.ReadAsFormDataAsync();
            var body = content.ReadAsAsync<JObject>().Result;
            
            // Verify that the data we read was the same as we submitted to the `StringContent`.
            var arr = (JArray)body["a"];
            Helpers.AssertEquality(Helpers.__, arr.ToString()); 
        }

        [Koan]
        public static void UsingByteArrayContent()
        {
            // ByteArrayContent is hardly different than StringContent,
            // but it can be more convenient for lower-level operations
            // when bytes are already available (as opposed to using a Stream).
            var bytes = Encoding.ASCII.GetBytes(Helpers.__);
            var content = new ByteArrayContent(bytes); // This also takes offset and limit parameters.

            var body = content.ReadAsStringAsync().Result;

            Helpers.AssertEquality("Hello, bytes!", body);
        }

        [Koan]
        public static void UsingStreamContent()
        {
            // When you have a stream of data, StreamContent is your best bet.
            var bytes = Encoding.ASCII.GetBytes(Helpers.__);
            var stream = new MemoryStream(bytes);
            var content = new StreamContent(stream);

            var body = content.ReadAsStringAsync().Result;

            Helpers.AssertEquality("Hello, stream!", body);
        }
    }
}
