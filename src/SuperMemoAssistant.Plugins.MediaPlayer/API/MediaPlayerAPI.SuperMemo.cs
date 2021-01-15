using System.IO;
using AustinHarris.JsonRpc;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    // Implements Basic SM Functions
    // TODO: Lock UI, check elementId is expectedId, then execute
    // use MethodDecorator.Fody for ui locking and unlocking
    public partial class MediaPlayerAPI : JsonRpcService
    {

        public event EventHandler Closed;

        //
        // Mpv

        [JsonRpcMethod]
        public void Shutdown()
        {
            Closed?.Invoke(null, null);
        }

        //
        // Learning

        [JsonRpcMethod]
        public void SMLearn()
        {
            ForwardKeysToSM(new HotKey(Key.L, KeyModifiers.Ctrl));
        }

        [JsonRpcMethod]
        public void LearnAndReschedule()
        {
            Svc.SM.UI.ElementWdw.ForceRepetitionAndResume(
                    Config.LearnForcedScheduleInterval,
                    false
                    );
        }

        [JsonRpcMethod]
        public void SMReschedule()
        {
            ForwardKeysToSM(new HotKey(Key.J, KeyModifiers.Ctrl));
        }

        [JsonRpcMethod]
        public void SMLaterToday()
        {
            ForwardKeysToSM(new HotKey(Key.J, KeyModifiers.CtrlShift));
        }

        [JsonRpcMethod]
        public void SMDone()
        {
            Svc.SM.UI.ElementWdw.ActivateWindow();
            Svc.SM.UI.ElementWdw.Done();
        }

        [JsonRpcMethod]
        public void SMDelete()
        {
            Svc.SM.UI.ElementWdw.ActivateWindow();
            Svc.SM.UI.ElementWdw.Delete();
        }

        //
        // SM Navigation

        [JsonRpcMethod]
        public void SMPrevious()
        {
            ForwardKeysToSM(new HotKey(Key.Left, KeyModifiers.Alt));
        }

        [JsonRpcMethod]
        public void SMNext()
        {
            ForwardKeysToSM(new HotKey(Key.Right, KeyModifiers.Alt));
        }

        [JsonRpcMethod]
        public void SMParent()
        {
            var parent = Svc.SM.UI.ElementWdw.CurrentElement?.Parent;

            if (parent != null)
                Svc.SM.UI.ElementWdw.GoToElement(parent.Id);
        }

        [JsonRpcMethod]
        public void SMChild()
        {
            var child = Svc.SM.UI.ElementWdw.CurrentElement?.FirstChild;

            if (child != null)
                Svc.SM.UI.ElementWdw.GoToElement(child.Id);
        }

        [JsonRpcMethod]
        public void SMPrevSibling()
        {
            var prevSibling = Svc.SM.UI.ElementWdw.CurrentElement?.PrevSibling;

            if (prevSibling != null)
                Svc.SM.UI.ElementWdw.GoToElement(prevSibling.Id);
        }

        [JsonRpcMethod]
        public void SMNextSibling()
        {
            var nextSibling = Svc.SM.UI.ElementWdw.CurrentElement?.NextSibling;

            if (nextSibling != null)
                Svc.SM.UI.ElementWdw.GoToElement(nextSibling.Id);
        }
    }
}
