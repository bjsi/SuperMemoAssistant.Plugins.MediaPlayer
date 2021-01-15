using Anotar.Serilog;
using CliWrap;
using CliWrap.Buffered;
using System;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MediaPlayer.YouTube
{
    public class AVStreams
    {
        public string Video { get; }
        public string Audio { get; }
        public AVStreams(string video, string audio)
        {
            this.Video = video;
            this.Audio = audio;
        }
    }

    public static class Ydl
    {
        public static async Task<AVStreams> GetAVStreamUrlsAsync(string idOrUrl)
        {
            try
            {
                var res = await Cli.Wrap("youtube-dl")
                    .WithArguments($"--youtube-skip-dash-manifest -g {idOrUrl}")
                    .ExecuteBufferedAsync();
                var stdout = res.StandardOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                return new AVStreams(stdout[0], stdout[1]);
            }
            catch (Exception e)
            {
                LogTo.Error($"Failed to get AV stream urls with exception {e}");
            }

            return null;
        }

        public static async Task<string> GetVideoStreamUrlAsync(string idOrUrl)
        {
            var streams = await GetAVStreamUrlsAsync(idOrUrl);
            return streams?.Video;
        }

        public static async Task<string> GetAudioStreamUrlAsync(string idOrUrl)
        {
            var streams = await GetAVStreamUrlsAsync(idOrUrl);
            return streams?.Audio;
        }
    }

    public static class Ffmpeg
    {
        public static async Task<string> VideoExtractAsync(string videoStream, string idOrUrl, double start, double end, string outputFile)
        {
            try
            {
                var res = await Cli.Wrap("ffmpeg")
                    .WithArguments($"-ss {start} -i \"{videoStream}\" -map 0:v -t {end - start} -c:v libx264 {outputFile}")
                    .ExecuteAsync();
            }
            catch (Exception e)
            {
                LogTo.Error($"Ffmpeg video extract failed with exception {e}");
                return null;
            }
            return outputFile;
        }

        public static async Task<string> AudioVideoExtractAsync(AVStreams streams, double start, double end, string outputFile)
        {
            try
            {
                var res = await Cli.Wrap("ffmpeg")
                    .WithArguments($"-ss {start} -i \"{streams.Video}\" -ss {start} -i \"{streams.Audio}\" -map 0:v -map 1:a -t {end - start} -c:v libx264 -c:a aac {outputFile}")
                    .ExecuteAsync();
            }
            catch (Exception e)
            {
                LogTo.Error($"Ffmpeg AV extract failed with exception {e}");
                return null;
            }

            return outputFile;
        }

        public static async Task<string> AudioExtractAsync(string audioStream, double start, double end, string outputFile)
        {
            try
            {
                var res = await Cli.Wrap("ffmpeg")
                    .WithArguments($"-ss {start} -i \"{audioStream}\" -map 0:a -t {end - start} -c:a aac {outputFile}")
                    .ExecuteAsync();
            }
            catch (Exception e)
            {
                LogTo.Error($"Ffmpeg audio extract failed with exception {e}");
                return null;
            }

            return outputFile;
        }

        public static async Task<string> GifExtract(string videoStream, double start, double end, string outputFile)
        {
            try
            {
                var res = await Cli.Wrap("ffmpeg")
                    .WithArguments($"-ss {start} -i \"{videoStream}\" -map 0:v -t {end - start} -r 15 -vf scale=512:-1 {outputFile}")
                    .ExecuteAsync();
            }
            catch (Exception e)
            {
                LogTo.Error($"Ffmpeg gif extract failed with exception {e}");
                return null;
            }
            return outputFile;
        }
    }
}
