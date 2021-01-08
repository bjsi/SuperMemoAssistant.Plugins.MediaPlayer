using Newtonsoft.Json;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{
    public class MediaExtract
    {
        #region Properties & Fields - Public

        [JsonProperty(PropertyName = "ET")]
        public double EndTime { get; set; }

        [JsonProperty(PropertyName = "ST")]
        public int StartTime { get; set; }

        #endregion




        #region Methods

        public bool IsExtractValid()
        {
            return (StartTime >= 0) && (EndTime >= 0) && (EndTime >= StartTime);
        }

        public static implicit operator MediaExtract(IVExtractInfo extractInfo)
        {
            return new MediaExtract
            {
                StartTime = extractInfo.StartTime,
                EndTime = extractInfo.EndTime
            };
        }

        #endregion
    }
}
