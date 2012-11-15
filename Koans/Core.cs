using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;

namespace Koans
{
    public static class Core
    {
        public static readonly HttpConfiguration Config = new HttpConfiguration();
        public static readonly HttpServer Server = new HttpServer(Config);
        public static readonly HttpMessageInvoker Client = new HttpMessageInvoker(Server);
        public static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        public static void Reset()
        {
            Config.MessageHandlers.Clear();
            Config.Routes.Clear();
        }

        public static void Cleanup()
        {
            if (Cts != null)
            {
                Cts.Cancel();
                Cts.Dispose();
            }

            if (Client != null)
                Client.Dispose();

            if (Server != null)
                Server.Dispose();

            if (Config != null)
                Config.Dispose();
        }
    }
}
