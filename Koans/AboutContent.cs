using System.Net.Http;
using System.Text;
using FSharpKoans.Core;
using Newtonsoft.Json.Linq;

namespace Koans
{
    /* Lesson 1: Learn about HttpContent and MediaTypeFormatters

    The body of the request and response can take many forms.
    `System.Net.Http` defines `HttpContent` as the base for these representations.
    `System.Net.Http` also includes several other built-in types,
    as well as an `ObjectContent` that will serialize any .NET object
    based on the provided `MediaTypeFormatter`.
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

            Core.Reset();
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

            Core.Reset();
        }
    }
}
