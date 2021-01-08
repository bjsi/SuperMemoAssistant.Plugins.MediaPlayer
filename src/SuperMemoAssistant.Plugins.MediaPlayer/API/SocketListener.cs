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
        public static bool Running { get; set; } = true;

        private static RpcResultHandler ResultHandler { get; } = new AsyncCallback(state =>
                    {
                        var async = ((JsonRpcStateAsync)state);
                        var result = async.Result;
                        var writer = ((StreamWriter)async.AsyncState);

                        Console.WriteLine(result);
                        writer.WriteLine(result);
                        writer.FlushAsync();
                        writer.Close(); // Handle a single call, then disconnect the client
                    }
                );

        private static void ProcessJson(StreamWriter writer, string line)
        {
            var async = new JsonRpcStateAsync(rpcResultHandler, writer) { JsonRpc = line };
            JsonRpcProcessor.Process(async, writer);
        }

        public static void Start()
        {
            var server = new TcpListener(IPAddress.Parse(Config.APIHost), Config.APIPort);
            server.Start();
            LogTo.Info($"MediaPlayer JSON RPC API running at {Config.APIHost}:{Config.APIPort}");
            while (true)
            {
                try
                {
                    using (var client = server.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        LogTo.Debug("Client connected to MediaPlayer JSON RPC API");
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        var writer = new StreamWriter(stream, new UTF8Encoding(false));

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            ProcessJson(writer, line);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogTo.Info($"MediaPlayer JSON RPC API caught exception {e}");
                }
            }
        }
    }
}
