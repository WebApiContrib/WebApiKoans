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

    [Koan(Sort = 2)]
    public static partial class AboutControllers
    {
        [Koan]
        public static void SimpleHelloWorldController()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);

                // Controllers are identified through the routing mechanism.
                // Web API includes a convenient extension method to the
                // HttpRouteCollection type, which is exposed as the
                // HttpConfiguration.Routes property. We will use the default
                // uri template and map it using the `MapHttpRoute` extension method.
                // If you have worked with MVC, this will be very familiar.
                // In a larger project, this would most likely be defined in
                // your Global.asax or WebApiConfig.cs file.
                config.Routes.MapHttpRoute(
                    name: "Api",
                    routeTemplate: "api/{controller}"
                );

                using (var response = client.GetAsync("http://example.org/api/helloworld").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality("\"Hello, ApiController!\"", body);
                }
            }
        }
    }

    // To use an ApiController, you need to define a subclass.
    // ApiController implements the IHttpController interface
    // and implements a number of helpful functionality familiar
    // to MVC developers such as ActionFilters, Model Binding,
    // content negotiation, and more.
    public class HelloWorldController : ApiController
    {
        // Inside your controller, define the actions you want to support.
        // By convention, actions matching HTTP method names will be
        // matched the requests to that method. If you don't follow this
        // convention, you can use the AcceptVerbsAttribute or any of
        // the instances such as HttpGet, HttpPost, etc.
        // Here, we will just define a Get method.
        public string Get()
        {
            return Helpers.__;
        }
    }

    public static partial class AboutControllers
    {
        [Koan]
        public static void CreateAnEchoController()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api/{controller}");

                // Now send a POST request from the client to retrieve the result.
                var uri = "http://example.org/api/echo";
                var content = new StringContent("Hello, ApiController!");
                using (var response = client.PostAsync(uri, content).Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality("\"Hello, ApiController!\"", body);
                    Helpers.AssertEquality(Helpers.__, response.Content.Headers.ContentType.MediaType);
                }
            }
        }
    }

    // We'll amend our existing controller with a Post method
    // that will return whatever is POSTed. 
    public class EchoController : ApiController
    {
        public string Post()
        {
            return Helpers.__;
        }
    }

    public static partial class AboutControllers
    {
        [Koan]
        public static void AreYouSureYouMadeAnEchoController()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api/{controller}");

                var uri = "http://example.org/api/testfixed";
                var content = new StringContent("Hello, ApiController!");
                using (var response = client.PostAsync(uri, content).Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                    // Note that this passes for a test of the request's content type as we copied the value above.
                    Helpers.AssertEquality(Helpers.__, response.Content.Headers.ContentType.MediaType);
                }
            }
        }
    }

    public class TestFixedController : ApiController
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
            // NOTE that here we model bind the request.
            // The ApiController already has a Request property,
            // so you don't need to do this.

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
}
