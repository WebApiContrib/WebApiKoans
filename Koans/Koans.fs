module Koans.Core

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading.Tasks
open System.Web.Http
open System.Web.Http.Controllers
open System.Web.Http.Dependencies
open System.Web.Http.Dispatcher

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
