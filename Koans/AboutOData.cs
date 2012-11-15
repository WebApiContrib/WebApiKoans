using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Query;
using FSharpKoans.Core;

namespace Koans
{
    [Koan(Sort = 10)]
    public static partial class AboutOData
    {
        // This sample customer controller demonstrates how to create
        // an action which supports OData style queries using the
        // [Queryable] attribute.

        [Koan]
        public static void ExposeQueryable()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                config.Routes.MapHttpRoute(
                    name: "Api",
                    routeTemplate: "api/{controller}"
                );

                using (var response = client.GetAsync("http://example.org/api/customer?$top=2&$orderby=name").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
        }
    }

    public class CustomerController : ApiController
    {
        [Queryable]
        public IEnumerable<Customer> Get()
        {
            return CustomerList;
        }

        private static readonly List<Customer> CustomerList = new List<Customer>
        {  
            new Customer
            { 
                Id = 11, Name = "Lowest", BirthTime = new DateTime(2001, 1, 1),
                Orders = new Order[] 
                { 
                    new Order { Id = 0 , Quantity = 10 },  
                    new Order { Id = 1 , Quantity = 50 } 
                }
            }, 
            new Customer
            { 
                Id = 33, Name = "Highest", BirthTime = new DateTime(2002, 2, 2),
                Orders = new Order[] 
                { 
                    new Order { Id = 2 , Quantity = 10 }, 
                    new Order { Id = 3 , Quantity = 5 } 
                }
            }, 
            new Customer { Id = 22, Name = "Middle", BirthTime = new DateTime(2003, 3, 3) }, 
            new Customer { Id = 3, Name = "NewLow", BirthTime = new DateTime(2004, 4, 4) },
        };
    }

    public static partial class AboutOData
    {
        // This sample order controller demonstrates how to create an action which supports
        // OData style queries using by accessing the query directly before applying it.
        // This allows for inspection and manipulation of the query before it is being
        // applied.

        public static void ManuallyApplyODataQueryParameters()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                config.Routes.MapHttpRoute(
                    name: "Api",
                    routeTemplate: "api/{controller}"
                );

                using (var response = client.GetAsync("http://example.org/api/order?$top=2&$orderby=quantity").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
        }
    }

    public class OrderController : ApiController
    {
        private static readonly List<Order> OrderList = new List<Order>
        {  
            new Order{ Id = 11, Name = "Order1", Quantity = 1 }, 
            new Order{ Id = 33, Name = "Order3", Quantity = 3 }, 
            new Order { Id = 22, Name = "Order2", Quantity = 2 }, 
            new Order { Id = 3, Name = "Order0", Quantity = 0 },
        };

        public IQueryable<Order> Get(ODataQueryOptions queryOptions)
        {
            // Validate the top parameter
            if (!ValidateTopQueryOption(queryOptions.Top))
            {
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid value for $top query parameter");
                throw new HttpResponseException(response);
            }

            return queryOptions.ApplyTo(OrderList.AsQueryable()) as IQueryable<Order>;
        }

        private static bool ValidateTopQueryOption(TopQueryOption top)
        {
            if (top != null && top.RawValue != null)
            {
                int topValue = Int32.Parse(top.RawValue, NumberStyles.None);
                return topValue < 10;
            }
            return true;
        }
    }

    public static partial class AboutOData
    {
        // This sample customer controller demonstrates how to create an action which supports
        // OData style queries using the [Queryable] attribute. The difference in this controller
        // is that we return an HttpResponseMessage instead of IEnumerable<T> as is 
        // the case in the CustomerController. This allows up to add extra header
        // fields, manipulate the status code, etc.

        public static void ODataWithReturnedResponseMessage()
        {
            using (var config = new HttpConfiguration())
            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                config.Routes.MapHttpRoute(
                    name: "Api",
                    routeTemplate: "api/{controller}"
                );

                using (var response = client.GetAsync("http://example.org/api/response?$top=2&$orderby=name").Result)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    Helpers.AssertEquality(Helpers.__, body);
                }
            }
        }
    }

    public class ResponseController : ApiController
    {
        [Queryable]
        public HttpResponseMessage Get()
        {
            // Create an HttpResponseMessage and add an HTTP header field
            HttpResponseMessage response = Request.CreateResponse<IEnumerable<Customer>>(HttpStatusCode.OK, CustomerList);
            response.Headers.Add("Sample-Header", "Sample-Value");

            // Return our response. Query composition will still happen on the CustomerList given the Queryable attribute
            return response;
        }

        private static readonly List<Customer> CustomerList = new List<Customer>
        {  
            new Customer
            { 
                Id = 11, Name = "Lowest", BirthTime = new DateTime(2001, 1, 1),
                Orders = new Order[] 
                { 
                    new Order { Id = 0 , Quantity = 10 },  
                    new Order { Id = 1 , Quantity = 50 } 
                }
            }, 
            new Customer
            { 
                Id = 33, Name = "Highest", BirthTime = new DateTime(2002, 2, 2),
                Orders = new Order[] 
                { 
                    new Order { Id = 2 , Quantity = 10 }, 
                    new Order { Id = 3 , Quantity = 5 } 
                }
            }, 
            new Customer { Id = 22, Name = "Middle", BirthTime = new DateTime(2003, 3, 3) }, 
            new Customer { Id = 3, Name = "NewLow", BirthTime = new DateTime(2004, 4, 4) },
        };
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BirthTime { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
