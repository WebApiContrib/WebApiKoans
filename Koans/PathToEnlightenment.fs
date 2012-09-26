#if INTERACTIVE
#r @"..\packages\FAKE.1.64.7\tools\FakeLib.dll"
#r @"..\packages\ImpromptuInterface.5.6.7\lib\net40\ImpromptuInterface.dll"
#r @"..\packages\ImpromptuInterface.FSharp.1.1.0\lib\net40\ImpromptuInterface.FSharp.dll"
#r @"..\packages\Microsoft.Net.Http.2.0.20505.0\lib\net40\System.Net.Http.dll"
#r @"..\packages\Microsoft.AspNet.WebApi.Client.4.0.20505.0\lib\net40\System.Net.Http.Formatting.dll"
#r @"..\packages\Microsoft.AspNet.WebApi.Core.4.0.20505.0\lib\net40\System.Web.Http.dll"
#r @"..\packages\Newtonsoft.Json.4.5.6\lib\net40\Newtonsoft.Json.dll"
#r @"..\packages\Unquote.2.2.1\lib\net40\Unquote.dll"
#r @"..\FSharpKoans.Core\bin\Debug\FSharpKoans.Core.dll"
#endif

open System
open System.IO
open FSharpKoans.Core

#if INTERACTIVE
#load "Koans.fs"
#load "AboutContent.fs"
#load "AboutControllers.fs"
#load "AboutMessageHandlers.fs"
#load "AboutClients.fs"
#endif

open System
open FSharpKoans.Core

let runner = new KoanRunner()
let result = runner.ExecuteKoans()
Koans.Core.cleanup()

match result with
| Success message -> printf "%s" message
| Failure (message, ex) -> 
    printf "%s" message
    printfn ""
    printfn ""
    printfn ""
    printfn ""
    printfn "You have not yet reached enlightenment ..."
    printfn "%s" ex.Message
    printfn ""
    printfn "Please meditate on the following code:"
    printfn "%s" ex.StackTrace
    
printfn ""
printfn ""
printfn ""
printfn ""
printf "Press any key to continue..."
System.Console.ReadKey() |> ignore
