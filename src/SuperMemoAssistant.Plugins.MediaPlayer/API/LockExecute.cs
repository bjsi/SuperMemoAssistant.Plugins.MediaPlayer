using Anotar.Serilog;
using MethodDecorator.Fody.Interfaces;
using SuperMemoAssistant.Plugins.MediaPlayer.API;
using System;
using System.Reflection;

[module: LockExecute]

namespace SuperMemoAssistant.Plugins.MediaPlayer.API
{
    // Similar to Python decorators -> wraps a method and execs code before and after the method runs
    // Used for UI locking, element id validation, exception handling

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class LockExecuteAttribute : Attribute, IMethodDecorator
    {
        private MethodBase Method { get; set; }

        public void Init(object instance, MethodBase method, object[] args)
        {
            Method = method;
        }

        public void OnEntry()
        {
            // Svc.SM.UI.ElementWdw.EnterUIUpdateLock();
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
