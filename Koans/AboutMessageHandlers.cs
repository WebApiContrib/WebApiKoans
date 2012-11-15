using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                new HttpResponseMessage { Content = new StringContent("Hello, world!") });

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
        // What happens when you chain multiple handlers together?

        [Koan]
        public static void HandlingHeadRequests()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api");

                // Add our message handlers to the configuration.
                // Message handlers should be added "outside -> in".
                config.MessageHandlers.Add(new HeadMessageHandler());
                config.MessageHandlers.Add(new UriFormatExtensionHandler(new UriExtensionMappings()));
                config.MessageHandlers.Add(new DelegateHandler(request =>
                    new HttpResponseMessage { Content = new ObjectContent<string>("Hello, world!", config.Formatters.XmlFormatter) }));

                using (var request = new HttpRequestMessage(HttpMethod.Head, "http://go.com/api/multiplehandlers.xml"))
                using (var response = client.SendAsync(request).Result)
                    Helpers.AssertEquality(typeof(Helpers.FILL_ME_IN), response.Content.GetType());
            }
        }

        [Koan]
        public static void HandlingUriExtensionRequests()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api");

                // Add our message handlers to the configuration.
                // Message handlers should be added "outside -> in".
                config.MessageHandlers.Add(new HeadMessageHandler());
                config.MessageHandlers.Add(new UriFormatExtensionHandler(new UriExtensionMappings()));
                config.MessageHandlers.Add(new DelegateHandler(request =>
                    new HttpResponseMessage { Content = new ObjectContent<string>("Hello, world!", config.Formatters.XmlFormatter) }));

                using (var response = client.GetAsync("http://go.com/api/multiplehandlers.xml").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
        }

        [Koan]
        public static void PassingThroughAllHandlers()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api");

                // Add our message handlers to the configuration.
                // Message handlers should be added "outside -> in".
                config.MessageHandlers.Add(new HeadMessageHandler());
                config.MessageHandlers.Add(new UriFormatExtensionHandler(new UriExtensionMappings()));
                config.MessageHandlers.Add(new DelegateHandler(request =>
                    new HttpResponseMessage { Content = new ObjectContent<string>("Hello, world!", config.Formatters.JsonFormatter) }));

                using (var response = client.GetAsync("http://go.com/api/multiplehandlers").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
        }
    }

    public class HeadMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Head)
            {
                request.Method = HttpMethod.Get;
                return base.SendAsync(request, cancellationToken)
                    .ContinueWith<HttpResponseMessage>(task =>
                    {
                        var response = task.Result;
                        response.RequestMessage.Method = HttpMethod.Head;
                        response.Content = new HeadContent(response.Content);
                        return task.Result;
                    });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    internal class HeadContent : HttpContent
    {
        public HeadContent(HttpContent content)
        {
            CopyHeaders(content.Headers, Headers);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }


        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        private static void CopyHeaders(HttpContentHeaders fromHeaders, HttpContentHeaders toHeaders)
        {

            foreach (KeyValuePair<string, IEnumerable<string>> header in fromHeaders)
            {
                toHeaders.Add(header.Key, header.Value);
            }
        }
    }

    public class UriFormatExtensionHandler : DelegatingHandler
    {
        private static readonly Dictionary<string, MediaTypeWithQualityHeaderValue> extensionMappings = new Dictionary<string, MediaTypeWithQualityHeaderValue>();

        public UriFormatExtensionHandler(IEnumerable<UriFormatExtensionMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                extensionMappings[mapping.Extension] = mapping.MediaType;
            }
        }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var segments = request.RequestUri.Segments;
            var lastSegment = segments.LastOrDefault();
            MediaTypeWithQualityHeaderValue mediaType;
            var found = extensionMappings.TryGetValue(lastSegment, out mediaType);
            
            if (found)
            {
                var newUri = request.RequestUri.OriginalString.Replace("/" + lastSegment, "");
                request.RequestUri = new Uri(newUri, UriKind.Absolute);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(mediaType);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class UriFormatExtensionMapping
    {
        public string Extension { get; set; }
        public MediaTypeWithQualityHeaderValue MediaType { get; set; }
    }

    public class UriExtensionMappings : List<UriFormatExtensionMapping>
    {
        public UriExtensionMappings()
        {
            this.AddMapping("xml", "application/xml");
            this.AddMapping("json", "application/json");
            this.AddMapping("proto", "application/x-protobuf");
            this.AddMapping("png", "image/png");
            this.AddMapping("jpg", "image/jpg");
        }
    }

    public static class UriFormatExtensionMappingExtensions
    {
        public static void AddMapping(this IList<UriFormatExtensionMapping> mappings, string extension, string mediaType)
        {
            mappings.Add(new UriFormatExtensionMapping { Extension = extension, MediaType = new MediaTypeWithQualityHeaderValue(mediaType) });
        }
    }
}
