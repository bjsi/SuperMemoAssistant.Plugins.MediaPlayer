using System;
using System.Windows.Input;
using AustinHarris.JsonRpc;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    // Implements Basic SM Functions
    // TODO: When new Interop available: Lock UI, check elementId is expectedId, then execute
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
        [LockValidateExecute]
        public void SMLearn()
        {
            ForwardKeysToSM(new HotKey(Key.L, KeyModifiers.Ctrl));
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public void LearnAndReschedule()
        {
            Svc.SM.UI.ElementWdw.ForceRepetitionAndResume(
                    Config.LearnForcedScheduleInterval,
                    false
                    );
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public void SMReschedule()
        {
            ForwardKeysToSM(new HotKey(Key.J, KeyModifiers.Ctrl));
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public void SMLaterToday()
        {
            ForwardKeysToSM(new HotKey(Key.J, KeyModifiers.CtrlShift));
        }

        [JsonRpcMethod]
        [LockValidateExecute]
        public void SMDone()
        {
            Svc.SM.UI.ElementWdw.ActivateWindow();
            Svc.SM.UI.ElementWdw.Done();
        }

        [JsonRpcMethod]
        [LockValidateExecute]
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

        private static bool ForwardKeysToSM(HotKey hotKey,
                               int timeout = 100)
        {
            var handle = Svc.SM.UI.ElementWdw.Handle;

            if (handle.ToInt32() == 0)
                return false;

            if (hotKey.Alt && hotKey.Ctrl == false && hotKey.Win == false)
                return Sys.IO.Devices.Keyboard.PostSysKeysAsync(
                  handle,
                  hotKey
                ).Wait(timeout);

            return Sys.IO.Devices.Keyboard.PostKeysAsync(
              handle,
              hotKey
            ).Wait(timeout);
        }
    }
}
