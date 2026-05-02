using MAUI.LMS.Models;

namespace MAUI.LMS.Services
{
    public class StudentService
    {
        private static StudentService _instance;
        public static StudentService Current
        {
            get
            {
                if (_instance == null) _instance = new StudentService();
                return _instance;
            }
        }
        private StudentService() { }

        private List<Student> _students = new List<Student>();

        public List<Student> GetAll() => _students;

        public Student GetById(int id) => _students.FirstOrDefault(s => s.Id == id);

        public Student Add(string name, string code, string classification)
        {
            var student = new Student
            {
                Id = _students.Count > 0 ? _students.Max(s => s.Id) + 1 : 1,
                Name = name,
                Code = code,
                Classification = classification
            };
            _students.Add(student);
            return student;
        }

        public void Update(Student student, string name, string code, string classification)
        {
            student.Name = name;
            student.Code = code;
            student.Classification = classification;
        }

        public void Delete(Student student)
        {
            foreach (var course in CourseService.Current.GetAll())
            {
                course.Roster.RemoveAll(s => s.Id == student.Id);
                foreach (var assignment in course.Assignments)
                    assignment.Submissions.RemoveAll(s => s.StudentId == student.Id);
            }
            _students.Remove(student);
        }
    }
}
