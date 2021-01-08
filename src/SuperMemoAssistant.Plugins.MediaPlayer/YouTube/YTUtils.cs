namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class YdlUtils
    {
        public static async Task<JObject> GetYouTubeVideoMetadata(string urlOrId)
        {
            if (string.IsNullOrWhiteSpace(urlOrId))
                return null;

            var result = await Cli.Wrap("youtube-dl")
                .WithArguments($"--no-playlist -j --playlist-items 0 {urlOrId}")
                .ExecuteBufferedAsync();

            dynamic jObj = JObject.Parse(result.StandardOutput);
            try
            {
                return (string)jObj["extractor"] == "youtube"
                        ? jObj
                        : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
