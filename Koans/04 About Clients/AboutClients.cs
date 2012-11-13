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
    /* Lesson 4: Consume APIs with HttpClient

    We've already used a `client` to pass a request and receive a response in order to learn
    about controllers and message handlers. We used the simplest possible approach to get
    us through the basics.

    You consume web apis with the `System.Net.Http.HttpClient`. This type provides methods
    for retrieving data using the standard HTTP methods, as well as a general `SendAsync`
    method that allows you to supply a complete `HttpRequestMessage`. In this lesson, we'll
    look at how you can consume apis and leverage the power of
    [F# Active Patterns](http://msdn.microsoft.com/en-us/library/dd233248.aspx).
    */

    [Koan(Sort = 4)]
    public static class AboutClients
    {
        // First things first: HttpClient is intended to be kept around for the life of your application.
        // Starting up a client in every use is very expensive and can lead to bugs.
        static readonly HttpClient client = new HttpClient(Core.Server);

        // We'll use message handlers to create koan-specific request handlers for each of the following koans.
        // This option is the best for preventing collisions with the existing controller types, and it lets us
        // specifically set the expectation within each koan. We'll define a helper here to return an instance
        // of the `AsyncHandler` we saw in AboutMessageHandlers.
        static void AddHandler(Func<HttpRequestMessage, HttpResponseMessage> f)
        {
            var handler = new DelegateHandler(f);
            Core.Config.MessageHandlers.Add(handler);
            Core.Config.Routes.MapHttpRoute("Api", "api");
        }

        static void AddHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> f)
        {
            var handler = new DelegateHandler(f);
            Core.Config.MessageHandlers.Add(handler);
            Core.Config.Routes.MapHttpRoute("Api", "api");
        }

        [Koan]
        public static async void SendRequestAndGetResponse()
        {
            AddHandler(request => new HttpResponseMessage { Content = new StringContent("Hello, client!") });
            using (var request = new HttpRequestMessage(HttpMethod.Get, "http://anything/api"))
            using (var response = await client.SendAsync(request))
            {
                var result = await response.Content.ReadAsStringAsync();
                Helpers.AssertEquality(Helpers.__, result);
            }

            Core.Reset();
        }

        [Koan]
        public static async void GetStringAsynchronously()
        {
            AddHandler(request => new HttpResponseMessage { Content = new StringContent("Hello, client!") });
            // Rather than creating a request ourselves, we can use the `GetAsync` method.
            using (var response = await client.GetAsync("http://anything/api"))
            {
                var result = await response.Content.ReadAsStringAsync();
                Helpers.AssertEquality(Helpers.__, result);
            }

            Core.Reset();
        }

        // TODO: Other methods, such as PostAsync, etc.
    }
}
