namespace CLI.LMS.Models
{
    public class Module
    {
        public int Id { get; set; }
        public List<ModuleContent> Content { get; set; } = new List<ModuleContent>();
    }
}
