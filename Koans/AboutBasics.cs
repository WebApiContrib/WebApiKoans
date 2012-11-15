using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * About Basics
     * 
     * Unlike many koans programs, Web API requires a bit of setup
     * in order to start learning how to use it. Each of the concepts
     * below will be inspected in further detail as you progress.
     */

    [Koan(Sort = 1)]
    public static class AboutBasics
    {
        [Koan]
        public static void NotFound()
        {
            // Create a configuration. Web API uses a code-based configuration
            // rather than the XML-based configuration familiar to WCF programmers.
            using (var config = new HttpConfiguration())
            // Create an HttpServer. HttpServer is the core for all other server
            // implementations. We'll discuss this in more detail later.
            using (var server = new HttpServer(config))
            // Create an HttpClient. HttpClient can be used without the server
            // to hit the public web, but passing in a server will allow you
            // direct access.
            using (var client = new HttpClient(server))
            {
                // Send a GET message. The Web API API is asynchronous.
                // You should avoid calling .Result, but for testing,
                // this is desired. Fortunately, C# 5 helps deal with async.
                var response = client.GetAsync(Helpers.__).Result;

                Helpers.AssertEquality(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Koan]
        public static void FirstRoute()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                // In order to get anything other than a 404 response,
                // you first must add a route on the configuration.
                // Note how similar routing is to routing in MVC.
                // We'll explore routing further a little later.
                config.Routes.MapHttpRoute("Default", "api");

                var response = client.GetAsync("http://go.com/api").Result;

                Helpers.AssertEquality(Helpers.__, response.StatusCode);
            }
        }

        [Koan]
        public static void Ok()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                config.Routes.MapHttpRoute("Default", "api");

                // A route alone is insufficient to get past the 404.
                // You also need some form of handler. ASP.NET has IHttpHandler
                // and IHttpModule, but Web API uses ApiControllers and
                // HttpMessageHandlers. We will cover each in detail later,
                // but for now we will add an HttpMessageHandler for simplicity.
                config.MessageHandlers.Add(new OkHandler());

                var response = client.GetAsync("http://go.com/api").Result;

                Helpers.AssertEquality(Helpers.__, response.StatusCode);
            }
        }

        class OkHandler : DelegatingHandler
        {
            /**
             * Message Handlers are very similar to a delegate taking
             * an HttpRequestMessage and returning a Task returning
             * an HttpResponseMessage. DelegatingHandlers are an implementation
             * that allow chaining. Once again, we'll take a look at this
             * in more detail later.
             */
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage();

                // The best way to return a synchronous result is by using
                // a TaskCompletionSource. Look up more information about
                // Tasks at http://bradwilson.typepad.com/blog/2012/04/tpl-and-servers-pt1.html
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }
        }
    }
}
