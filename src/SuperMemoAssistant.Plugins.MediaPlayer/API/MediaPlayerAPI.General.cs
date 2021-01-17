using Anotar.Serilog;
using AustinHarris.JsonRpc;
using SuperMemoAssistant.Plugins.MediaPlayer.Helpers;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    public partial class MediaPlayerAPI
    {
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;
        private YouTubeMediaElement CurrentMediaElement { get; }
        public int ExpectedElementId { get; }

        public MediaPlayerAPI(YouTubeMediaElement element, int elementId)
        {
            element.ThrowIfArgumentNull("Failed to create MediaPlayer API JsonRpc service object because media element was null");
            LogTo.Debug($"Loading MediaPlayer API JsonRpc service for YouTube id {element.Id} element id {elementId}");
            this.ExpectedElementId = elementId;
            this.CurrentMediaElement = element;
        }

        [JsonRpcMethod]
        public string HelloWorld()
        {
            return "hello world!";
        }
    }
}
