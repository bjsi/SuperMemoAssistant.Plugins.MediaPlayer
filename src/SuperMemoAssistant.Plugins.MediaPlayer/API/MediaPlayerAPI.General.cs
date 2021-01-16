using AustinHarris.JsonRpc;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    public partial class MediaPlayerAPI
    {
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;

        [JsonRpcMethod]
        public string HelloWorld()
        {
            return "hello world!";
        }
    }
}
