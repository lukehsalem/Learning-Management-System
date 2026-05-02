namespace CLI.LMS.Models
{
    public class Module
    {
        public int Id { get; set; }
        public List<string> Content { get; set; } = new List<string>();
    }
}
