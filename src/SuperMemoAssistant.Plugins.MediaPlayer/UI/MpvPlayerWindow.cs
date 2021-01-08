namespace SuperMemoAssistant.Plugins.MediaPlayer.UI
{
    public class MpvPlayerWindow
    {

        private Process MpvProcess { get; set; }
        private YouTubeMediaElement Element { get; }

        public MpvPlayerWindow(YouTubeMediaElement element)
        {
            element.ThrowIfArgumentNull("Failed to open Mpv Player Window because media element is null");
            Element = element;
            BeginMpvProcess();
        }

        private void BeginMpvProcess()
        {
        }

        public void Close()
        {
            MpvProcess.CloseMainWindow();
            OnClosing?.Invoke();
        }

        public event EventHandler OnClosing;
    }
}
