using CLI.LMS.Models;
using CLI.LMS.Services;

namespace CLI.LMS.Helpers
{
    public class TeacherMenuHelper
    {
        public void EnterMainMenu()
        {
            var choice = "";
            do
            {
                Console.WriteLine("--=========================--");
                Console.WriteLine("Teacher Main Menu:");
                Console.WriteLine("--=========================--\n");
                Console.WriteLine("1. Add Course");
                Console.WriteLine("2. Select Course");
                Console.WriteLine("3. Back");
                choice = Console.ReadLine();

                if (choice == "1") AddCourse();
                else if (choice == "2") SelectCourse();
            } while (choice != "3");
        }

        private void AddCourse()
        {
            Console.Write("Course Name: ");
            var name = Console.ReadLine();
            Console.Write("Course Code: ");
            var code = Console.ReadLine();
            Console.Write("Course Description: ");
            var description = Console.ReadLine();
            CourseService.Current.Add(name, code, description);
            Console.WriteLine("Course added.\n");
        }

        private void SelectCourse()
        {
            var courses = CourseService.Current.GetAll();
            if (courses.Count == 0)
            {
                Console.WriteLine("No courses available.\n");
                return;
            }
            Console.WriteLine("Courses:");
            foreach (var c in courses)
                Console.WriteLine($"  [{c.Id}] {c.Name} ({c.Code})");
            Console.Write("Enter Course Id: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var course = CourseService.Current.GetById(id);
                if (course != null)
                    EnterCourseMenu(course);
                else
                    Console.WriteLine("Course not found.\n");
            }
        }

        private void EnterCourseMenu(Course course)
        {
            var choice = "";
            do
            {
                Console.WriteLine($"\n--=========================--");
                Console.WriteLine($"Course: {course.Name} ({course.Code})");
                Console.WriteLine("--=========================--");
                Console.WriteLine("1. Back");
                choice = Console.ReadLine();
            } while (choice != "1");
        }
    }
}
