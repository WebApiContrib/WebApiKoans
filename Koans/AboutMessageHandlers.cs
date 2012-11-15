using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * Lesson 7: Build APIs directly with instances of HttpMessageHandler.
     * In order to run our tests, we first need a test runner.
     */

    [Koan(Sort = 7)]
    public static class AboutMessageHandlers
    {
        [Koan]
        public static void RespondToGetRequestWithDelegatingHandler()
        {
            // Now we can create an `HttpHandler` to return a response of `"Hello, world!"`.
            // `HttpMessageHandler`s always return a `Task<'T>` from the `SendAsync` method,
            // but since we don't need the async response, we'll just use `TaskCompletionSource`.
            var handler = new DelegateHandler(request =>
            {
                var response = new HttpResponseMessage { Content = new StringContent("Hello, world!") };
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            });

            // Set up the configuration for the server.
            var config = new HttpConfiguration();

            // Add our message handler to the configuration.
            config.MessageHandlers.Add(handler);

            // Create the server and set its configuration and the message handler.
            // Note that the HttpServer ctor also takes a message handler,
            // but that handler changes how the server behaves. We'll use the
            var server = new HttpServer(config);

            // Create an `HttpClient` that will send directly to the `server`.
            var client = new HttpClient(server);

            // Now send a GET request from the client to retrieve the result.
            using (var request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/api/test"))
            {
                var response = client.SendAsync(request, Core.Cts.Token).Result;
                var body = response.Content.ReadAsStringAsync().Result;
                Helpers.AssertEquality(Helpers.__, body);
            }

            Core.Reset();
        }
    }

    // This type is defined in the [Frank](http://github.com/frank-fs/frank) library
    // and is intended to make the creation of DelegatingHandlers even simpler.
    class DelegateHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> f, HttpMessageHandler inner)
            : base(inner)
        {
            _sendAsync = (request, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                
                var response = f(request);
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            };
        }

        public DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> f)
            : base()
        {
            _sendAsync = (request, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                
                var response = f(request);
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            };
        }

        public DelegateHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> f, HttpMessageHandler inner)
            : base(inner)
        {
            _sendAsync = (request, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                
                return f(request);
            };
        }

        public DelegateHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> f)
            : base()
        {
            _sendAsync = (request, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                
                return f(request);
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }
}
