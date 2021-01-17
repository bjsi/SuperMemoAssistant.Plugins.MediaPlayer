using System;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    [Serializable]
    public class ExtractResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ExtractResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }
    }
}
