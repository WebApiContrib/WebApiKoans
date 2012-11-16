using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FSharpKoans.Core;

namespace Koans
{
    /**
     * Lesson 1: About Basics
     * 
     * Unlike many koans programs, Web API requires a bit of setup
     * in order to start learning how to use it. Each of the concepts
     * below will be inspected in further detail as you progress.
     */

    [Koan(Sort = 1)]
    public static class AboutBasics
    {
        [Koan]
        public static void UsingHttpClientToGetResponse()
        {
            // Send a GET message. The Web API API is asynchronous.
            // You should avoid calling .Result, but for testing,
            // this is desired. Fortunately, C# 5 helps deal with async.
            using (var response = WebApiKoans.Client.GetAsync("http://go.com/").Result)
                Helpers.AssertEquality(Helpers.__, response.StatusCode);
        }

        [Koan]
        public static void RetrieveResponseWithStatusCodeOk()
        {
            // Write code to allow the client to retrieve an HttpResponseMessage with a status code of 200 OK.
            using (var response = WebApiKoans.Client.GetAsync("http://go.com/api").Result)
                Helpers.AssertEquality(HttpStatusCode.OK, response.StatusCode);
        }

        [Koan]
        public static void RetrieveResponseWithContentHelloWorld()
        {
            // Write code to allow the client to retrieve an HttpResponseMessage with a status code of 200 OK
            // and a message body containing the string "Hello, world!".
            using (var response = WebApiKoans.Client.GetAsync("http://go.com/api").Result)
            {
                var body = response.Content.ReadAsStringAsync().Result;

                Helpers.AssertEquality(HttpStatusCode.OK, response.StatusCode);
                Helpers.AssertEquality("Hello, world!", body);
            }
        }
    }
}
