using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.Remoting;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MediaPlayerPlugin : SentrySMAPluginBase<MediaPlayerPlugin>
    {
        #region Constructors

        // public MediaPlayerPlugin() : base("") { }

        #endregion







        #region Properties Impl - Public

        /// <inheritdoc />
        public override string Name => "MediaPlayerPlugin";

        public override bool HasSettings => true;

        #endregion




        #region Methods Impl

        /// <inheritdoc />
        protected override void OnPluginInitialized()
        {
            // TODO: Required?
            MediaPlayerState.Instance.CaptureContext();
            base.OnPluginInitialized();
        }

        /// <inheritdoc />
        protected override void OnSMStarted(bool wasSMAlreadyStarted)
        {
            Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(OnElementChanged);

            base.OnSMStarted(wasSMAlreadyStarted);
        }

        /// <inheritdoc />
        public override void ShowSettings()
        {
            ConfigurationWindow.ShowAndActivate(null, HotKeyManager.Instance, MediaPlayerState.Instance.Config);
        }

        #endregion




        #region Methods

        [LogToErrorOnException]
        public static void OnElementChanged(SMDisplayedElementChangedEventArgs e)
        {
            try
            {
                IControlHtml ctrlHtml = Svc.SM.UI.ElementWdw.ControlGroup.GetFirstHtmlControl();

                MediaPlayerState.Instance.OnElementChanged(e.NewElement, ctrlHtml);
            }
            catch (RemotingException) { }
        }

        #endregion

    }
}
