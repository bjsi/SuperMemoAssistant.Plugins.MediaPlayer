using System;
using System.Globalization;
using System.Threading.Tasks;
using Anotar.Serilog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.MediaPlayer.Helpers;
using SuperMemoAssistant.Plugins.MediaPlayer.YouTube;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{

    public class YouTubeMediaElement : MediaElement
    {
        #region Constructors

        // TODO: Support playlists
        public YouTubeMediaElement()
        {
        }

        #endregion


        #region Properties & Fields - Public

        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }

        public string Url => YTConst.VidURLPrefix + Id;

        #endregion


        public static async Task<CreationResult> Create(
          string urlOrId,
          double startTime = -1,
          double endTime = -1,
          int parentElementId = -1,
          double watchPoint = -1,
          ViewMode viewMode = MediaPlayerConst.DefaultViewMode,
          bool shouldDisplay = true)
        {
            JObject metadata = await YdlUtils.GetYouTubeVideoMetadata(urlOrId);
            if (metadata == null || string.IsNullOrWhiteSpace((string)metadata["id"]))
            {
                LogTo.Warning($"Failed to recognise {urlOrId} as a YouTube video");
                return CreationResult.FailUnknown;
            }

            YouTubeMediaElement ytEl;
            string youtubeId = (string)metadata["id"];
            string title = (string)metadata["title"];
            string uploader = (string)metadata["uploader"];
            string uploaderId = (string)metadata["uploader_id"];
            string creationDate = (string)metadata["upload_date"];

            try
            {
                ytEl = new YouTubeMediaElement
                {
                    Id = youtubeId,
                    StartTime = startTime,
                    EndTime = endTime,
                    WatchPoint = watchPoint,
                    ViewMode = viewMode,
                };
            }
            catch (Exception ex)
            {
                LogTo.Error(ex, "Exception thrown while creating new Advanced Media Player YouTube element");
                return CreationResult.FailUnknown;
            }

            string elementHtml = string.Format(CultureInfo.InvariantCulture,
                                               MediaPlayerConst.YouTubeElementFormat,
                                               title,
                                               ytEl.GetJsonB64());

            IElement parentElement =
              parentElementId > 0
                ? Svc.SM.Registry.Element[parentElementId]
                : null;

            var elemBuilder =
              new ElementBuilder(ElementType.Topic,
                                 elementHtml)
                .WithParent(parentElement)
                .WithTitle(title)
                .WithPriority(MediaPlayerState.Instance.Config.DefaultExtractPriority)
                .WithReference(
                  r => r.WithTitle(title)
                        .WithAuthor($"{uploader} (id={uploaderId})")
                        .WithDate(creationDate)
                        .WithSource("YouTube")
                        .WithLink($"https://www.youtube.com/watch?v=" + youtubeId)
                );

            if (shouldDisplay == false)
                elemBuilder = elemBuilder.DoNotDisplay();

            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder)
              ? CreationResult.Ok
              : CreationResult.FailCannotCreateElement;
        }

        public static CreationResult Create(
                int parentElementId,
                string videoId,
                double startTime,
                double endTime,
                double watchPoint,
                ViewMode viewMode,
                bool shouldDisplay)
        {

            var html = ContentEx.GetCurrentElementContent();
            var refs = ReferenceParser.GetReferences(html);
            if (refs == null)
                return CreationResult.FailUnknown;

            string title = refs.Title;
            string uploader = refs.Author;
            string creationDate = refs.Date;

            var ytEl = new YouTubeMediaElement
            {
                Id = videoId,
                StartTime = startTime,
                EndTime = endTime,
                WatchPoint = watchPoint,
                ViewMode = viewMode,
            };

            string elementHtml = string.Format(CultureInfo.InvariantCulture,
                                               MediaPlayerConst.YouTubeElementFormat,
                                               title,
                                               ytEl.GetJsonB64());

            IElement parentElement =
              parentElementId > 0
                ? Svc.SM.Registry.Element[parentElementId]
                : null;

            var elemBuilder =
              new ElementBuilder(ElementType.Topic,
                                 elementHtml)
                .WithParent(parentElement)
                .WithTitle(title)
                .WithPriority(MediaPlayerState.Instance.Config.DefaultExtractPriority)
                .WithReference(
                  r => r.WithTitle(title)
                        .WithAuthor(uploader)
                        .WithDate(creationDate)
                        .WithSource("YouTube")
                        .WithLink($"https://www.youtube.com/watch?v=" + videoId)
                );

            if (shouldDisplay == false)
                elemBuilder = elemBuilder.DoNotDisplay();

            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder)
              ? CreationResult.Ok
              : CreationResult.FailCannotCreateElement;
        }

        public static YouTubeMediaElement TryReadElement(string elText,
                                                int elementId)
        {
            if (string.IsNullOrWhiteSpace(elText))
                return null;

            var reRes = MediaPlayerConst.RE_YouTubeElement.Match(elText);

            if (reRes.Success == false)
                return null;

            try
            {
                string toDeserialize = reRes.Groups[1].Value.FromBase64();

                var ytEl = JsonConvert.DeserializeObject<YouTubeMediaElement>(toDeserialize);

                if (ytEl != null) // && elementId > 0)
                {
                    ytEl.ElementId = elementId;

                    // TODO: Remove element Id test when better element transition is implemented
                    // Double check
                    if (Svc.SM.UI.ElementWdw.CurrentElementId != elementId)
                        return null;
                }

                return ytEl;
            }
            catch
            {
                return null;
            }
        }

        // TODO: Save, Update Html

        private string UpdateHtml(string html)
        {
            string newElementDataDiv = string.Format(CultureInfo.InvariantCulture,
                                                     MediaPlayerConst.LocalElementDataFormat,
                                                     GetJsonB64());

            return MediaPlayerConst.RE_YouTubeElement.Replace(html,
                                               newElementDataDiv);
        }
    }
}
