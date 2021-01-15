using System.ComponentModel;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using PropertyChanged;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{
    [Form(Mode = DefaultFields.None)]
    public abstract class MediaElement : INotifyPropertyChanged
    {
        public MediaElement()
        {
            StartTime = -1;
            EndTime = -1;
            WatchPoint = -1;
        }

        #region Properties & Fields - Public


        [JsonProperty(PropertyName = "ST")]
        public double StartTime { get; set; }

        [JsonProperty(PropertyName = "ET")]
        public double EndTime { get; set; }

        [JsonProperty(PropertyName = "WPt")]
        public double WatchPoint { get; set; }

        [Field(Name = "Default Playback Rate")]
        [Value(Must.BeGreaterThan,
               0,
               StrictValidation = true)]
        public double DefaultPlaybackRate { get; set; } = 1.0;

        [Field(Name = "Default Extract Priority (%)")]
        [Value(Must.BeGreaterThanOrEqualTo,
               0,
               StrictValidation = true)]
        [Value(Must.BeLessThanOrEqualTo,
               100,
               StrictValidation = true)]
        public double DefaultExtractPriority { get; set; }

        [JsonProperty(PropertyName = "VM")]
        public ViewMode ViewMode { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        public int ElementId { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        public bool IsChanged { get; set; }

        [JsonIgnore]
        [DoNotNotify]
        // TODO: && EndTime < Duration
        public bool IsFullVideo => StartTime < 0;

        #endregion 

        protected string GetJsonB64()
        {
            string elementJson = JsonConvert.SerializeObject(this,
                                                             Formatting.None);

            return elementJson.ToBase64();
        }


        #region Events

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }

    #region Enums

    public enum CreationResult
    {
        Ok,
        FailUnknown,
        FailCannotCreateElement,
        // TODO: Not using video registry
        FailVideoRegistryInsertionFailed,
        FailVideoMemberFileMissing
    }

    public enum SaveResult
    {
        Ok = 0,
        FailWithBackup = 1,
        Fail = 2,
        FailDeleted,
        FailInvalidComponent,
        FailInvalidTextMember
    }

    #endregion
}
