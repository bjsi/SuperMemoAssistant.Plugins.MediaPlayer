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

        // TODO: Required?
        private SynchronizationContext SyncContext { get; set; }
        private SemaphoreSlim ImportSemaphore { get; } = new SemaphoreSlim(1, 1);
        private MPVWindow PlayerWindow { get; set; }
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

            // TODO: Handle both local and yt
            var ivEl = newElem.Type == ElementType.Topic
              ? AMPYouTubeElement.TryReadElement(html, newElem.Id)
              : null;

            bool noNewElem = ivEl == null;
            bool noLastElem = LastElement == null || (Svc.SM.Registry.Element[LastElement.ElementId]?.Deleted ?? true);

            if (noNewElem && noLastElem)
                return;

            // TODO: Required?
            SyncContext.Send(
              delegate
              {
                  bool close = LastElement != null && ivEl == null;

                  CloseElement();

                  OpenElement(ivEl);

                  if (close)
                      PlayerWindow?.Close();
              },
              null);
        }

        public void ImportYouTubeFile()
        {
            // TODO: Required?
            SyncContext.Post(
              _ =>
              {
                  if (ImportSemaphore.Wait(0) == false)
                      return;

                  // TODO: Doesn't need to be defined on the window
                  if (PlayerWindow == null)
                      CreateVideoWindow(null);

                  string idOrUrl = PlayerWindow.YouTubeImportDialog();

                  if (!string.IsNullOrWhiteSpace(idOrUrl))
                      PlayerWindow.Create(idOrUrl);

                  ImportSemaphore.Release();
              },
              null
            );
        }

        public void OpenFile()
        {
            // TODO: Required?
            SyncContext.Post(
              _ =>
              {
                  if (ImportSemaphore.Wait(0) == false)
                      return;

                  if (PlayerWindow == null)
                      CreateVideoWindow(null);

                  // TODO: Doesn't need to be defined on the window
                  string filePath = PlayerWindow.OpenFileDialog();

                  if (filePath != null)
                      PlayerWindow.Create(filePath);

                  ImportSemaphore.Release();
              },
              null
            );
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

            EnsureMediaPlayerWindow();

            PlayerWindow.OpenVideo(ivElem);
        }

        // TODO: Might not be possible to update window position 
        // after creation
        public void UpdateWindowPosition(double top,
                                         double height,
                                         double left,
                                         double width,
                                         ViewMode viewMode)
        {
            Config.WindowTop = top;
            Config.WindowHeight = height;
            Config.WindowLeft = left;
            Config.WindowWidth = width;
            Config.DefaultViewMode = viewMode;
        }

        public Task SaveConfigAsync()
        {
            return Svc.Configuration.SaveAsync(Config);
        }

        // TODO: Required?
        public void CaptureContext()
        {
            SyncContext = new DispatcherSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(SyncContext);

            CreateVideoWindow(null);
        }

        private void EnsureMediaPlayerWindow()
        {
            if (PlayerWindow == null)
                SyncContext.Send(CreateVideoWindow, null);
        }

        private void CreateVideoWindow(object _)
        {
            PlayerWindow = new MPVWindow();
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
