using System.ComponentModel;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{

    /// <summary>
    ///   Main configuration file for the Media Player plugin. Some values
    ///   can be overriden by <see cref="MediaElement" />
    /// </summary>
    [Form(Mode = DefaultFields.None)]
    [Title("Media Player Settings",
            IsVisible = "{Env DialogHostContext}")]
    [DialogAction("cancel",
                  "Cancel",
                  IsCancel = true)]
    [DialogAction("save",
                  "Save",
                  IsDefault = true,
                  Validates = true)]
    public class MediaPlayerCfg : CfgBase<MediaPlayerCfg>, INotifyPropertyChangedEx
    {

        [Field(Name = "YouTube Subtitle Languages (IETF language tags separated  by commas)")]
        public string SubtitleLanguages { get; set; } = "en";

        [Field(Name = "Default YouTube Video Quality (youtube-dl format)")]
        public string YouTubeQuality { get; set; } = "bestvideo[height<=?720][fps<=?30][vcodec!=?vp9]+bestaudio/best";

        [Field(Name = "Include YouTube Thumbnail?")]
        public bool IncludeYouTubeThumbnail { get; set; } = true;

        [Field(Name = "Media Player Host")]
        public string Host { get; set; } = "0.0.0.0";

        [Field(Name = "Media Player Port")]
        public int Port { get; set; } = 9898;

        [Field(Name = "Default Extract Priority (%)")]
        [Value(Must.BeGreaterThanOrEqualTo,
               0,
               StrictValidation = true)]
        [Value(Must.BeLessThanOrEqualTo,
               100,
               StrictValidation = true)]
        public double DefaultExtractPriority { get; set; } = MediaPlayerConst.DefaultExtractPriority;

        [Field(Name = "Default Forced Schedule Interval (days)")]
        [Value(Must.BeGreaterThanOrEqualTo,
               1,
               StrictValidation = true)]
        [Value(Must.BeLessThanOrEqualTo,
               0xFFF,
               StrictValidation = true)]
        public int LearnForcedScheduleInterval { get; set; } = 1;

        [Field(Name = "Default Image Stretch Type")]
        [SelectFrom(typeof(ImageStretchMode),
                    SelectionType = SelectionType.RadioButtonsInline)]
        public ImageStretchMode ImageStretchType { get; set; } = ImageStretchMode.Proportional;

        [Field(Name = "Default view mode")]
        [SelectFrom(typeof(ViewMode),
                    SelectionType = SelectionType.ComboBox)]
        public ViewMode DefaultViewMode { get; set; } = MediaPlayerConst.DefaultViewMode;

        public double WindowTop { get; set; } = 100;
        public double WindowHeight { get; set; } = 800;
        public double WindowLeft { get; set; } = 200;
        public double WindowWidth { get; set; } = 1400;

        #region Methods Impl

        public override string ToString()
        {
            return "Media Player";
        }

        [JsonIgnore]
        public bool IsChanged { get; set; }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
