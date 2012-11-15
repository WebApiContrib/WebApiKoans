using System.IdentityModel.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FSharpKoans.Core;
using Thinktecture.IdentityModel.Tokens.Http;

namespace Koans
{
    [Koan(Sort = 9)]
    public static class AboutAuthentication
    {
        [Koan]
        public static void BasicAuthentication()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                config.Routes.MapHttpRoute(
                    name: "Api",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new {id = RouteParameter.Optional}
                );

                var authConfig = new AuthenticationConfiguration
                {
                    InheritHostClientIdentity = true,
                    ClaimsAuthenticationManager = FederatedAuthentication
                        .FederationConfiguration
                        .IdentityConfiguration
                        .ClaimsAuthenticationManager
                };

                // You can setup authentication against membership:
                //authConfig.AddBasicAuthentication((username, password) =>
                //    Membership.ValidateUser(username, password));
                authConfig.AddBasicAuthentication((username, password) =>
                    username == Helpers.__ && password == Helpers.__);

                config.MessageHandlers.Add(new AuthenticationHandler(authConfig));

                client.DefaultRequestHeaders.Authorization =
                    new BasicAuthenticationHeaderValue("happy", "holidays");
                using (var response = client.GetAsync("http://go.com/api/authenticationkoan").Result)
                    Helpers.AssertEquality(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }

    // This is just a placeholder to allow the koan above to run.
    public class AuthenticationKoanController : ApiController
    {
        public string Get()
        {
            return "Authenticated!";
        }
    }
}
