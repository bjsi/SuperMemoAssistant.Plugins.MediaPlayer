using Forge.Forms;
using SuperMemoAssistant.Services;
using System.Reflection;
using System.Windows.Forms;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class Importer
    {
        /// <summary>
        /// Show a dialog which prompts the user to pick a PDF file to import
        /// </summary>
        /// <returns>Filename or null</returns>
        public static string OpenFileDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".pdf",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                CheckFileExists = true,
            };

            bool res = (bool)dlg.GetType()
                .GetMethod("RunDialog", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(dlg, new object[] { Svc.SM.UI.ElementWdw.Handle });

            return res //dlg.ShowDialog(this).GetValueOrDefault(false)
                ? dlg.FileName
                : null;
        }
    }
}
