(* Lesson 2: Build APIs with ApiControllers

The standard api for building applications with Web API
is through the ApiController. This implements the IHttpController
interface and provides a number of additional helpers such
as model binding and formatting.
*)

#load "Koans.fsx"

open System.Net.Http
open System.Web.Http
open Swensen.Unquote.Assertions

module ``Respond to requests with ApiControllers`` =

  module ``Simple Hello world controller`` =

    // To use an ApiController, you need to define a subclass.
    // We will extend and reuse this type throughout this set of koans.
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
      test <@ "\"Hello, ApiController!\"" = body @>
    } |> Async.RunSynchronously

    reset()

  module ``Create an echo controller`` =

    type TestController() =
      inherit ApiController()

      // We'll amend our existing controller with a Post method
      // that will return whatever is POSTed. 
      member x.Post() = __

    controllerFactory.Register<TestController>()
    config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

    // Now send a POST request from the client to retrieve the result.
    async {
      let content = new StringContent("Hello, ApiController!")
      let! response = Async.AwaitTask <| client.PostAsync("http://example.org/api/test", content)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ "Hello, ApiController!" = body @>
    } |> Async.RunSynchronously

    reset()

  module ``Are you sure you made an echo controller`` =
    open System.IO
    open System.Net

    type TestController() =
      inherit ApiController()

      // Contrary to what you might have expected, copying a request body is not quite as simple
      // as it may at first appear. The previous step can be successfully completed by returning
      // x.Request.Content.ReadAsStreamAsync() or x.Request.Content.ReadAsStringAsync().
      // However, if you add a test to check the content type of the request and the response, you
      // will find the request submitted text/plain, and the response submitted application/json.
      // Those certainly look the same, but they are not. Also, returning x.Request.Content
      // will throw an ObjectDisposedException as the request content is disposed before the
      // response completes. You have to copy the content to a new content.
      member x.Post() =
        let request = x.Request
        let stream = new MemoryStream()
        request.Content.CopyToAsync(stream).ContinueWith(fun _ ->
          // Don't miss the importance of resetting the position on the stream.
          // If you don't know what this does, try commenting it out.
          stream.Position <- 0L
          let content = new StreamContent(stream)
          for header in request.Content.Headers do
            content.Headers.AddWithoutValidation(header.Key, header.Value)
          new HttpResponseMessage(HttpStatusCode.OK, Content = content))

    controllerFactory.Register<TestController>()
    config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

    // Now send a POST request from the client to retrieve the result.
    async {
      let content = new StringContent("Hello, ApiController!")
      let! response = Async.AwaitTask <| client.PostAsync("http://example.org/api/test", content)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ "Hello, ApiController!" = body @>
      // Note that this passes for a test of the request's content type as we copied the value above.
      test <@ __ = response.Content.Headers.ContentType.MediaType @>
    } |> Async.RunSynchronously

    reset()

cleanup()
