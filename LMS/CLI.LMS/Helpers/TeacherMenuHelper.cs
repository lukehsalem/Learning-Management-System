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
                Console.WriteLine("1.  Update Description");
                Console.WriteLine("2.  Enroll Student");
                Console.WriteLine("3.  Unenroll Student");
                Console.WriteLine("4.  Add Assignment");
                Console.WriteLine("5.  Edit Assignment");
                Console.WriteLine("6.  Delete Assignment");
                Console.WriteLine("7.  Grade Submission");
                Console.WriteLine("8.  Add Module");
                Console.WriteLine("9.  Manage Module Content");
                Console.WriteLine("10. Delete Course");
                Console.WriteLine("11. Back");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":  UpdateDescription(course); break;
                    case "2":  EnrollStudent(course); break;
                    case "3":  UnenrollStudent(course); break;
                    case "4":  AddAssignment(course); break;
                    case "5":  EditAssignment(course); break;
                    case "6":  DeleteAssignment(course); break;
                    case "7":  GradeSubmission(course); break;
                    case "8":  AddModule(course); break;
                    case "9":  ManageModuleContent(course); break;
                    case "10":
                        DeleteCourse(course);
                        choice = "11";
                        break;
                }
            } while (choice != "11");
        }

        private void UpdateDescription(Course course)
        {
            Console.Write("New Description: ");
            course.Description = Console.ReadLine();
            Console.WriteLine("Description updated.\n");
        }
        private void EnrollStudent(Course course)
        {
            Console.WriteLine("1. Select Existing Student");
            Console.WriteLine("2. Create New Student");
            var choice = Console.ReadLine();
            if (choice == "1")
            {
                var students = StudentService.Current.GetAll();
                if (students.Count == 0) { Console.WriteLine("No students in system.\n"); return; }
                foreach (var s in students)
                    Console.WriteLine($"  [{s.Id}] {s.Name} ({s.Code})");
                Console.Write("Enter Student Id: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var student = StudentService.Current.GetById(id);
                    if (student == null) { Console.WriteLine("Student not found.\n"); return; }
                    if (course.Roster.Any(s => s.Id == student.Id))
                    {
                        Console.WriteLine("Student already enrolled.\n");
                        return;
                    }
                    course.Roster.Add(student);
                    Console.WriteLine($"{student.Name} enrolled.\n");
                }
            }
            else if (choice == "2")
            {
                Console.Write("Name: ");
                var name = Console.ReadLine();
                Console.Write("Code (FSUID): ");
                var code = Console.ReadLine();
                Console.Write("Classification: ");
                var classification = Console.ReadLine();
                var student = StudentService.Current.Add(name, code, classification);
                course.Roster.Add(student);
                Console.WriteLine($"{student.Name} created and enrolled.\n");
            }
        }
        private void UnenrollStudent(Course course)
        {
            if (course.Roster.Count == 0) { Console.WriteLine("No students enrolled.\n"); return; }
            foreach (var s in course.Roster)
                Console.WriteLine($"  [{s.Id}] {s.Name} ({s.Code})");
            Console.Write("Enter Student Id to unenroll: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var student = course.Roster.FirstOrDefault(s => s.Id == id);
                if (student == null) { Console.WriteLine("Student not found in roster.\n"); return; }
                course.Roster.Remove(student);
                Console.WriteLine($"{student.Name} unenrolled.\n");
            }
        }
        private void AddAssignment(Course course) { }
        private void EditAssignment(Course course) { }
        private void DeleteAssignment(Course course) { }
        private void GradeSubmission(Course course) { }
        private void AddModule(Course course) { }
        private void ManageModuleContent(Course course) { }
        private void DeleteCourse(Course course)
        {
            CourseService.Current.Delete(course);
            Console.WriteLine("Course deleted.\n");
        }
    }
}
