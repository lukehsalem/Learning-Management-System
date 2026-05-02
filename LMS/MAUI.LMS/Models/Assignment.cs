namespace MAUI.LMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AvailablePoints { get; set; }
        public DateTime DueDate { get; set; }
        public List<Submission> Submissions { get; set; } = new List<Submission>();
        public bool IsQuiz { get; set; }
        public string QuizQuestion { get; set; }
    }
}
