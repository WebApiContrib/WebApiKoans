module Program

open System
open Koans
open Koans.Core

[<EntryPoint>]
let main args =
  Console.WriteLine("Run the scripts and fill in the missing __")
  
  try
    try
      AboutContent.``Reading string content``()
      AboutContent.``Reading form data``()

      AboutControllers.``Simple Hello world controller``()
      AboutControllers.``Create an echo controller``()
      AboutControllers.``Are you sure you made an echo controller``()

      AboutMessageHandlers.``Respond to a GET request with a DelegatingHandler``()
    with e -> printfn "%A" e
  finally cleanup()

  Console.ReadLine() |> ignore
  0
