namespace Koans
(* Lesson 3: Build APIs directly with instances of HttpMessageHandler.

In order to run our tests, we first need a test runner.
*)

open System.Net.Http
open System.Web.Http
open System.Threading.Tasks
open FSharpKoans.Core
open Koans.Core
open Swensen.Unquote.Assertions

[<Koan(Sort = 3)>]
module ``about message handlers`` =

  [<Koan>]
  let ``Respond to a GET request with a DelegatingHandler``() =

    // Now we can create an `HttpHandler` to return a response of `"Hello, world!"`.
    // `HttpMessageHandler`s always return a `Task<'T>` from the `SendAsync` method,
    // but since we don't need the async response, we'll just use `TaskCompletionSource`.
    let handler =
      { new DelegatingHandler() with
          override x.SendAsync(request, cancellationToken) =
            let response = new HttpResponseMessage(Content = new StringContent("Hello, world!"))
            let tcs = new TaskCompletionSource<_>()
            tcs.SetResult(response)
            tcs.Task }

    // Set up the configuration for the server.
    let config = new HttpConfiguration()

    // Add our message handler to the configuration.
    config.MessageHandlers.Add(handler)

    // Create the server and set its configuration and the message handler.
    // Note that the HttpServer ctor also takes a message handler,
    // but that handler changes how the server behaves. We'll use the
    let server = new HttpServer(config)

    // Create an `HttpClient` that will send directly to the `server`.
    let client = new HttpClient(server)

    // Now send a GET request from the client to retrieve the result.
    async {
      use request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/api/test")
      let! response = Async.AwaitTask <| client.SendAsync(request, cts.Token)
      let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
      test <@ __ = body @>
    } |> Async.RunSynchronously

    reset()
