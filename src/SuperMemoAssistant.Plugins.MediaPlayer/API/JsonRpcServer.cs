using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Anotar.Serilog;
using AustinHarris.JsonRpc;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    public class JsonRpcServer
    {

        private MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;
        private bool HasExited => Svc<MediaPlayerPlugin>.Plugin.HasExited;
        private TcpListener Server { get; set; }
        private object Service { get; set; } // TODO: Test when this is null
        private const string SessionId = "session";

        public async Task StartAsync()
        {
            Server = new TcpListener(IPAddress.Parse("0.0.0.0"), 9898);
            Server.Start();
            LogTo.Debug($"MediaPlayer API started");
            while (!HasExited)
            {
                try
                {
                    using (var client = await Server.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        LogTo.Debug("Client connected to MediaPlayer socket");

                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {

                            var line = await reader.ReadLineAsync();
                            LogTo.Debug("MediaPlayer API received data from client: " + line);

                            var response = await JsonRpcProcessor.Process(SessionId, line);
                            LogTo.Debug("MediaPlayer API responsded to client: " + response);

                            await writer.WriteLineAsync(response);
                            await writer.FlushAsync();

                            client.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    LogTo.Debug($"MediaPlayer API caught exception {e}");
                }
            }

            LogTo.Debug("MediaPlayer API shutting down");
            Server.Stop();
        }

        public void RegisterService(object service)
        {
            Handler.RegisterInstance(SessionId, service);
        }

        public void RevokeService()
        {
            this.Service = null;
            Handler.GetSessionHandler(SessionId).Destroy();
        }
    }
}
