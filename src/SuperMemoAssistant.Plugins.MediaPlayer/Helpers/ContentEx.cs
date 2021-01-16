using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Drawing;
using System.Drawing;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class ContentEx
    {
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;

        /// <summary>
        /// Get the HTML string content representing the first html control of the current element.
        /// </summary>
        /// <returns>HTML string or null</returns>
        public static string GetCurrentElementContent()
        {
            var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
            var htmlCtrl = ctrlGroup?.GetFirstHtmlControl()?.AsHtml();
            return htmlCtrl?.Text;
        }

        public static ContentBase CreateImageContent(Image image, string title)
        {
            if (image == null)
                return null;

            int imgRegistryId = Svc.SM.Registry.Image.AddMember(
                    new ImageWrapper(image),
                    title
                    );

            if (imgRegistryId <= 0)
                return null;

            return new ImageContent(imgRegistryId, Config.ImageStretchType);
        }
    }
}
