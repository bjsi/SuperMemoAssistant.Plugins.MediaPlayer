using Anotar.Serilog;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using SuperMemoAssistant.Plugins.MediaPlayer.Helpers;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Services;
using System;
using System.Diagnostics;
using System.IO;

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
            var luaDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lua");
            var scriptPath = Path.Combine(luaDir, "subs2srs.lua");
            if (!File.Exists(scriptPath))
            {
                LogTo.Debug("MediaPlayer MPV lua script file does not exist");
            }

            var args = new string[]
            {
                $"--start={Element.WatchPoint}",
                $"--end={Element.EndTime}",
                $"--speed={Element.DefaultPlaybackRate}",
                $"--geometry={Config.WindowWidth}x{Config.WindowHeight}+{Config.WindowLeft}+{Config.WindowTop}",
                $"--script-opts=expected_id={Element.ElementId}",
                $"--script={scriptPath}",
                $"--loop-file=inf",
                $"--ontop",
                $"--ytdl-format={Config.YouTubeQuality}",
                $"{Element.Url}"
            };

            MpvProcess = new Process
            {
                StartInfo =
                {
                    FileName        = "mpv",
                    Arguments       = string.Join(" ", args),
                    UseShellExecute = false,
                    CreateNoWindow  = false,
                    WorkingDirectory = luaDir
                }
            };

            MpvProcess.Start();
        }

        public void Close()
        {
            if (MpvProcess != null && !MpvProcess.HasExited)
            {
                MpvProcess?.CloseMainWindow();
            }
        }
    }
}
