namespace SuperMemoAssistant.Plugins.MediaPlayer.UI
{
    public class MpvPlayerWindow
    {

        private Process MpvProcess { get; set; }

        public MpvPlayerWindow(MediaElement element)
        {
            element.ThrowIfArgumentNull("Failed to open Mpv Player Window because media element is null");
        }

        private void BeginMpvProcess(double start, double end)
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
