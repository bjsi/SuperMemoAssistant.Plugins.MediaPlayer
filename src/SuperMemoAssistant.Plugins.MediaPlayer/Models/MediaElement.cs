using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AdvancedMediaPlayer.Models;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;

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

        public SaveResult Save()
        {
            if (IsChanged == false)
                return SaveResult.Ok;

            if (ElementId <= 0)
                return SaveToBackup();

            try
            {
                bool saveToControl = Svc.SM.UI.ElementWdw.CurrentElementId == ElementId;

                if (saveToControl)
                {
                    IControlHtml ctrlHtml = Svc.SM.UI.ElementWdw.ControlGroup.GetFirstHtmlControl();

                    ctrlHtml.Text = UpdateHtml(ctrlHtml.Text);

                    IsChanged = false;
                }

                else
                {
                    return SaveResult.Fail;

                    /*
                      var elem = Svc.SM.Registry.Element[ElementId];

                      if (elem == null || elem.Deleted)
                        return SaveResult.FailDeleted;

                      var compGroup = elem.ComponentGroup;

                      if (compGroup == null || compGroup.Count == 0)
                        return SaveResult.FailDeleted;

                      var htmlComp = compGroup.GetFirstHtmlComponent();

                      if (htmlComp == null)
                        return SaveResult.FailInvalidComponent;

                      var textMember = htmlComp.Text;

                      if (textMember == null || textMember.Empty)
                        return SaveResult.FailInvalidTextMember;

                      textMember.Value = UpdateHtml(textMember.Value);

                      IsChanged = false;
                    */
                }


                return SaveResult.Ok;
            }
            catch (Exception)
            {
                return SaveToBackup();
            }
        }

        public SaveResult SaveToBackup()
        {
            // TODO: Save to temp file
            // TODO: Set Dirty = false
            return SaveResult.Fail;
        }

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
