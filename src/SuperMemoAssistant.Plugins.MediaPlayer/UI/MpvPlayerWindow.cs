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
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;
        private static JsonRpcServer Server => Svc<MediaPlayerPlugin>.Plugin.JsonRpcServer;
        public MediaPlayerAPI API { get; }

        public MpvPlayerWindow(YouTubeMediaElement element, int elementId)
        {
            element.ThrowIfArgumentNull("Failed to open Mpv Player Window because media element is null");
            Element = element;
            API = new MediaPlayerAPI(element, elementId);
            Server.RegisterService(API);
            BeginMpvProcess();
        }

        private void BeginMpvProcess()
        {
            var luaDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mpvacious");
            var scriptPath = Path.Combine(luaDir, "subs2srs.lua");
            if (!File.Exists(scriptPath))
            {
                LogTo.Debug("MediaPlayer MPV lua script file does not exist");
            }

            var args = new string[]
            {
                $"--start={Element.WatchPoint}",
                $"--end={Element.EndTime}",
                $"--loop-file=inf",
                $"--speed={Element.DefaultPlaybackRate}",
                $"--geometry={Config.WindowWidth}x{Config.WindowHeight}+{Config.WindowLeft}+{Config.WindowTop}",
                $"--script-opts=expected_id={Element.ElementId}",
                $"--script={scriptPath}",
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
            Server.RevokeService();
            if (MpvProcess != null && !MpvProcess.HasExited)
                MpvProcess?.CloseMainWindow();
        }
    }
}
