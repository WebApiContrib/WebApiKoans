module Program

open System
open Koans
open Koans.Core

[<EntryPoint>]
let main args =
  Console.WriteLine("Run the scripts and fill in the missing __")
  
  try
    try
      Lesson1.Content.``Reading string content``()
      Lesson1.Content.``Reading form data``()

      Lesson2.Controllers.``Simple Hello world controller``()
      Lesson2.Controllers.``Create an echo controller``()
      Lesson2.Controllers.``Are you sure you made an echo controller``()

      Lesson3.Handlers.``Respond to a GET request with a DelegatingHandler``()
    with e -> printfn "%s" e.Message
  finally cleanup()

  Console.ReadLine() |> ignore
  0
