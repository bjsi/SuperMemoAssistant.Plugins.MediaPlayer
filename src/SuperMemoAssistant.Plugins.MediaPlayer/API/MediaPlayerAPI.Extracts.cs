using AustinHarris.JsonRpc;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Plugins.MediaPlayer.YouTube;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using References = SuperMemoAssistant.Plugins.MediaPlayer.Helpers.References;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    //
    // Implements extraction
    public partial class MediaPlayerAPI
    {
        private static bool CreateSMExtract(YouTubeMediaElement element, double priority, References refs, List<ContentBase> contents)
        {
            if (priority < 0 || priority > 100)
                priority = MediaPlayerConst.DefaultExtractPriority;

            var parentElementId = element.ElementId;
            IElement parentElement =
              parentElementId > 0
                ? Svc.SM.Registry.Element[parentElementId]
                : null;

            if (parentElement == null)
                return false;

            var elemBuilder =
              new ElementBuilder(ElementType.Topic,
                                 contents.ToArray())
                .WithParent(parentElement)
                .WithTitle(refs.Title + ": {element.StartTime} -> {element.EndTime}")
                .WithPriority(MediaPlayerState.Instance.Config.DefaultExtractPriority)
                .WithReference(
                  r => r.WithTitle(refs.Title)
                        .WithAuthor(refs.Author)
                        .WithDate(refs.Date)
                        .WithSource("YouTube")
                        .WithLink(element.Url)
                );

            elemBuilder.DoNotDisplay();
            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder);
        }

        private static Image CreateImage(string b64)
        {
            if (string.IsNullOrWhiteSpace(b64))
                return null;

            byte[] bytes = Convert.FromBase64String(b64);

            using (MemoryStream ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }

        private static ContentBase CreateImageContent(Image image, string title)
        {
            if (image == null)
                return null;

            int imgRegistryId = Svc.SM.Registry.Image.AddMember(
                    new ImageWrapper(image),
                    title
                    );

            if (imgRegistryId <= 0)
                return null;

            return new ImageContent(imgRegistryId, Config.ImageStretchType);
        }

        private static ContentBase CreateAudioContent(string base64, string title)
        {
            throw new NotImplementedException();
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public static string AudioImageExtract(int expectedId, string audiobase64, string image)
        {

            if (string.IsNullOrWhiteSpace(audiobase64))
                return "Image extract failed - the base64 encoded image was null or whitespace";

            throw new NotImplementedException();

            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public static async Task<string> GifExtractRemote(int expectedId, string idOrUrl, double start, double end, string subs)
        {
            var streams = await Ydl.GetAVStreamUrlsAsync(idOrUrl);
            if (streams == null)
                return "Failed to get AV streams for the video";

            var filepath = await Ffmpeg.GifExtract(streams.Video, start, end, "outputfile.gif");
            if (!File.Exists(filepath))
                return "Ffmpeg failed to create gif extract";

            throw new NotImplementedException();
            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public static async Task<string> AudioExtractRemote(int expectedElementId, string idOrUrl, double start, double end, string subs)
        {
            var streams = await Ydl.GetAVStreamUrlsAsync(idOrUrl);
            var filepath = await Ffmpeg.AudioExtractAsync(streams.Audio, start, end, "outputfile.?");

            // TODO: Create element
            throw new NotImplementedException();
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public static async Task<string> VideoExtractRemote(int expectedId, string idOrUrl, double start, double end, string subs)
        {
            var streams = await Ydl.GetAVStreamUrlsAsync(idOrUrl);
            var filepath = await Ffmpeg.VideoExtractAsync(streams.Video, idOrUrl, start, end, "outputfile.?");
            // TODO: Create element
            throw new NotImplementedException();
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public static string MediaPlayerExtract(int expectedId, double start, double end, string subs)
        {
            if (start > end)
                return null;

            return $"Extract: start: {start} end: {end} subs: {subs}";
            throw new NotImplementedException();
        }
    }
}
