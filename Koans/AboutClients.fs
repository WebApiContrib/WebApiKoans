namespace Koans
(* Lesson 4: Consume APIs with HttpClient

We've already used a `client` to pass a request and receive a response in order to learn
about controllers and message handlers. We used the simplest possible approach to get
us through the basics.

You consume web apis with the `System.Net.Http.HttpClient`. This type provides methods
for retrieving data using the standard HTTP methods, as well as a general `SendAsync`
method that allows you to supply a complete `HttpRequestMessage`. In this lesson, we'll
look at how you can consume apis and leverage the power of
[F# Active Patterns](http://msdn.microsoft.com/en-us/library/dd233248.aspx).
*)

open System
open System.Net
open System.Net.Http
open System.Web.Http
open FSharpKoans.Core
open Koans.Core
open Swensen.Unquote.Assertions

[<Koan(Sort = 4)>]
module ``about clients`` =
  // First things first: HttpClient is intended to be kept around for the life of your application.
  // Starting up a client in every use is very expensive and can lead to bugs.
  let client = new HttpClient(server)

  // We'll use message handlers to create koan-specific request handlers for each of the following koans.
  // This option is the best for preventing collisions with the existing controller types, and it lets us
  // specifically set the expectation within each koan. We'll define a helper here to return an instance
  // of the `AsyncHandler` we saw in AboutMessageHandlers.
  let addHandler f =
    let handler = new AsyncHandler(f)
    config.MessageHandlers.Add(handler)
    config.Routes.MapHttpRoute("Api", "api") |> ignore

  [<Koan>]
  let ``Send a request and get a response``() =
    addHandler <| fun request -> async.Return <| new HttpResponseMessage(Content = new StringContent("Hello, client!"))
    async {
      use request = new HttpRequestMessage(HttpMethod.Get, "http://anything/api")
      use! response = Async.AwaitTask <| client.SendAsync(request)
      let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ __ = result @>
    } |> Async.RunSynchronously
    reset()
  
  [<Koan>]
  let ``GET a string asynchronously``() =
    addHandler <| fun request -> async.Return <| new HttpResponseMessage(Content = new StringContent("Hello, client!"))
    async {
      // Rather than creating a request ourselves, we can use the `GetAsync` method.
      use! response = Async.AwaitTask <| client.GetAsync("http://anything/api")
      let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ __ = result @>
    } |> Async.RunSynchronously
    reset()

  // TODO: Other methods, such as PostAsync, etc.

  // TODO: use different approaches in each of the returned responses.
  [<Koan>]
  let ``Pattern matching response status codes`` =
    // You are now enlightened enough to get a response using SendAsync and it's various helper methods.
    // However, in many cases you really need to identify one of several expected response status codes.
    // HTTP has a rich set of status codes that allow your application to respond intelligently.
    let random = Random()
    addHandler <| fun request ->
      match random.Next(3) with
      | 0 -> new HttpResponseMessage(Content = new StringContent("You found me!"))
      | 1 -> new HttpResponseMessage(HttpStatusCode.NotFound, Content = new StringContent("Not here"))
      | 2 -> new HttpResponseMessage(HttpStatusCode.BadRequest, Content = new StringContent("Come again?"))
      | _ -> new HttpResponseMessage(HttpStatusCode.InternalServerError, Content = new StringContent("Business hours are over, baby."))
      |> async.Return

    async {
      use! response = Async.AwaitTask <| client.GetAsync("http://anything/api")
      match response.StatusCode with
      | HttpStatusCode.OK ->
          let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
          test <@ __ = result @>
      | HttpStatusCode.BadRequest ->
          let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
          test <@ __ = result @>
      | HttpStatusCode.NotFound ->
          let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
          test <@ __ = result @>
      | _ ->
          let! result = Async.AwaitTask <| response.Content.ReadAsStringAsync()
          test <@ __ = result @>
    } |> Async.RunSynchronously
    reset()

  // TODO: use different approaches in each of the returned responses.
  [<Koan>]
  let ``Pattern matching response status codes with Active Patterns`` =
    let random = Random()
    addHandler <| fun request ->
      match random.Next(3) with
      | 0 -> new HttpResponseMessage(Content = new StringContent("You found me!"))
      | 1 -> new HttpResponseMessage(HttpStatusCode.NotFound, Content = new StringContent("Not here"))
      | 2 -> new HttpResponseMessage(HttpStatusCode.BadRequest, Content = new StringContent("Come again?"))
      | _ -> new HttpResponseMessage(HttpStatusCode.InternalServerError, Content = new StringContent("Business hours are over, baby."))
      |> async.Return

    // Rather than using an if/then/else branch structure, let's leverage F# Active Patterns.
    let (|OK|BadRequest|NotFound|Unknown|) (response: HttpResponseMessage) =
      match response.StatusCode with
      | HttpStatusCode.OK -> OK(response.Headers, response.Content)
      | HttpStatusCode.BadRequest -> BadRequest(response.Headers, response.Content)
      | HttpStatusCode.NotFound -> NotFound(response.Headers, response.Content)
      | _ -> Unknown(response.Headers, response.Content)

    async {
      use! response = Async.AwaitTask <| client.GetAsync("http://anything/api")
      match response with
      | OK(_, content) ->
          let! result = Async.AwaitTask <| content.ReadAsStringAsync()
          test <@ __ = result @>
      | BadRequest(_, content) ->
          let! result = Async.AwaitTask <| content.ReadAsStringAsync()
          test <@ __ = result @>
      | NotFound(_, content) ->
          let! result = Async.AwaitTask <| content.ReadAsStringAsync()
          test <@ __ = result @>
      | Unknown(_, content) ->
          let! result = Async.AwaitTask <| content.ReadAsStringAsync()
          test <@ __ = result @>
    } |> Async.RunSynchronously
    reset()

  // Don't forget to dispose your client when you are throught with it.
  client.Dispose()
