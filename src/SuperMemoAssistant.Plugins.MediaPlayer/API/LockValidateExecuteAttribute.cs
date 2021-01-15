[module: LockValidateExecute]

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InterceptorAttribute : Attribute, IMethodDecorator
    {
        private int ExpectedElementId { get; set; }
        private MethodBase Method { get; set; }

        public void Init(object instance, MethodBase method, object[] args)
        {
            ExpectedElementId = (int)args[0];
            Method = method;
        }

        public void OnEntry()
        {
            Svc.SM.EnterUIUpdateLock();
            if (Svc.SM.UI.CurrentElementId != ExpectedElementId)
                throw new Exception(); // TODO: what kind
        }

        public void OnExit()
        {
            Svc.SM.ExitUIUpdateLock();
        }

        public void OnException(Exception e)
        {
            Svc.SM.ExitUIUpdateLock();
            LogTo.Info($"MediaPlayer API failed to call JsonRpc method {Method.Name} with exception {e}");
        }
    }
}
