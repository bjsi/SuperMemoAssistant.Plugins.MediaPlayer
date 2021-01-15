using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    public partial class MediaPlayerAPI
    {
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;
    }
}
