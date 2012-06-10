namespace Koans
(* Lesson 2: Build APIs with ApiControllers

The standard api for building applications with Web API
is through the ApiController. This implements the IHttpController
interface and provides a number of additional helpers such
as model binding and formatting.
*)

open System.IO
open System.Net
open System.Net.Http
open System.Web.Http
open FSharpKoans.Core
open Koans.Core
open Swensen.Unquote.Assertions

// Note that while we use a `client` to run the tests below, we'll address `System.Net.Http.HttpClient`
// in greater detail in `AboutClients`.

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

  // We'll amend our existing controller with a Post method
  // that will return whatever is POSTed. 
  member x.Post() = __

type TestFixedController() =
  inherit ApiController()

  // Contrary to what you might have expected, copying a request body is not quite as simple
  // as it may at first appear. The previous step can be successfully completed by returning
  // x.Request.Content.ReadAsStreamAsync() or x.Request.Content.ReadAsStringAsync().
  // However, if you add a test to check the content type of the request and the response, you
  // will find the request submitted text/plain, and the response submitted application/json.
  // Those certainly look the same, but they are not. Also, returning x.Request.Content
  // will throw an ObjectDisposedException as the request content is disposed before the
  // response completes. You have to copy the content to a new content.
  member x.Post(request: HttpRequestMessage) =
    printfn "Received %A" request
    let stream = new MemoryStream()
    request.Content.CopyToAsync(stream).ContinueWith(fun _ ->
      // Don't miss the importance of resetting the position on the stream.
      // If you don't know what this does, try commenting it out.
      stream.Position <- 0L
      let content = new StreamContent(stream)
      for header in request.Content.Headers do
        content.Headers.TryAddWithoutValidation(header.Key, header.Value) |> ignore
      new HttpResponseMessage(HttpStatusCode.OK, Content = content))

[<AutoOpen>]
[<Koan(Sort = 2)>]
module ``about controllers`` =

  [<Koan>]
  let ``Simple Hello world controller``() =
    // Controllers can't be found without routing. Here, we use a very generic
    // uri template and map it using the `MapHttpRoute` extension method.
    // If you have worked with MVC, this will be very familiar. In a larger
    // project, this would most likely be defined in your Global.asax file.
    config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

    // Now send a GET request from the client to retrieve the result.
    async {
      use request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/api/test")
      let! response = Async.AwaitTask <| client.SendAsync(request, cts.Token)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ "\"Hello, ApiController!\"" = body @>
    } |> Async.RunSynchronously

    reset()

  [<Koan>]
  let ``Create an echo controller``() =
    config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

    // Now send a POST request from the client to retrieve the result.
    async {
      let request = new HttpRequestMessage(HttpMethod.Post, "http://example.org/api/test", Content = new StringContent("Hello, ApiController!"))
      let! response = Async.AwaitTask <| client.SendAsync(request, cts.Token)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ "\"Hello, ApiController!\"" = body @>
      test <@ __ = response.Content.Headers.ContentType.MediaType @>
    } |> Async.RunSynchronously

    reset()

  [<Koan>]
  let ``Are you sure you made an echo controller``() =
    config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

    // Now send a POST request from the client to retrieve the result.
    async {
      let request = new HttpRequestMessage(HttpMethod.Post, "http://example.org/api/testfixed", Content = new StringContent("Hello, ApiController!"))
      let! response = Async.AwaitTask <| client.SendAsync(request, cts.Token)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ __ = body @>
      // Note that this passes for a test of the request's content type as we copied the value above.
      test <@ __ = response.Content.Headers.ContentType.MediaType @>
    } |> Async.RunSynchronously

    reset()
