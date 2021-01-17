using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Forge.Forms;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Plugins.MediaPlayer.UI;
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
        public MpvPlayerWindow PlayerWindow { get; private set; }
        private MediaElement LastElement { get; set; }

        #endregion

    
        private async Task LoadConfigAsync()
        {
            Config = await Svc.Configuration.Load<MediaPlayerCfg>() ?? new MediaPlayerCfg();
        }


        #region Constructors

        public MediaPlayerState()
        {
            LoadConfigAsync();
        }

        #endregion




        #region Properties & Fields - Public

        public MediaPlayerCfg Config { get; set; }

        #endregion




        #region Methods

        public void OnElementChanged(IElement newElem,
                                     IControlHtml ctrlHtml)
        {
            // PlayerWindow?.CancelSave();

            if (newElem == null)
                return;

            if (LastElement?.ElementId == newElem.Id)
                return;

            var html = ctrlHtml?.Text ?? string.Empty;

            var ytEl = newElem.Type == ElementType.Topic
              ? YouTubeMediaElement.TryReadElement(html, newElem.Id)
              : null;

            bool noNewElem = ytEl == null;
            bool noLastElem = LastElement == null || (Svc.SM.Registry.Element[LastElement.ElementId]?.Deleted ?? true);

            if (noNewElem && noLastElem)
                return;

            bool close = LastElement != null && ytEl == null;

            CloseElement();

            OpenElement(ytEl, newElem.Id);

            if (close)
                PlayerWindow?.Close();
        }

        public async void ImportYouTubeVideo()
        {
            if (await ImportSemaphore.WaitAsync(0) == false)
                return;

            var res = await Application.Current.Dispatcher.Invoke(() =>
            {
                return Show.Dialog().For(new Prompt<string> { Message = "YouTube Id or Url:" });
            });

            string idOrUrl = res.Model.Value;

            if (!string.IsNullOrWhiteSpace(idOrUrl))
                await YouTubeMediaElement.Create(idOrUrl);

            ImportSemaphore.Release();
        }

        public void OpenFile()
        {
            if (ImportSemaphore.Wait(0) == false)
                return;

            string filePath = Importer.OpenFileDialog();

            if (filePath != null)
            {
                // TODO:
                // Importer.Create(filePath);
            }

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

        private void OpenElement(YouTubeMediaElement ivElem, int elementId)
        {
            if (ivElem == null)
                return;

            LastElement = ivElem;

            CreateVideoWindow(ivElem, elementId);
        }

        public void SaveConfig()
        {
            Svc.Configuration.Save(Config);
        }

        private void CreateVideoWindow(YouTubeMediaElement el, int elementId)
        {
            PlayerWindow = new MpvPlayerWindow(el, elementId);
            PlayerWindow.API.Closed += VideoWindow_Closed;
        }

        private void VideoWindow_Closed(object sender,
                                      EventArgs e)
        {
            PlayerWindow.API.Closed -= VideoWindow_Closed;
            PlayerWindow = null;
        }

        #endregion
    }
}
