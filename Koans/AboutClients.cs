using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /* Lesson 8: Consume APIs with HttpClient

    We've already used a `client` to pass a request and receive a response in order to learn
    about controllers and message handlers. We used the simplest possible approach to get
    us through the basics.

    You consume web apis with the `System.Net.Http.HttpClient`. This type provides methods
    for retrieving data using the standard HTTP methods, as well as a general `SendAsync`
    method that allows you to supply a complete `HttpRequestMessage`. In this lesson, we'll
    look at how you can consume apis and leverage the power of
    [F# Active Patterns](http://msdn.microsoft.com/en-us/library/dd233248.aspx).
    */

    [Koan(Sort = 8)]
    public static class AboutClients
    {
        // First things first: HttpClient is intended to be kept around for the life
        // of your application. Starting up a client in every use is very expensive
        // and can lead to bugs when running asynchronously.
        // That said, we will ignore this for the following koans.

        [Koan]
        public static void GetStringAsynchronously()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                AddHandler(config, request => new HttpResponseMessage { Content = new StringContent("Hello, client!") });

                // As we've done all along, e can use the `GetAsync` method.
                // HttpClient has many, similar methods for other HTTP methods.
                using (var response = client.GetAsync("http://anything/api").Result)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, result);
                }
            }
        }

        [Koan]
        public static void SendRequestAndGetResponse()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                AddHandler(config, request => new HttpResponseMessage { Content = new StringContent("Hello, client!") });

                // Of course, we can also create our own request message.
                using (var request = new HttpRequestMessage(HttpMethod.Get, "http://anything/api"))
                {
                    // Creating your own request gives you finer grained control over what is sent.
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

                    // To send a request object, use SendAsync.
                    using (var response = client.SendAsync(request).Result)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        Helpers.AssertEquality(Helpers.__, result);
                    }
                }
            }
        }

        [Koan]
        public static void SendRequestAndGetResponseWithCommonHeaders()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                AddHandler(config, request => new HttpResponseMessage { Content = new StringContent("Hello, client!") });

                // For common headers across all requests, you can simply modify
                // the client's DefaultRequestHeaders and still use the GetAsync
                // or similar method.
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                using (var response = client.GetAsync("http://anything/api").Result)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, result);
                }
            }
        }

        // TODO: Other methods, such as PostAsync, etc.

        // We'll use message handlers to create koan-specific request handlers for each of the following koans.
        // This option is the best for preventing collisions with the existing controller types, and it lets us
        // specifically set the expectation within each koan. We'll define a helper here to return an instance
        // of the `AsyncHandler` we saw in AboutMessageHandlers.
        static void AddHandler(HttpConfiguration config, Func<HttpRequestMessage, HttpResponseMessage> f)
        {
            var handler = new DelegateHandler(f);
            config.MessageHandlers.Add(handler);
            config.Routes.MapHttpRoute("Api", "api");
        }

        static void AddHandler(HttpConfiguration config, Func<HttpRequestMessage, Task<HttpResponseMessage>> f)
        {
            var handler = new DelegateHandler(f);
            config.MessageHandlers.Add(handler);
            config.Routes.MapHttpRoute("Api", "api");
        }
    }
}
