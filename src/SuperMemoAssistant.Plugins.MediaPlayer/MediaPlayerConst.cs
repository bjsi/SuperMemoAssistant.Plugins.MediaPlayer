using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using System.Text.RegularExpressions;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    internal static class MediaPlayerConst
    {

        #region Constants & Statics
        // HTML snippets here are formatted in the final form they should assume in SM.

        //
        // Elements with locally saved video data
        public const string LocalElementFormat = @"<DIV id=media-player-local-element-title>{0}</DIV>
<DIV id=media-player-local-element-filename>{1}</DIV>
<DIV id=media-player-local-element-data>{2}</DIV>";
        public const string LocalElementDataFormat = "<DIV id=media-player-local-element-data>{0}</DIV>";

        public static readonly Regex RE_LocalElement = new Regex("<DIV id=media-player-local-element-data>([^<]+)</DIV>", RegexOptions.IgnoreCase);

        //
        // Elements with data from youtube 
        public const string YouTubeElementFormat = @"<DIV id=media-player-yt-element-title>{0}</DIV>
<DIV id=media-player-yt-element-filename>{1}</DIV>
<DIV id=media-player-yt-element-data>{2}</DIV>";
        public const string YouTubeElementDataFormat = "<DIV id=media-player-yt-element-data>{0}</DIV>";

        public static readonly Regex RE_YouTubeElement = new Regex("<DIV id=media-player-yt-element-data>([^<]+)</DIV>", RegexOptions.IgnoreCase);

        public const double DefaultExtractPriority = 15.0;

        public const ViewMode DefaultViewMode = ViewMode.Normal;
        #endregion
    }
}
