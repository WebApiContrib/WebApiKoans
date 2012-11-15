using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * Lesson 5: About Content Negotiation
     * 
     * As noted in the last section, Web API provides an abstraction to run
     * content negotiation based on request headers. You are free to change
     * this implementation with your own, but for now we'll stick with the
     * DefaultContentNegotiator.
     */

    [Koan(Sort = 5)]
    public static partial class AboutContentNegotiation
    {
        // Automatic content negotiation
        public static void NegotiateXml()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api");

                using (var response = client.GetAsync("http://anything/api/negotiatexml").Result)
                {
                    var body = response.Content.ReadAsAsync<Helpers.FILL_ME_IN>().Result;
                    Helpers.Assert(body.GetType() == typeof(Helpers.FILL_ME_IN));
                }
            }
        }
    }

    public class NegotiateXmlController
    {
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            var person = new Person { FirstName = "Brad", LastName = "Wilson" };
            
            // Web API includes a very handy extension method for HttpRequestMessage
            // that will automatically perform content negotiation for you.
            return request.CreateResponse(HttpStatusCode.OK, person);
        }
    }

    public static partial class AboutContentNegotiation
    {
        // Manual content negotiation
        public static void ManuallyNegotiateXml()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                TraceConfig.Register(config);
                config.Routes.MapHttpRoute("Api", "api");

                using (var response = client.GetAsync("http://anything/api/manuallynegotiatexml").Result)
                {
                    var body = response.Content.ReadAsAsync<Person>().Result;
                    Helpers.Assert(body.GetType() == typeof(Person));
                }
            }
        }
    }

    public class ManuallyNegotiateXmlController
    {
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            var config = request.GetConfiguration();

            // The IContentNegotiator instance is registered with
            // the HttpConfiguration. By default, it uses an instance
            // of DefaultContentNegotiator.
            IContentNegotiator negotiator = config.Services.GetContentNegotiator();

            // Negotiate takes the type, the request, and the formatters you
            // wish to use. By default, Web API inserts the JsonMediaTypeFormatter
            // and XmlMediaTypeFormatter, in that order.
            ContentNegotiationResult result =
                negotiator.Negotiate(typeof(Helpers.FILL_ME_IN), request, config.Formatters);

            var person = new Person { FirstName = "Ryan", LastName = "Riley" };

            // Use the ContentNegotiationResult with an ObjectContent to format the object.
            var content = new ObjectContent<Person>(person, result.Formatter, result.MediaType);

            return new HttpResponseMessage { Content = content };
        }
    }
}
