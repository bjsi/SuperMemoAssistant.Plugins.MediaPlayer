namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    //
    // Implements extraction
    public partial class MediaPlayerAPI
    {
        private static bool CreateSMExtract(YouTubeMediaElement element, double priority, References refs, List<ContentBase> contents)
        {
            if (priority < 0 || priority > 100)
                priority = MediaPlayerConst.DefaultExtractPriority;

            var parentElementId = element.parentElementId;
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
                .WithTitle(res.Title + ": {element.StartTime} -> {element.EndTime}")
                .WithPriority(MediaPlayerState.Instance.Config.DefaultExtractPriority)
                .WithReference(
                  r => r.WithTitle(refs.Title)
                        .WithAuthor(refs.Author)
                        .WithDate(refs.Date)
                        .WithSource("YouTube")
                        .WithLink(YTConst.CreateVideoUrl(element.Id, element.StartTime))
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

            int imgRegistryId = Svc.SM.Registry.Image.Add(
                    new ImageWrapper(image),
                    title
                    );

            if (imgRegistryId <= 0)
                return null;

            return new ImageContent(imgRegistryId,
                    Config.ImageStretchType);
        }

        private static ContentBase CreateAudioContent(string base64, string title)
        {
            throw new NotImplementedException();
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public string AudioImageExtract(int expectedId, string audiobase64, string image)
        {

            if (string.IsNullOrWhiteSpace(base64))
                return "Image extract failed - the base64 encoded image was null or whitespace";


            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public string GifExtractRemote(int expectedId, string idOrUrl, double start, double end, string subs)
        {
            var streams = Ydl.GetAVStreamUrls(idOrUrl);
            if (streams == null)
                return "Failed to get AV streams for the video";

            var filepath = Ffmpeg.GifExtract(streams.Video);
            if (!File.Exists(filepath))
                return "Ffmpeg failed to create gif extract";

            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public string AudioExtractRemote(expecdouble start, double end, string subs)
        {
            var streams = Ydl.GetAVStreamUrls(idOrUrl);
            var filepath = Ffmpeg.AudioExtract(streams.Audio);

            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public string VideoExtractRemote(int expectedId, string idOrUrl, double start, double end, string subs)
        {
            var streams = Ydl.GetAVStreamUrls(idOrUrl);
            var filepath = Ffmpeg.VideoExtract(streams, start, end);
            // TODO: Create element
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public string MediaPlayerExtract(int expectedId, double start, double end, string subs)
        {
            if (start > end)
                return null;

            return $"Extract: start: {start} end: {end} subs: {subs}";
        }
    }
}
