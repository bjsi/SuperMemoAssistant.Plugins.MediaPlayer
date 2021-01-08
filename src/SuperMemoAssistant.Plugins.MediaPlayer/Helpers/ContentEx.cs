namespace SuperMemoAssistant.Plugins.MediaPlayer
{
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
}
