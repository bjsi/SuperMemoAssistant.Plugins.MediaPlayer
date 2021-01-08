using System;
using System.Threading;
using System.Threading.Tasks;
using AdvancedMediaPlayer.Models;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public sealed class MediaPlayerState
    {
        #region Constants & Statics

        public static MediaPlayerState Instance { get; } = new MediaPlayerState();

        #endregion




        #region Properties & Fields - Non-Public

        private SemaphoreSlim ImportSemaphore { get; } = new SemaphoreSlim(1, 1);
        private MpvPlayerWindow PlayerWindow { get; set; }
        private MediaElement LastElement { get; set; }

        #endregion




        #region Constructors

        public MediaPlayerState()
        {
            Config = Svc.Configuration.Load<MediaPlayerCfg>() ?? new MediaPlayerCfg();
        }

        #endregion




        #region Properties & Fields - Public

        public MediaPlayerCfg Config { get; }

        #endregion




        #region Methods

        public void OnElementChanged(IElement newElem,
                                     IControlHtml ctrlHtml)
        {
            PlayerWindow?.CancelSave();

            if (newElem == null)
                return;

            if (LastElement?.ElementId == newElem.Id)
                return;

            var html = ctrlHtml?.Text ?? string.Empty;

            var ivEl = newElem.Type == ElementType.Topic
              ? YouTubeMediaElement.TryReadElement(html, newElem.Id)
              : null;

            bool noNewElem = ivEl == null;
            bool noLastElem = LastElement == null || (Svc.SM.Registry.Element[LastElement.ElementId]?.Deleted ?? true);

            if (noNewElem && noLastElem)
                return;

            bool close = LastElement != null && ivEl == null;

            CloseElement();

            OpenElement(ivEl);

            if (close)
                PlayerWindow?.Close();
        }

        public void ImportYouTubeVideo()
        {
            if (ImportSemaphore.Wait(0) == false)
                return;

            string idOrUrl = ClipBoard.GetText();

            if (!string.IsNullOrWhiteSpace(idOrUrl))
                YouTubeMediaElement.Create(idOrUrl);

            ImportSemaphore.Release();
        }

        public void OpenFile()
        {
            if (ImportSemaphore.Wait(0) == false)
                return;

            string filePath = Importer.OpenFileDialog();

            if (filePath != null)
                Importer.Create(filePath);

            ImportSemaphore.Release();
        }

        private void CloseElement()
        {
            try
            {
                if (LastElement != null && LastElement.IsChanged)
                {
                    // TODO: Display warning + Save to temp file
                    //var res = LastElement.Save();
                }
            }
            finally
            {
                LastElement = null;
            }
        }

        private void OpenElement(MediaElement ivElem)
        {
            if (ivElem == null)
                return;

            LastElement = ivElem;

            CreateVideoWindow(ivElem);
        }

        public Task SaveConfigAsync()
        {
            return Svc.Configuration.SaveAsync(Config);
        }

        private void CreateVideoWindow(MediaElement el)
        {
            PlayerWindow = new MPVWindow(el);
            PlayerWindow.Closed += VideoWindow_Closed;
        }

        private void VideoWindow_Closed(object sender,
                                      EventArgs e)
        {
            PlayerWindow.Closed -= VideoWindow_Closed;
            PlayerWindow = null;
        }

        #endregion
    }
}
