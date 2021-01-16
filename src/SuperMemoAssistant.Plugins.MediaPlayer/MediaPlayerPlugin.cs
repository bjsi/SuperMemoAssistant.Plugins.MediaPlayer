using Anotar.Serilog;
using CliWrap;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Remoting;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MediaPlayerPlugin : SMAPluginBase<MediaPlayerPlugin>
    {
        #region Constructors

        //public MediaPlayerPlugin() : base("") { }

        #endregion







        #region Properties Impl - Public

        /// <inheritdoc />
        public override string Name => "MediaPlayerPlugin";
        public override bool HasSettings => true;

        public MediaPlayerAPI API { get; } = new MediaPlayerAPI();
        public MediaPlayerCfg Config { get; private set; }
        public bool HasExited { get; private set; } = false;

        #endregion


        #region Methods Impl

        private async Task<bool> CheckProgramInPath(string program)
        {
            var result = await Cli.Wrap("where")
                .WithArguments("/q " + program)
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            return result.ExitCode == 0;
        }
        
        public async Task VerifyDependenciesExist()
        {
            var programs = new string[] { "youtube-dl", "ffmpeg", "mpv" };
            var result = await Task.WhenAll(programs.Select(CheckProgramInPath));
            if (result.Any(isInstalled => !isInstalled))
            {
                string missingPrograms = string.Join(", ",
                    programs
                    .Zip(result, (program, installed) => (program, installed))
                    .Where(res => !res.installed)
                    .Select(res => res.program));

                LogTo.Debug("MediaPlayer dependencies not found in path: " + missingPrograms);
            }
            else
            {
                LogTo.Debug("All MediaPlayer dependencies found in path");
            }
        }

        private async Task LoadConfig()
        {
            Config = await Svc.Configuration.Load<MediaPlayerCfg>() ?? new MediaPlayerCfg();
        }

        /// <inheritdoc />
        protected override void PluginInit()
        {
            VerifyDependenciesExist();

            LoadConfig();

            Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedArgs>(OnElementChanged);

            Svc.HotKeyManager.RegisterGlobal(
                    "ImportYouTube",
                    "Import a YouTube video into the Media Player",
                    HotKeyScope.SMBrowser,
                    new HotKey(Key.Y, KeyModifiers.CtrlShift),
                    MediaPlayerState.Instance.ImportYouTubeVideo
                    );

            Task.Run(SocketListener.Start);
        }

        public override void Dispose()
        {
            HasExited = true;
            MediaPlayerState.Instance.PlayerWindow?.Close();
            base.Dispose();
        }

        /// <inheritdoc />
        public override void ShowSettings()
        {
            ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, MediaPlayerState.Instance.Config);
        }

        #endregion




        #region Methods

        [LogToErrorOnException]
        public static void OnElementChanged(SMDisplayedElementChangedArgs e)
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
