using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * Lesson 2: Build APIs with ApiControllers
     * 
     * The standard api for building applications with Web API
     * is through the ApiController. This implements the IHttpController
     * interface and provides a number of additional helpers such
     * as model binding and formatting.
     */

    // To use an ApiController, you need to define a subclass.
    // We will extend and reuse this type throughout this set of koans.
    class TestController : ApiController
    {
        // Inside your controller, define the actions you want to support.
        // By convention, actions matching HTTP method names will be
        // matched the requests to that method. If you don't follow this
        // convention, you can use the AcceptVerbsAttribute or any of
        // the shortcut versions, such as HttpGet, HttpPost, etc.
        // Here, we will just define a Get method.
        public string Get()
        {
            return Helpers.__;
        }

        // We'll amend our existing controller with a Post method
        // that will return whatever is POSTed. 
        public string Post()
        {
            return Helpers.__;
        }
    }

    class TestFixedController : ApiController
    {
        // Contrary to what you might have expected, copying a request body is not quite as simple
        // as it may at first appear. The previous step can be successfully completed by returning
        // x.Request.Content.ReadAsStreamAsync() or x.Request.Content.ReadAsStringAsync().
        // However, if you add a test to check the content type of the request and the response, you
        // will find the request submitted text/plain, and the response submitted application/json.
        // Those certainly look the same, but they are not. Also, returning x.Request.Content
        // will throw an ObjectDisposedException as the request content is disposed before the
        // response completes. You have to copy the content to a new content.
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request)
        {
            Console.WriteLine("Received {0}", request);
            var stream = new MemoryStream();
            await request.Content.CopyToAsync(stream);

            // Don't miss the importance of resetting the position on the stream.
            // If you don't know what this does, try commenting it out.
            stream.Position = 0;
            var content = new StreamContent(stream);
            foreach (var header in request.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }
    }

    [Koan(Sort = 2)]
    public static class AboutControllers
    {
        [Koan]
        public static void SimpleHelloWorldController()
        {
            // Controllers can't be found without routing. Here, we use a very generic
            // uri template and map it using the `MapHttpRoute` extension method.
            // If you have worked with MVC, this will be very familiar. In a larger
            // project, this would most likely be defined in your Global.asax file.
            Core.Config.Routes.MapHttpRoute("Api", "api/{controller}");

            // Now send a GET request from the client to retrieve the result.
            using (var request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/api/test"))
            using (var response = Core.Client.SendAsync(request, Core.Cts.Token).Result)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                Helpers.AssertEquality("\"Hello, ApiController!\"", body);
            }

            Core.Reset();
        }

        public static void CreateAnEchoController()
        {
            Core.Config.Routes.MapHttpRoute("Api", "api/{controller}");

            // Now send a POST request from the client to retrieve the result.
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://example.org/api/test") { Content = new StringContent("Hello, ApiController!") })
            using (var response = Core.Client.SendAsync(request, Core.Cts.Token).Result)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                Helpers.AssertEquality("\"Hello, ApiController!\"", body);
                Helpers.AssertEquality(Helpers.__, response.Content.Headers.ContentType.MediaType);
            }

            Core.Reset();
        }

        public static void AreYouSureYouMadeAnEchoController()
        {
            Core.Config.Routes.MapHttpRoute("Api", "api/{controller}");

            // Now send a POST request from the client to retrieve the result.
            using (var request = new HttpRequestMessage(HttpMethod.Post, "http://example.org/api/testfixed") { Content = new StringContent("Hello, ApiController!") })
            using (var response = Core.Client.SendAsync(request, Core.Cts.Token).Result)
            {
                var body = response.Content.ReadAsStringAsync().Result;
                Helpers.AssertEquality(Helpers.__, body);
                // Note that this passes for a test of the request's content type as we copied the value above.
                Helpers.AssertEquality(Helpers.__, response.Content.Headers.ContentType.MediaType);
            }

            Core.Reset();
        }
    }
}
