(* Lesson 1: Learn about HttpContent and MediaTypeFormatters

The body of the request and response can take many forms.
`System.Net.Http` defines `HttpContent` as the base for these representations.
`System.Net.Http` also includes several other built-in types,
as well as an `ObjectContent` that will serialize any .NET object
based on the provided `MediaTypeFormatter`.
*)

#load "Koans.fsx"

open System.Net.Http
open Newtonsoft.Json.Linq
open Swensen.Unquote.Assertions

module ``Reading string content`` =

  // Arguably the simplest of the `HttpContent` types is `StringContent`.
  // You create a new `StringContent` in the normal way you create .NET instances.
  // Note that in F# you need to use the `new` keyword because HttpContent
  // implements `IDisposable`.
  let content = new StringContent("Hello, string!")

  // `HttpContent` contains many methods and extension methods for reading its data.
  // All the read methods are asynchronous, as they are generally intended to read
  // data from a network stream. Here we will read the string content back out
  // using `ReadAsStringAsync`.
  let result = content.ReadAsStringAsync()
  
  // Verify that the data we read was the same as we submitted to the `StringContent`.
  async {
    let! body = Async.AwaitTask result
    test <@ __ = body @>
  } |> Async.RunSynchronously

  reset()

module ``Reading form data`` =

  let content = new StringContent("a[]=1&a[]=5&a[]=333", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded")

//  let result = content.ReadAsFormDataAsync()
  let result = content.ReadAsAsync<JObject>()
  
  // Verify that the data we read was the same as we submitted to the `StringContent`.
  async {
    let! body = Async.AwaitTask result
    let arr = body.["a"] :?> JArray
    test <@ __ = arr.ToString() @>
  } |> Async.RunSynchronously

  reset()

cleanup()
