namespace CLI.LMS.Models
{
    public class PageContent : ModuleContent
    {
        public string Text { get; set; }
        public PageContent() { Type = "Page"; }
    }
}
