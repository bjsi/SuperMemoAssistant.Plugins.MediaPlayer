namespace SuperMemoAssistant.Plugins.MediaPlayer.Helpers
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>Extension methods for <see cref="Process" /></summary>
    public static class ProcessEx
    {
        #region Methods

        /// <summary>Create a process and runs it in the background (no window, no shell execute)</summary>
        /// <param name="binName">The executable name</param>
        /// <param name="args">Optional parameters</param>
        /// <param name="workingDirectory">Optional working directory</param>
        /// <returns>The created process</returns>
        public static Process CreateBackgroundProcess(string binName,
                                                      string args,
                                                      string workingDirectory = null)
        {
            Process p = new Process
            {
                StartInfo =
        {
          FileName        = binName,
          Arguments       = args,
          UseShellExecute = false,
          CreateNoWindow  = true,
        }
            };

            if (workingDirectory != null)
                p.StartInfo.WorkingDirectory = workingDirectory;

            return p;
        }

        /// <summary>Executes the process and waits until it returns. Reads standard and error output.</summary>
        /// <param name="p">The process to execute</param>
        /// <param name="timeout">Optional execution timeout</param>
        /// <param name="kill">Whether to make sure the process is killed</param>
        /// <returns>Process' exit code, standard + error output, and whether the process timed out.</returns>
        public static (int exitCode, string output, bool timedOut) ExecuteBlockingWithOutputs(this Process p,
                                                                                              int timeout = int.MaxValue,
                                                                                              bool kill = true)
        {
            StringBuilder outputBuilder = new StringBuilder();

            void OutputDataReceived(object sender,
                                    DataReceivedEventArgs e)
            {
                lock (outputBuilder)
                    outputBuilder.AppendLine(e.Data);
            }

            p.EnableRaisingEvents = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += OutputDataReceived;
            p.ErrorDataReceived += OutputDataReceived;

            try
            {
                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                if (!p.WaitForExit(timeout))
                {
                    if (kill)
                        p.Kill();

                    return (-1, null, true);
                }

                p.WaitForExit();

                return (p.ExitCode, outputBuilder.ToString(), false);
            }
            finally
            {
                p.Dispose();
            }
        }

        #endregion
    }
}
