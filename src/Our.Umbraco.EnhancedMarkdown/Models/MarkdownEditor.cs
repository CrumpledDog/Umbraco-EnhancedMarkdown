using Newtonsoft.Json;

namespace Our.Umbraco.EnhancedMarkdown.Models
{
    public class MarkdownEditor
    {
        [JsonProperty("uniqueId")]
        public string Id { get; set; }

        [JsonProperty("editor")]
        public Editor Editor { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }
    }

    public class Editor
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
