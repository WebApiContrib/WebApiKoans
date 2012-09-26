module Koans.Core

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading.Tasks
open System.Web.Http
open System.Web.Http.Controllers
open System.Web.Http.Dependencies
open System.Web.Http.Dispatcher

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

let server = new HttpServer()
let config = server.Configuration
let client = new HttpMessageInvoker(server)
let cts = new System.Threading.CancellationTokenSource()

let reset() =
  config.MessageHandlers.Clear()
  config.Routes.Clear()

let cleanup() =
  if cts <> null then cts.Cancel(); cts.Dispose()
  if client <> null then client.Dispose()
  if server <> null then server.Dispose()
  if config <> null then config.Dispose()
  if serializationHandler <> null then serializationHandler.Dispose()
