[<AutoOpen>]
module Koans

#I @"..\packages\Microsoft.Net.Http.2.0.20505.0\lib\net40"
#I @"..\packages\Microsoft.AspNet.WebApi.Core.4.0.20505.0\lib\net40"
#I @"..\packages\Microsoft.AspNet.WebApi.Client.4.0.20505.0\lib\net40"
#I @"..\packages\ImpromptuInterface.5.6.7\lib\net40"
#I @"..\packages\ImpromptuInterface.FSharp.1.1.0\lib\net40"
#I @"..\packages\Newtonsoft.Json.4.5.6\lib\net40"
#I @"..\packages\Unquote.2.2.1\lib\net40"

#r "System.Net.Http.dll"
#r "System.Net.Http.Formatting.dll"
#r "System.Web.Http.dll"
#r "ImpromptuInterface.dll"
#r "ImpromptuInterface.FSharp.dll"
#r "Newtonsoft.Json.dll"
#r "Unquote.dll"

let __ = "Please fill in the blank"
let ___ = async.Return "Please fill in the blank"

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading.Tasks
open System.Web.Http
open System.Web.Http.Controllers
open System.Web.Http.Dependencies
open System.Web.Http.Dispatcher

// This monstrosity is required to detect controllers when running in FSI. In a console or web project, this is unnecessary.
[<AllowNullLiteral>]
type DictionaryDependencyResolver() as x =
  let mutable store = new Dictionary<Type, obj[]>()
  do store.Add(typeof<IHttpControllerActivator>, [| x |])

  member x.RegisterInstance(key, value) =
    if store.ContainsKey(key) then
      store.[key] <- [| yield value; yield! store.[key] |]
    else store.Add(key, [| value |])

  member x.RegisterInstances(key, values) = store.Add(key, values)

  member x.Dispose() =
    store.Clear()
    store <- null

  interface IDependencyResolver with
    member x.BeginScope() = x :> IDependencyScope

    member x.GetService(serviceType) =
      if store.ContainsKey(serviceType) then
        store.[serviceType] |> Seq.head
      else null

    member x.GetServices(serviceType) =
      if store.ContainsKey(serviceType) then
        store.[serviceType] |> Array.toSeq
      else Seq.empty

    member x.Dispose() = x.Dispose()

  interface IHttpControllerActivator with
    member x.Create(request, controllerDescriptor, controllerType) =
      store.[controllerType] |> Seq.head :?> IHttpController

// NOTE: Thanks to Kiran Challa of Microsoft for this code. (http://forums.asp.net/t/1787356.aspx/1?In+memory+host+with+formatting)
let serializationHandler =
  let convertToStreamContent(originalContent: HttpContent) =
    if originalContent = null then null else
    if originalContent :? StreamContent then originalContent :?> StreamContent else
    let ms = new System.IO.MemoryStream()
    originalContent.CopyToAsync(ms).Wait()
    ms.Position <- 0L
    let streamContent = new StreamContent(ms)
    for header in originalContent.Headers do
      streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value) |> ignore
    streamContent
  { new DelegatingHandler() with
      override x.SendAsync(request, cancellationToken) =
        request.Content <- convertToStreamContent(request.Content)
        base.SendAsync(request, cancellationToken).ContinueWith((fun (responseTask: Task<HttpResponseMessage>) ->
          let response = responseTask.Result
          response.Content <- convertToStreamContent(response.Content)
          response), cancellationToken) }

let config = new HttpConfiguration()
let resolver = new DictionaryDependencyResolver()
config.DependencyResolver <- resolver
config.Services.Replace(typeof<IHttpControllerActivator>, resolver)

let server = new HttpServer(config)
let client = new HttpClient(server)

let reset() =
  config.Routes.Clear()

let cleanup() =
  if client <> null then client.Dispose()
  if server <> null then server.Dispose()
  if config <> null then config.Dispose()
  if serializationHandler <> null then serializationHandler.Dispose()
