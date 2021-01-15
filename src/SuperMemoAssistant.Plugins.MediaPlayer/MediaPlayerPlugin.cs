using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Remoting;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MediaPlayerPlugin : SentrySMAPluginBase<MediaPlayerPlugin>
    {
        #region Constructors

        public MediaPlayerPlugin() : base("") { }

        #endregion







        #region Properties Impl - Public

        /// <inheritdoc />
        public override string Name => "MediaPlayerPlugin";
        public override bool HasSettings => true;

        public MediaPlayerAPI API { get; } = new MediaPlayerAPI();
        public MediaPlayerCfg Config { get; }

        #endregion


        #region Methods Impl

        /// <inheritdoc />
        protected override void OnPluginInitialized()
        {
            Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(OnElementChanged);

            Svc.HotKeyManager.RegisterGlobal(
                    "ImportYouTube",
                    "Import a YouTube video into the Media Player",
                    HotKeyScopes.SMBrowser,
                    new HotKey(Key.Y, KeyModifiers.CtrlShift),
                    MediaPlayerState.Instance.ImportYouTubeVideo
                    );

            _ = Task.Factory.StartNew(SocketListener.Start, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public override void ShowSettings()
        {
            // ConfigurationWindow.ShowAndActivate(null, HotKeyManager.Instance, MediaPlayerState.Instance.Config);
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
