using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AustinHarris.JsonRpc;


namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    public static class SocketListener
    {

        private static MediaPlayerCfg Config => Svc<MediaPlayer>.Plugin.Config;
        public bool HasExited { get; set; } = false;

        public static async Task Start()
        {
            var server = new TcpListener(IPAddress.Parse(Config.Host), Config.Port);
            await server.StartAsync();
            LogTo.Info($"MediaPlayer API running");
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
                        var line = await client.ReadLineAsync();
                        LogTo.Debug("MediaPlayer API received data from client: " + line);
                        var response = await JsonProcessor.ProcessAsync(line);
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                        client.Close();
                    }
                }
                catch (Exception e)
                {
                    LogTo.Info($"MediaPlayer API caught exception {e}");
                }
            }
        }
    }
}
