using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.EnhancedMarkdown.PropertyEditors
{
    [PropertyEditor("Our.Umbraco.EnhancedMarkdownEditor", "Enhanced Markdown Editor", "~/App_Plugins/EnhancedMarkdownEditor/views/markdowneditor.html", ValueType = "JSON")]
    public class EnhancedMarkdownPropertyEditor : PropertyEditor
    {
    }
}
