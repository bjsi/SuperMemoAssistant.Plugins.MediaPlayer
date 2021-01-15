using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Services;
using System;
using System.Diagnostics;

namespace SuperMemoAssistant.Plugins.MediaPlayer.UI
{
    public class MpvPlayerWindow
    {

        private Process MpvProcess { get; set; }
        private YouTubeMediaElement Element { get; }
        public MediaPlayerAPI API => Svc<MediaPlayerPlugin>.Plugin.API;
        private MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;

        public MpvPlayerWindow(YouTubeMediaElement element)
        {
            element.ThrowIfArgumentNull("Failed to open Mpv Player Window because media element is null");
            Element = element;
            BeginMpvProcess();
        }

        private void BeginMpvProcess()
        {
            var args = new string[]
            {
                $"--start={Element.StartTime}",
                $"--geometry={Config.WindowWidth}x{Config.WindowHeight}+{Config.WindowLeft}+{Config.WindowTop}",
                $"--script-opts=elementid={Element.ElementId}",
                $"{Element.Url}"
            };

            MpvProcess = new Process
            {
                StartInfo =
                {
                    FileName        = "mpv",
                    Arguments       = string.Join(" ", args),
                    UseShellExecute = false,
                    CreateNoWindow  = false, // TODO: test
                }
            };

            MpvProcess.Start();

            /* if (workingDirectory != null) */
            /*     p.StartInfo.WorkingDirectory = workingDirectory; */

        }

        public void Close()
        {
            MpvProcess.CloseMainWindow(); // TODO: Test if this is enough
        }
    }
}
