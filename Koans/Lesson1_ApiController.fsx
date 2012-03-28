(* Lesson 1: Learn about ApiControllers

The standard api for building applications with Web API
is through the ApiController. This implements the IHttpController
interface and provides a number of additional helpers such
as model binding and formatting.
*)

#load "Koans.fsx"

open System.Net.Http
open System.Web.Http
open Swensen.Unquote.Assertions

module ``Simple Hello world controller`` =

  // To use an ApiController, you need to define a subclass.
  type TestController() =
    inherit ApiController()

    // Inside your controller, define the actions you want to support.
    // By convention, actions matching HTTP method names will be
    // matched the requests to that method. If you don't follow this
    // convention, you can use the AcceptVerbsAttribute or any of
    // the shortcut versions, such as HttpGet, HttpPost, etc.
    // Here, we will just define a Get method.
    member x.Get() = __

  // You don't normally need to do this; it's required for running in FSI.
  // The `DefaultHttpControllerFactory` looks in the currently loaded assembly
  // for all types implementing `IHttpController`. Here, we are using a
  // custom controller factory and registering our `TestController` type.
  // The custom controller factory can be found in Koans.fsx, if you are
  // curious to see more about implementing a custom factory.
  controllerFactory.Register<TestController>()

  // Controllers can't be found without routing. Here, we use a very generic
  // uri template and map it using the `MapHttpRoute` extension method.
  // If you have worked with MVC, this will be very familiar. In a larger
  // project, this would most likely be defined in your Global.asax file.
  config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

  // Now send a GET request from the client to retrieve the result.
  async {
    let! response = Async.AwaitTask <| client.GetAsync("http://example.org/api/test")
    let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
    test <@ "Hello, ApiController!" = body @>
  } |> Async.RunSynchronously

cleanup()
