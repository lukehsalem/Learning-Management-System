using CLI.LMS.Models;

namespace CLI.LMS.Services
{
    public class CourseService
    {
        private static CourseService _instance;
        public static CourseService Current
        {
            get
            {
                if (_instance == null) _instance = new CourseService();
                return _instance;
            }
        }
        private CourseService() { }

        private List<Course> _courses = new List<Course>();

        public List<Course> GetAll() => _courses;

        public Course GetById(int id) => _courses.FirstOrDefault(c => c.Id == id);

        public Course Add(string name, string code, string description)
        {
            var course = new Course
            {
                Id = _courses.Count > 0 ? _courses.Max(c => c.Id) + 1 : 1,
                Name = name,
                Code = code,
                Description = description
            };
            _courses.Add(course);
            return course;
        }

        public void Delete(Course course) => _courses.Remove(course);
    }
}
