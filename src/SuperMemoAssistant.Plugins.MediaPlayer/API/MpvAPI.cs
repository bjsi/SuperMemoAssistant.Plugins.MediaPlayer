namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    // Dunno if this is actually necessary
    /* public class MpvAPI */
    /* { */
    /*     private MediaPlayerCfg Config => Svc<MediaPlayer>.Plugin.Config; */
    /*     private const string IpcPipePath = @"\\.\pipe\mpvsocket"; */

    /*     public Process CreateMpvInstance(string filePathOrUrl) */
    /*     { */
    /*         string[] args = new */
    /*         { */
    /*             "--input-ipc-server=" + IpcPipePath, */
    /*             "" */
    /*         }; */

    /*         return ProcessEx.CreateBackgroundProcess("path to mpv", args); */
    /*     } */

    /*     public static void Shutdown() */
    /*     { */
    /*         using (var pipeClient = NamedPipeClientStream(IpcPipePath, PipeDirection.InOut)) */
    /*         { */
    /*             pipeClient.Connect(); */
    /*             using (var reader = new StreamReader(pipeClient)) */
    /*             using (var writer = new StreamWriter(pipeClient)) */
    /*                 writer.Write(""); */
    /*         } */
    /*     } */
    /* } */
}
