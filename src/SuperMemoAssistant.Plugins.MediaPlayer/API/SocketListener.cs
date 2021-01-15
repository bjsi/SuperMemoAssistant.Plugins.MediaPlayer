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
    public static class SocketListener
    {

        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;
        public static bool HasExited { get; set; }

        public static async Task Start()
        {
            var server = new TcpListener(IPAddress.Parse(Config.Host), Config.Port);
            server.Start();
            LogTo.Information($"MediaPlayer API running");
            while (!HasExited)
            {
                try
                {
                    using (var client = await server.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        LogTo.Debug("Client connected to MediaPlayer socket");
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        var writer = new StreamWriter(stream, new UTF8Encoding(false));
                        var line = await reader.ReadLineAsync();
                        LogTo.Debug("MediaPlayer API received data from client: " + line);
                        var response = await JsonRpcProcessor.Process(line);
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                        client.Close();
                    }
                }
                catch (Exception e)
                {
                    LogTo.Information($"MediaPlayer API caught exception {e}");
                }
            }

            server.Stop();
        }
    }
}
