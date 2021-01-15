using System;
using System.IO;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class Extractor
    {
        public static void WriteBase64MusicToFile(string b64, string filePath)
        {
            byte[] bytes = Convert.FromBase64String(b64);
            File.WriteAllBytes(filePath, bytes);
        }

        public static Image Base64ToImage(string b64)
        {
            if (string.IsNullOrWhiteSpace(b64))
                return null;

            byte[] bytes = Convert.FromBase64String(b64);

            using (MemoryStream ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }

        public static ContentBase CreateImageContent(Image image, string title)
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

        public static bool CreateSMExtract(YouTubeMediaElement element, double priority, References refs, List<ContentBase> contents)
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
    }
}
