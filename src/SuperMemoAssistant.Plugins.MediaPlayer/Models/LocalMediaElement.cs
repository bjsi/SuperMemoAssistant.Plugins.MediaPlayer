using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedMediaPlayer.Models;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{

    // TODO: Big todo: Don't use the Video Registry
    public class LocalMediaElement : MediaElement
    {
        #region Constructors

        // TODO: Subtitles
        public LocalMediaElement()
        {
            VideoMemberId = -1;
        }

        #endregion




        #region Properties & Fields - Public

        [JsonProperty(PropertyName = "BM")]
        public int VideoMemberId { get; set; }

        // TODO: Should be JsonIgnore?
        [JsonProperty(PropertyName = "SF")]
        public string SubtitlesFilePath { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        public string FilePath { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        public IVideo VideoMember => Svc.SM.Registry.Video?[VideoMemberId];

        #endregion




        #region Methods

        public static CreationResult Create(
          string filePath,
          double startTime = -1,
          double endTime = -1,
          int parentElementId = -1,
          double watchPoint = default,
          ViewMode viewMode = AMPConst.DefaultViewMode,
          bool shouldDisplay = true)
        {
            IVideo vidMem = null;

            try
            {
                var fileName = Path.GetFileName(filePath);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    LogTo.Warning($"Path.GetFileName(filePath) returned null for filePath '{filePath}'.");
                    return CreationResult.FailUnknown;
                }

                var vidMems = Svc.SM.Registry.Video.FindByName(
                  new Regex(Regex.Escape(fileName) + ".*", RegexOptions.IgnoreCase)).ToList();

                if (vidMems.Any())
                {
                    var oriVideoFileInfo = new FileInfo(filePath);

                    if (oriVideoFileInfo.Exists == false)
                    {
                        LogTo.Warning($"New video file '{filePath}' doesn't exist.");
                        return CreationResult.FailUnknown;
                    }

                    foreach (var x in vidMems)
                    {
                        var smVidFilePath = x.GetFilePath();
                        var smVidFileInfo = new FileInfo(smVidFilePath);

                        if (smVidFileInfo.Exists == false)
                        {
                            LogTo.Warning($"Video file '{smVidFilePath}' associated with Video member id {x.Id} is missing.");
                            continue;
                        }

                        try
                        {
                            if (smVidFileInfo.Length != oriVideoFileInfo.Length)
                                continue;
                        }
                        catch (FileNotFoundException ex)
                        {
                            LogTo.Warning(ex, $"Video file '{smVidFilePath}' or '{filePath}' has gone missing. Weird.");
                            continue;
                        }

                        vidMem = x;
                        break;
                    }
                }

                if (vidMem == null)
                {
                    int vidMemId = Svc.SM.Registry.Video.Add(filePath, fileName);

                    if (vidMemId < 0)
                        return CreationResult.FailVideoRegistryInsertionFailed;

                    vidMem = Svc.SM.Registry.Video[vidMemId];
                }
            }
            catch (RemotingException)
            {
                return CreationResult.FailUnknown;
            }
            catch (Exception ex)
            {
                LogTo.Error(ex, "Exception thrown while creating new Video element");
                return CreationResult.FailUnknown;
            }

            return Create(vidMem,
                          startTime,
                          endTime,
                          parentElementId,
                          watchPoint,
                          viewMode,
                          shouldDisplay);
        }

        public static CreationResult Create(
          IVideo vidMem,
          double startTime = -1,
          double endTime = -1,
          int parentElementId = -1,
          double watchPoint = -1,
          ViewMode viewMode = AMPConst.DefaultViewMode,
          bool shouldDisplay = true)
        {
            AMPLocalElement vidEl;

            // TODO: Metadata
            string title;

            try
            {
                // TODO: Filepaths
                filePath = vidMem.GetFilePath();

                if (File.Exists(filePath) == false)
                    return CreationResult.FailVideoMemberFileMissing;

                vidEl = new AMPLocalElement
                {
                    VideoMemberId = vidMem.Id,
                    FilePath = filePath,
                    StartTime = startTime,
                    EndTime = endTime,
                    WatchPoint = watchPoint,
                    ViewMode = viewMode,
                };

                // TODO
                vidEl.GetInfos(out title, out _, out _);
            }
            catch (Exception ex)
            {
                LogTo.Error(ex, "Exception thrown while creating new advanced media player element");
                return CreationResult.FailUnknown;
            }

            string elementHtml = string.Format(CultureInfo.InvariantCulture,
                                               AMPConst.LocalElementFormat,
                                               title,
                                               vidMem.Name,
                                               vidEl.GetJsonB64());

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
                        .WithSource("Advanced Media Player")
                        .WithLink("..\\" + Svc.SM.Collection.MakeRelative(filePath))
                );

            if (shouldDisplay == false)
                elemBuilder = elemBuilder.DoNotDisplay();

            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder)
              ? CreationResult.Ok
              : CreationResult.FailCannotCreateElement;
        }

        public static LocalMediaElement TryReadElement(string elText,
                                                int elementId)
        {
            if (string.IsNullOrWhiteSpace(elText))
                return null;

            var reRes = MediaPlayerConst.RE_LocalElement.Match(elText);

            if (reRes.Success == false)
                return null;

            try
            {
                string toDeserialize = reRes.Groups[1].Value.FromBase64();

                var vidEl = JsonConvert.DeserializeObject<LocalMediaElement>(toDeserialize);

                if (vidEl != null) // && elementId > 0)
                {
                    vidEl.ElementId = elementId;
                    // TODO: FilePaths
                    vidEl.FilePath = vidEl.VideoMember.GetFilePath();

                    // TODO: Remove element Id test when better element transition is implemented
                    // Double check
                    if (Svc.SM.UI.ElementWdw.CurrentElementId != elementId)
                        return null;

                    if (File.Exists(vidEl.FilePath) == false)
                    {
                        var pdfDirPath = Svc.SM.Collection.CombinePath(Path.GetDirectoryName(vidEl.FilePath));

                        $"The video file is missing.\r\nFilename: {Path.GetFileName(vidEl.FilePath)}".ShowDesktopNotification(
                          new ToastButton("Open containing folder", pdfDirPath)
                          {
                              ActivationType = ToastActivationType.Protocol
                          }
                        );

                        return null;
                    }
                }

                return vidEl;
            }
            catch
            {
                return null;
            }
        }

        private string UpdateHtml(string html)
        {
            string newElementDataDiv = string.Format(CultureInfo.InvariantCulture,
                                                     AMPConst.LocalElementDataFormat,
                                                     GetJsonB64());

            return MediaPlayerConst.RE_LocalElement.Replace(html,
                                               newElementDataDiv);
        }

        public static void GetInfos(string filePath,
                                    out string title,
                                    out string authors,
                                    out string date)
        {
            // TODO
            title = null;
            authors = null;
            date = null;
        }

        public void GetInfos(out string title,
                             out string authors,
                             out string date)
        {
            GetInfos(FilePath,
                     out title,
                     out authors,
                     out date);

            title = title ?? VideoMember.Name;

            if (StartTime >= 0 && EndTime >= 0)
                // TODO make human readable
                title += $" ({StartTime} -> {EndTime})";
        }

        public References ConfigureSMReferences(References r,
                                                string subtitle = null,
                                                string bookmarks = null)
        {
            // TODO
            string filePath = VideoMember.GetFilePath();

            GetInfos(out string title,
                     out _,
                     out _);


            return r.WithTitle(title + (bookmarks != null ? $" -- {bookmarks}" : string.Empty))
                    .WithSource("Advanced Media Player")
                    .WithLink("..\\" + Svc.SM.Collection.MakeRelative(filePath));
        }

        #endregion


    }
}
