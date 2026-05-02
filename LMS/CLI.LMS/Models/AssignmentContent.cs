namespace CLI.LMS.Models
{
    public class AssignmentContent : ModuleContent
    {
        public int AssignmentId { get; set; }
        public AssignmentContent() { Type = "Assignment"; }
    }
}
