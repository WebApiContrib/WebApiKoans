[<AutoOpen>]
module Koans

#I @"..\packages\System.Net.Http.2.0.20126.16343\lib\net40"
#I @"..\packages\System.Net.Http.Formatting.4.0.20126.16343\lib\net40"
#I @"..\packages\System.Web.Http.Common.4.0.20126.16343\lib\net40"
#I @"..\packages\System.Json.4.0.20126.16343\lib\net40"
#I @"..\packages\AspNetWebApi.Core.4.0.20126.16343\lib\net40"
#I @"..\packages\Unquote.2.1.0\lib\net40"

#r "System.Json.dll"
#r "System.Net.Http.dll"
#r "System.Net.Http.Formatting.dll"
#r "System.Web.Http.Common.dll"
#r "System.Web.Http.dll"
#r "Unquote.dll"

let __ = "Please fill in the blank"

open System
open System.Collections.Generic
open System.Net.Http
open System.Web.Http
open System.Web.Http.Controllers
open System.Web.Http.Dispatcher

type KoansControllerFactory(config) =
  let controllers = Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)

  member x.Register<'a when 'a :> ApiController>() =
    let _type = typeof<'a>
    let name = _type.Name.Replace("Controller", "")
    controllers.[name] <- _type

  member x.Clear() = controllers.Clear()

  interface IHttpControllerFactory with

    member x.CreateController(controllerContext, name) =
      match controllers.TryGetValue(name) with
      | true, _type ->
          controllerContext.ControllerDescriptor <- HttpControllerDescriptor(config, name, _type)
          controllerContext.Controller <- controllerContext.ControllerDescriptor.HttpControllerActivator.Create(controllerContext, _type)
          controllerContext.Controller
      | _ -> null

    member x.ReleaseController(controller) =
      (controller :?> ApiController).Dispose()

let config = new HttpConfiguration()
let controllerFactory = KoansControllerFactory(config)
config.ServiceResolver.SetService(typeof<IHttpControllerFactory>, controllerFactory)

let server = new HttpServer(config)
let client = new HttpClient(server)

let reset() =
  controllerFactory.Clear()
  config.Routes.Clear()

let cleanup() =
  client.Dispose()
  server.Dispose()
  controllerFactory.Clear()
  config.Dispose()

