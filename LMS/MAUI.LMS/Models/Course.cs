namespace MAUI.LMS.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
        public List<Student> Roster { get; set; } = new List<Student>();
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public List<AssignmentGroup> AssignmentGroups { get; set; } = new List<AssignmentGroup>();
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public GradeRange GradeRanges { get; set; } = new GradeRange();
    }
}
