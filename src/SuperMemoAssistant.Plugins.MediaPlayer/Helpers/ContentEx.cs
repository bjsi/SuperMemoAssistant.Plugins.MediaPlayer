using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Plugins.MediaPlayer.Models;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Drawing;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SuperMemoAssistant.Plugins.MediaPlayer
{
    public static class ContentEx
    {
        private static MediaPlayerCfg Config => Svc<MediaPlayerPlugin>.Plugin.Config;

        public static CreationResult CreateSMElement(int parentElId, double priority, List<ContentBase> contents, References refs, bool shouldDisplay)
        {
            IElement parentElement =
              parentElId > 0
                ? Svc.SM.Registry.Element[parentElId]
                : null;

            var date = refs.Dates.FirstOrDefault();
            string dateStr = date.Item2 == null || !date.Item2.HasValue
                ? string.Empty
                : date.Item2.Value.ToString();

            var elemBuilder =
              new ElementBuilder(ElementType.Topic,
                                 contents.ToArray())
                .WithParent(parentElement)
                .WithTitle(refs.Title)
                .WithPriority(priority)
                .WithReference(
                  r => r.WithTitle(refs.Title)
                        .WithAuthor(refs.Author)
                        .WithDate(dateStr)
                        .WithSource("YouTube")
                        .WithLink(refs.Link)
                );

            if (shouldDisplay == false)
                elemBuilder = elemBuilder.DoNotDisplay();

            return Svc.SM.Registry.Element.Add(out _, ElemCreationFlags.CreateSubfolders, elemBuilder)
              ? CreationResult.Ok
              : CreationResult.FailCannotCreateElement;
        }

        /// <summary>
        /// Get the HTML string content representing the first html control of the current element.
        /// </summary>
        /// <returns>HTML string or null</returns>
        public static IControlHtml GetFirstHtmlCtrl()
        {
            var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
            return ctrlGroup?.GetFirstHtmlControl()?.AsHtml();
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
