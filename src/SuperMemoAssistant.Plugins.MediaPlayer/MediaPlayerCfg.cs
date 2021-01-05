using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Services;
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

        // TODO: Create sane defaults
        public double WindowTop { get; set; } = 100;
        public double WindowHeight { get; set; } = 600;
        public double WindowLeft { get; set; } = 100;
        public double WindowWidth { get; set; } = 800;

        #region Methods Impl

        public override string ToString()
        {
            return "Media Player";
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
