namespace CLI.LMS.Models
{
    public class FileContent : ModuleContent
    {
        public string FileName { get; set; }
        public FileContent() { Type = "File"; }
    }
}
