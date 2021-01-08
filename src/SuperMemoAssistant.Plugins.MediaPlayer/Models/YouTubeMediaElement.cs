using System;
using System.Globalization;
using System.Threading.Tasks;
using AdvancedMediaPlayer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{

    public class YouTubeMediaElement : MediaElement
    {
        #region Constructors

        // TODO: Subtitles
        // TODO: Support playlists
        public YouTubeMediaElement()
        {
        }

        #endregion


        #region Properties & Fields - Public

        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }

        #endregion


        #region Methods

        public static async Task<CreationResult> Create(
          string urlOrId,
          double startTime = -1,
          double endTime = -1,
          int parentElementId = -1,
          double watchPoint = -1,
          ViewMode viewMode = MediaPlayerConst.DefaultViewMode,
          bool shouldDisplay = true)
        {
            // TODO: Only do this check on import, not
            // for every extract
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
                                               AMPConst.YouTubeElementFormat,
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
                .WithPriority(AMPState.Instance.Config.DefaultExtractPriority)
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

        public static YouTubeMediaElement TryReadElement(string elText,
                                                int elementId)
        {
            if (string.IsNullOrWhiteSpace(elText))
                return null;

            // TODO Separate regexes
            var reRes = AMPConst.RE_Element.Match(elText);

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

        // TODO: Move into base class and add Regex to params
        private string UpdateHtml(string html)
        {
            string newElementDataDiv = string.Format(CultureInfo.InvariantCulture,
                                                     AMPConst.LocalElementDataFormat,
                                                     GetJsonB64());

            return AMPConst.RE_Element.Replace(html,
                                               newElementDataDiv);
        }

        public static void GetInfos(JObject metadata,
                                    out string title,
                                    out string author,
                                    out string date)
        {
            author = null;
            date = null;

            using (var pdfDoc = PdfDocument.Load(filePath))
            {
                title = pdfDoc.Title;
                authors = pdfDoc.Author;
                date = pdfDoc.CreationDate;

                if (string.IsNullOrWhiteSpace(date) == false)
                {
                    var match = Regex.Match(date, "D\\:([0-9]{14})\\+[0-9]{2}'[0-9]{2}'");

                    if (match.Success)
                        if (DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.AssumeUniversal, out var dateTime))
                            date = dateTime.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (string.IsNullOrWhiteSpace(title))
                title = null;
        }

        // TODO: Support playlists
        public References ConfigureSMReferences(References r,
                                                string subtitle = null,
                                                string bookmarks = null)
        {
            string filePath = BinaryMember.GetFilePath("pdf");

            GetInfos(out string pdfTitle,
                     out string author,
                     out string creationDate);

            var title = ConfigureTitle(pdfTitle, subtitle);

            return r.WithTitle(title + (bookmarks != null ? $" -- {bookmarks}" : string.Empty))
                    .WithAuthor(author)
                    .WithDate(creationDate)
                    .WithSource("PDF")
                    .WithLink("..\\" + Svc.SM.Collection.MakeRelative(filePath));
        }
    }
}
