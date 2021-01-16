using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Anotar.Serilog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
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

        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;

        public static async Task<CreationResult> Create(
          string urlOrId,
          double startTime = 0,
          double endTime = -1,
          int parentElementId = -1,
          double watchPoint = 0,
          ViewMode viewMode = MediaPlayerConst.DefaultViewMode,
          bool shouldDisplay = true)
        {
            // TODO: Time the verification check
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
            string thumbnailUrl = (string)metadata["thumbnail"];

            ytEl = new YouTubeMediaElement
            {
                Id = youtubeId,
                StartTime = startTime,
                EndTime = endTime,
                WatchPoint = watchPoint,
                ViewMode = viewMode,
            };

            List<ContentBase> contents = new List<ContentBase>();

            string elementHtml = string.Format(CultureInfo.InvariantCulture,
                                               MediaPlayerConst.YouTubeElementFormat,
                                               title,
                                               ytEl.GetJsonB64());
            contents.Add(new TextContent(true, elementHtml));
            if (Config.IncludeYouTubeThumbnail)
            {
                Image img = DownloadThumbnail(thumbnailUrl);
                if (img != null)
                {
                    var imgContent = ContentEx.CreateImageContent(img, string.Format(YTConst.VideoThumbImgRegPath, ytEl.Id));
                    if (imgContent != null)
                        contents.Add(imgContent);
                }
            }

            return CreateSMElement(parentElementId, contents, title, uploader, creationDate, ytEl.Url, shouldDisplay);
        }

        private static string HumanReadableDate(string date) 
        {
            if (string.IsNullOrEmpty(date) || date.Length != 8)
                return null;

            var y = int.TryParse(date.Substring(0, 4), out var year);
            var m = int.TryParse(date.Substring(4, 2), out var month);
            var d = int.TryParse(date.Substring(6, 2), out var day);
            if (new[] { y, m, d }.Any(x => !x))
                return null;

            var dt = new DateTime(year, month, day);
            return dt.ToString(CultureInfo.InvariantCulture);
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

            var contents = new List<ContentBase> { new TextContent(true, elementHtml) };

            return CreateSMElement(parentElementId, contents, title, uploader, creationDate, ytEl.Url, shouldDisplay);

        }

        private static CreationResult CreateSMElement(int parentElId, List<ContentBase> contents, string title,
                                                      string uploader, string creationDate, string url,
                                                      bool shouldDisplay)
        {
            IElement parentElement =
              parentElId > 0
                ? Svc.SM.Registry.Element[parentElId]
                : null;

            var elemBuilder =
              new ElementBuilder(ElementType.Topic,
                                 contents.ToArray())
                .WithParent(parentElement)
                .WithTitle(title)
                .WithPriority(MediaPlayerState.Instance.Config.DefaultExtractPriority)
                .WithReference(
                  r => r.WithTitle(title)
                        .WithAuthor(uploader)
                        .WithDate(HumanReadableDate(creationDate))
                        .WithSource("YouTube")
                        .WithLink(url)
                );

            if (shouldDisplay == false)
                elemBuilder = elemBuilder.DoNotDisplay();

            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder)
              ? CreationResult.Ok
              : CreationResult.FailCannotCreateElement;
        }

        private static Image DownloadThumbnail(string url)
        {
            try
            {
                using(var wc = new WebClient())
                {
                    byte[] bytes = wc.DownloadData(url);
                    if (bytes != null)
                    {
                        MemoryStream ms = new MemoryStream(bytes);
                        if (ms != null)
                        {
                            return Image.FromStream(ms);
                        }
                    }
                }
            }
            catch (Exception)
            {
                LogTo.Debug("Failed to download thumbnail for MediaPlayer element");
            }

            return null;
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
