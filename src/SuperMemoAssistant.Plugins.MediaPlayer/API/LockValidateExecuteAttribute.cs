using Anotar.Serilog;
using MethodDecorator.Fody.Interfaces;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using SuperMemoAssistant.Services;
using System;
using System.Reflection;

[module: LockValidateExecute]

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    // Similar to Python decorators -> wraps a method and execs code before and after the method runs
    // Used for UI locking, element id validation, exception handling

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class LockValidateExecuteAttribute : Attribute, IMethodDecorator
    {
        private int ExpectedElementId { get; set; }
        private MethodBase Method { get; set; }

        public void Init(object instance, MethodBase method, object[] args)
        {
            ExpectedElementId = ((MediaPlayerAPI)instance).ExpectedElementId;
            Method = method;
        }

        public void OnEntry()
        {
            // Svc.SM.UI.ElementWdw.EnterUIUpdateLock();
            if (Svc.SM.UI.ElementWdw.CurrentElementId != ExpectedElementId)
                throw new Exception(); // TODO: what kind
        }

        public void OnExit()
        {
            // Svc.SM.UI.ElementWdw.ExitUIUpdateLock();
        }

        public void OnException(Exception e)
        {
            //Svc.SM.UI.ElementWdw.ExitUIUpdateLock();
            LogTo.Information($"MediaPlayer API failed to call JsonRpc method {Method.Name} with exception {e}");
        }
    }
}
