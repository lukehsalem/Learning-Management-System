using CLI.LMS.Models;
using CLI.LMS.Services;

namespace CLI.LMS.Helpers
{
    public class StudentMenuHelper
    {
        private Student _currentStudent;

        public void EnterMainMenu()
        {
            var students = StudentService.Current.GetAll();
            if (students.Count == 0)
            {
                Console.WriteLine("No students in the system.\n");
                return;
            }
            Console.WriteLine("--=========================--");
            Console.WriteLine("Student Proxy Selection:");
            Console.WriteLine("--=========================--\n");
            foreach (var s in students)
                Console.WriteLine($"  [{s.Id}] {s.Name} ({s.Code})");
            Console.Write("Enter Student Id: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                _currentStudent = StudentService.Current.GetById(id);
                if (_currentStudent == null) { Console.WriteLine("Student not found.\n"); return; }
                EnterStudentMenu();
            }
        }

        private void EnterStudentMenu()
        {
            var choice = "";
            do
            {
                Console.WriteLine("--=========================--");
                Console.WriteLine($"Student Main Menu: {_currentStudent.Name}");
                Console.WriteLine("--=========================--\n");
                Console.WriteLine("1. My Courses");
                Console.WriteLine("2. Back");
                choice = Console.ReadLine();

                if (choice == "1") SelectCourse();
            } while (choice != "2");
        }

        private void SelectCourse()
        {
            var enrolled = CourseService.Current.GetAll()
                .Where(c => c.Roster.Any(s => s.Id == _currentStudent.Id))
                .ToList();
            if (enrolled.Count == 0) { Console.WriteLine("You are not enrolled in any courses.\n"); return; }

            Console.WriteLine("Your Courses:");
            foreach (var c in enrolled)
                Console.WriteLine($"  [{c.Id}] {c.Name} ({c.Code})");
            Console.Write("Enter Course Id: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var course = enrolled.FirstOrDefault(c => c.Id == id);
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
                Console.WriteLine("1. View Modules");
                Console.WriteLine("2. View Assignments");
                Console.WriteLine("3. Submit Assignment");
                Console.WriteLine("4. View Other Students");
                Console.WriteLine("5. View Schedule");
                Console.WriteLine("6. Unenroll");
                Console.WriteLine("7. Back");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ViewModules(course); break;
                    case "2": ViewAssignments(course); break;
                    case "3": SubmitAssignment(course); break;
                    case "4": ViewOtherStudents(course); break;
                    case "5": ViewSchedule(course); break;
                    case "6":
                        UnenrollSelf(course);
                        choice = "7";
                        break;
                }
            } while (choice != "7");
        }

        private void ViewModules(Course course)
        {
            if (course.Modules.Count == 0) { Console.WriteLine("No modules.\n"); return; }
            foreach (var m in course.Modules)
            {
                Console.WriteLine($"\nModule {m.Id}:");
                for (int i = 0; i < m.Content.Count; i++)
                    Console.WriteLine($"  {i + 1}. {m.Content[i]}");
            }
            Console.WriteLine();
        }

        private void ViewAssignments(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var a in course.Assignments)
            {
                Console.WriteLine($"\n[{a.Id}] {a.Name}");
                Console.WriteLine($"  Description: {a.Description}");
                Console.WriteLine($"  Points: {a.AvailablePoints}");
                Console.WriteLine($"  Due: {a.DueDate:MM/dd/yyyy}");
                var mySub = a.Submissions.FirstOrDefault(s => s.StudentId == _currentStudent.Id);
                if (mySub != null)
                    Console.WriteLine($"  Grade: {(mySub.PointsAwarded.HasValue ? $"{mySub.PointsAwarded}/{a.AvailablePoints}" : "Not graded")}");
            }
            Console.WriteLine();
        }

        private void SubmitAssignment(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var a in course.Assignments)
                Console.WriteLine($"  [{a.Id}] {a.Name} (Due: {a.DueDate:MM/dd/yyyy})");
            Console.Write("Enter Assignment Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == id);
            if (assignment == null) { Console.WriteLine("Assignment not found.\n"); return; }

            Console.Write("Submission Content: ");
            var content = Console.ReadLine();
            var submission = new Submission
            {
                Id = assignment.Submissions.Count > 0 ? assignment.Submissions.Max(s => s.Id) + 1 : 1,
                StudentId = _currentStudent.Id,
                AssignmentId = assignment.Id,
                Content = content,
                SubmissionDate = DateTime.Now
            };
            assignment.Submissions.Add(submission);
            Console.WriteLine("Submission added.\n");
        }

        private void ViewOtherStudents(Course course)
        {
            var others = course.Roster.Where(s => s.Id != _currentStudent.Id).ToList();
            if (others.Count == 0) { Console.WriteLine("No other students enrolled.\n"); return; }
            Console.WriteLine("Other Students:");
            foreach (var s in others)
                Console.WriteLine($"  {s.Name} ({s.Code}) - {s.Classification}");
            Console.WriteLine();
        }

        private void ViewSchedule(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments scheduled.\n"); return; }
            Console.WriteLine("Schedule (by Due Date):");
            foreach (var a in course.Assignments.OrderBy(a => a.DueDate))
                Console.WriteLine($"  {a.DueDate:MM/dd/yyyy} - {a.Name}");
            Console.WriteLine();
        }

        private void UnenrollSelf(Course course)
        {
            course.Roster.RemoveAll(s => s.Id == _currentStudent.Id);
            Console.WriteLine($"You have been unenrolled from {course.Name}.\n");
        }
    }
}
