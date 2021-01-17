using Anotar.Serilog;
using AustinHarris.JsonRpc;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Plugins.MediaPlayer.Helpers;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Plugins.MediaPlayer.YouTube;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    //
    // Implements extraction
    public partial class MediaPlayerAPI
    {
        private static Image CreateImage(string b64)
        {
            if (string.IsNullOrWhiteSpace(b64))
                return null;

            byte[] bytes = Convert.FromBase64String(b64);

            using (MemoryStream ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }
        
        [JsonRpcMethod]
        [LockValidateExecute]
        public ExtractResult ImageExtract(int expectedId, string base64, string subs)
        {

            if (string.IsNullOrWhiteSpace(base64))
                return new ExtractResult(false, "Failed to create image extract because base 64 data was null or empty");

            subs = subs ?? string.Empty;
            byte[] bytes = Convert.FromBase64String(base64);

            try
            {
                List<ContentBase> contents = new List<ContentBase>();
                contents.Add(new TextContent(true, subs));

                var htmlCtrl = ContentEx.GetFirstHtmlCtrl();
                var refs = htmlCtrl?.GetReferences();
                string title = refs?.Title ?? CurrentMediaElement.Id;

                var image = CreateImage(base64);
                var imgContent = ContentEx.CreateImageContent(image, title);
                if (imgContent != null)
                    contents.Add(imgContent);

                var priority = MediaPlayerState.Instance.Config.DefaultExtractPriority;
                var res = ContentEx.CreateSMElement(expectedId, priority, contents, refs, false);
                return new ExtractResult(res == CreationResult.Ok, res.Name());
            }
            catch (Exception)
            {
                return new ExtractResult(false, "Failed to create image extract");
            }
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public ExtractResult GifExtract(int expectedId, string base64, string subs)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return new ExtractResult(false, "Failed to create image extract because base 64 data was null or empty");

            byte[] bytes = Convert.FromBase64String(base64);

            try
            {
                var htmlCtrl = ContentEx.GetFirstHtmlCtrl();
                var refs = htmlCtrl?.GetReferences();
                string filename = $"{refs?.Title ?? CurrentMediaElement.Id}.gif";
                string filepath = MediaPlayerFilesystem.ImageDir.CombineFile(filename).FullPath;
                File.WriteAllBytes(filepath, bytes);

                List<ContentBase> contents = new List<ContentBase>();
                subs = subs ?? string.Empty;
                subs += "\r\n";
                subs += $"<img href='{filepath}'>";
                contents.Add(new TextContent(true, subs));

                refs.Title += ": Gif Extract";
                var priority = MediaPlayerState.Instance.Config.DefaultExtractPriority;
                var res = ContentEx.CreateSMElement(expectedId, priority, contents, refs, false);
                return new ExtractResult(res == CreationResult.Ok, res.Name());
            }
            catch (Exception e)
            {
                return new ExtractResult(false, $"Failed to create image extract with exception {e}");
            }
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public async Task<ExtractResult> GifExtractRemote(double start, double end, string subs)
        {
            var streams = await Ydl.GetAVStreamUrlsAsync(CurrentMediaElement.Id);
            if (streams == null)
                return new ExtractResult(false, "Youtube-dl failed to get av streams");

            var htmlCtrl = ContentEx.GetFirstHtmlCtrl();
            var refs = htmlCtrl?.GetReferences();

            var filename = $"{refs?.Title ?? CurrentMediaElement.Id}-{start}-{end}";
            var filepath = MediaPlayerFilesystem.ImageDir.CombineFile(filename).FullPath;
            if (!await Ffmpeg.GifExtract(streams.Video, start, end, filepath))
                return new ExtractResult(false, "Ffmpeg failed to create gif extract");

            List<ContentBase> contents = new List<ContentBase>();
            var htmlContent = subs ?? string.Empty;
            htmlContent += "\r\n";
            htmlContent += $"<img href='{filepath}'>";
            var mainHtml = new TextContent(true, htmlContent);
            contents.Add(mainHtml);

            refs.Title += $": Gif Extract - {start} -> {end}"; // TODO: Convert to human readable
            var priority = MediaPlayerState.Instance.Config.DefaultExtractPriority;
            var res = ContentEx.CreateSMElement(ExpectedElementId, priority, contents, refs, false);
            return new ExtractResult(res == CreationResult.Ok, res.Name());
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public ExtractResult MediaPlayerExtract(double start, double end)
        {
            if (start >= end)
            {
                var message = "Start time cannot be greater than or equal to end time";
                LogTo.Debug("Failed Media Player Extract because " + message);
                return new ExtractResult(false, message);
            }

            LogTo.Debug("Executing Media Player Extract");
            var res = CurrentMediaElement.Create(ExpectedElementId, start, end, start, ViewMode.Normal, false);
            return new ExtractResult(res == CreationResult.Ok, res.Name());
        }
    }
}
