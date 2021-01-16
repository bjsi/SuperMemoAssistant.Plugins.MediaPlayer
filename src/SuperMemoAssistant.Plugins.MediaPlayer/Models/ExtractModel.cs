using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MediaPlayer.Models
{
    public class Data
    {
    }

    public class TextData
    {
        public string Value { get; set; }
        public bool IsHtml { get; set; }
    }

    public class Base64Data : Data
    {
        public string Data { get; set; }
        public string Extension { get; set; }
    }

    public class URLData : Data
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class MpvAudio
    {
        Data Data { get; set; }
    }

    public class MpvImage
    {
        Data ImageData { get; set; }
    }

    public class MpvSubs
    {
        public TextData Text { get; set; }
    }

    public class MpvExtract
    {
        public List<MpvAudio> Audio { get; set; }
    }
}
