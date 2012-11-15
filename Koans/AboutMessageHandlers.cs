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
    public static partial class AboutMessageHandlers
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

            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);

                // Add our message handler to the configuration.
                config.MessageHandlers.Add(handler);
                config.Routes.MapHttpRoute("Api", "api");

                using (var response = client.GetAsync("http://go.com/api").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
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

    public static partial class AboutMessageHandlers
    {
        // Multiple handlers
    }
}
