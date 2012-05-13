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
open System.Threading.Tasks
open System.Web.Http
open System.Web.Http.Controllers
open System.Web.Http.Dispatcher

type HttpContentSerializationHandler =
  inherit DelegatingHandler
  new () = { inherit DelegatingHandler() }
  new (innerHandler) = { inherit DelegatingHandler(innerHandler) }

  static member private ConvertToStreamContent(originalContent: HttpContent) =
    if originalContent = null then null else
    if originalContent :? StreamContent then originalContent :?> StreamContent else
    let ms = new System.IO.MemoryStream()
    originalContent.CopyToAsync(ms).Wait()
    ms.Position <- 0L
    let streamContent = new StreamContent(ms)
    for header in originalContent.Headers do
      streamContent.Headers.AddWithoutValidation(header.Key, header.Value)
    streamContent

  override x.SendAsync(request, cancellationToken) =
    request.Content <- HttpContentSerializationHandler.ConvertToStreamContent(request.Content)
    base.SendAsync(request, cancellationToken).ContinueWith((fun (responseTask: Task<HttpResponseMessage>) ->
        let response = responseTask.Result
        response.Content <- HttpContentSerializationHandler.ConvertToStreamContent(response.Content)
        response), cancellationToken)

//  override x.SendAsync(request, cancellationToken) =
//    request.Content <- 

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
config.MessageHandlers.Add(new HttpContentSerializationHandler())
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

