namespace SuperMemoAssistant.Plugins.MediaPlayer.UI
{
    public class MpvPlayerWindow
    {

        private Process MpvProcess { get; set; }
        private YouTubeMediaElement Element { get; }
        private MpvPlayerAPI API => Svc<MediaPlayer>.Plugin.API;
        private MediaPlayerCfg Config => Svc<MediaPlayer>.Plugin.Config;

        public MpvPlayerWindow(YouTubeMediaElement element)
        {
            element.ThrowIfArgumentNull("Failed to open Mpv Player Window because media element is null");
            Element = element;
            BeginMpvProcess();
        }

        private void BeginMpvProcess()
        {
            MpvProcess = new Process
            {
                StartInfo =
                {
                    FileName        = "mpv",
                    // TODO end?
                    // TODO geometry
                    Arguments       = $"--start={Element.Start} {Element.Url}",
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
            Closed?.Invoke();
        }

        public event EventHandler Closed;
    }
}
