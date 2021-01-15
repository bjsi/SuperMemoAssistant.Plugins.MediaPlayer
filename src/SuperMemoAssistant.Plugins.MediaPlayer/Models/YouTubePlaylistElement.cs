using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{

    public class YouTubePlaylistElement
    {
        #region Constructors

        public YouTubePlaylistElement()
        {
        }

        #endregion

        #region Properties & Fields - Public

        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }

        #endregion


        /* public static async Task<CreationResult> Create( */
        /*   string urlOrId, */
        /*   int parentElementId = -1) */
        /* { */
        /*     JObject metadata = await YdlUtils.GetYouTubeVideoMetadata(urlOrId); */
        /*     if (metadata == null || string.IsNullOrWhiteSpace((string)metadata["id"])) */
        /*     { */
        /*         LogTo.Warning($"Failed to recognise {urlOrId} as a YouTube Playlist"); */
        /*         return CreationResult.FailUnknown; */
        /*     } */
        /* } */
    }
}
