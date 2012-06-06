(* Lesson 4: Consume APIs with HttpClient

You consume web apis with the `System.Net.Http.HttpClient`. This type provides methods
for retrieving data using the standard HTTP methods, as well as a general `SendAsync`
method that allows you to supply a complete `HttpRequestMessage`. In this lesson, we'll
look at how you can consume apis and leverage the power of
[F# Active Patterns](http://msdn.microsoft.com/en-us/library/dd233248.aspx).
*)

#load "Koans.fsx"

open System.Net.Http
open Swensen.Unquote.Assertions
