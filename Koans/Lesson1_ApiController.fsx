(* Lesson 0: Build a test runner

In order to run our tests, we first need a test runner.
*)

#load "Koans.fsx"

open System.Net.Http
open System.Web.Http
open Swensen.Unquote.Assertions

// Define a controller deriving from ApiController.
type TestController() =
  inherit ApiController()
  member x.Get() = __

// You don't normally need to do this; it's required for running in FSI.
controllerFactory.Register<TestController>()

// Add a route for the controller.
config.Routes.MapHttpRoute("Api", "api/{controller}") |> ignore

// Now send a GET request from the client to retrieve the result.
async {
  let! response = Async.AwaitTask <| client.GetAsync("http://example.org/api/test")
  let! body = Async.AwaitTask <| response.Content.ReadAsStringAsync()
  test <@ "Hello, ApiController!" = body @>
} |> Async.RunSynchronously

cleanup()

