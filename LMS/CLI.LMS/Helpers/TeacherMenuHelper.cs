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
        private void AddAssignment(Course course)
        {
            Console.Write("Assignment Name: ");
            var name = Console.ReadLine();
            Console.Write("Description: ");
            var description = Console.ReadLine();
            Console.Write("Available Points: ");
            int.TryParse(Console.ReadLine(), out int points);
            Console.Write("Due Date (MM/DD/YYYY): ");
            DateTime.TryParse(Console.ReadLine(), out DateTime dueDate);
            var assignment = new Assignment
            {
                Id = course.Assignments.Count > 0 ? course.Assignments.Max(a => a.Id) + 1 : 1,
                Name = name,
                Description = description,
                AvailablePoints = points,
                DueDate = dueDate
            };
            course.Assignments.Add(assignment);
            Console.WriteLine("Assignment added.\n");
        }
        private void EditAssignment(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var a in course.Assignments)
                Console.WriteLine($"  [{a.Id}] {a.Name} (Due: {a.DueDate:MM/dd/yyyy})");
            Console.Write("Enter Assignment Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == id);
            if (assignment == null) { Console.WriteLine("Assignment not found.\n"); return; }

            Console.Write($"Name [{assignment.Name}]: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) assignment.Name = name;

            Console.Write($"Description [{assignment.Description}]: ");
            var desc = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(desc)) assignment.Description = desc;

            Console.Write($"Available Points [{assignment.AvailablePoints}]: ");
            var pts = Console.ReadLine();
            if (int.TryParse(pts, out int points)) assignment.AvailablePoints = points;

            Console.Write($"Due Date [{assignment.DueDate:MM/dd/yyyy}]: ");
            var dt = Console.ReadLine();
            if (DateTime.TryParse(dt, out DateTime dueDate)) assignment.DueDate = dueDate;

            Console.WriteLine("Assignment updated.\n");
        }
        private void DeleteAssignment(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var a in course.Assignments)
                Console.WriteLine($"  [{a.Id}] {a.Name}");
            Console.Write("Enter Assignment Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == id);
            if (assignment == null) { Console.WriteLine("Assignment not found.\n"); return; }
            course.Assignments.Remove(assignment);
            Console.WriteLine("Assignment and its submissions deleted.\n");
        }
        private void GradeSubmission(Course course)
        {
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var a in course.Assignments)
                Console.WriteLine($"  [{a.Id}] {a.Name} ({a.Submissions.Count} submission(s))");
            Console.Write("Enter Assignment Id: ");
            if (!int.TryParse(Console.ReadLine(), out int assignmentId)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == assignmentId);
            if (assignment == null) { Console.WriteLine("Assignment not found.\n"); return; }
            if (assignment.Submissions.Count == 0) { Console.WriteLine("No submissions.\n"); return; }

            foreach (var sub in assignment.Submissions)
            {
                var student = StudentService.Current.GetById(sub.StudentId);
                var graded = sub.PointsAwarded.HasValue ? $" [Graded: {sub.PointsAwarded}/{assignment.AvailablePoints}]" : "";
                Console.WriteLine($"  [{sub.Id}] {student?.Name ?? $"Student {sub.StudentId}"}{graded}");
            }
            Console.Write("Enter Submission Id: ");
            if (!int.TryParse(Console.ReadLine(), out int subId)) return;
            var submission = assignment.Submissions.FirstOrDefault(s => s.Id == subId);
            if (submission == null) { Console.WriteLine("Submission not found.\n"); return; }

            Console.WriteLine($"Content: {submission.Content}");
            Console.Write($"Points Awarded (out of {assignment.AvailablePoints}): ");
            if (int.TryParse(Console.ReadLine(), out int pts))
            {
                submission.PointsAwarded = pts;
                Console.WriteLine("Submission graded.\n");
            }
        }
        private void AddModule(Course course)
        {
            var module = new Module
            {
                Id = course.Modules.Count > 0 ? course.Modules.Max(m => m.Id) + 1 : 1
            };
            course.Modules.Add(module);
            Console.WriteLine($"Module {module.Id} added.\n");
        }
        private void ManageModuleContent(Course course)
        {
            if (course.Modules.Count == 0) { Console.WriteLine("No modules.\n"); return; }
            foreach (var m in course.Modules)
                Console.WriteLine($"  [{m.Id}] Module {m.Id} ({m.Content.Count} item(s))");
            Console.Write("Enter Module Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var module = course.Modules.FirstOrDefault(m => m.Id == id);
            if (module == null) { Console.WriteLine("Module not found.\n"); return; }

            var choice = "";
            do
            {
                Console.WriteLine($"\nModule {module.Id} Content:");
                for (int i = 0; i < module.Content.Count; i++)
                    Console.WriteLine($"  [{i + 1}] {module.Content[i]}");
                Console.WriteLine("1. Add Content");
                Console.WriteLine("2. Modify Content");
                Console.WriteLine("3. Remove Content");
                Console.WriteLine("4. Back");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Content: ");
                        module.Content.Add(Console.ReadLine());
                        Console.WriteLine("Content added.\n");
                        break;
                    case "2":
                        if (module.Content.Count == 0) { Console.WriteLine("No content.\n"); break; }
                        Console.Write("Enter position number: ");
                        if (int.TryParse(Console.ReadLine(), out int pos) && pos >= 1 && pos <= module.Content.Count)
                        {
                            Console.Write("New content: ");
                            module.Content[pos - 1] = Console.ReadLine();
                            Console.WriteLine("Content updated.\n");
                        }
                        else Console.WriteLine("Invalid position.\n");
                        break;
                    case "3":
                        if (module.Content.Count == 0) { Console.WriteLine("No content.\n"); break; }
                        Console.Write("Enter position number: ");
                        if (int.TryParse(Console.ReadLine(), out int removePos) && removePos >= 1 && removePos <= module.Content.Count)
                        {
                            module.Content.RemoveAt(removePos - 1);
                            Console.WriteLine("Content removed.\n");
                        }
                        else Console.WriteLine("Invalid position.\n");
                        break;
                }
            } while (choice != "4");
        }
        private void DeleteCourse(Course course)
        {
            CourseService.Current.Delete(course);
            Console.WriteLine("Course deleted.\n");
        }
    }
}
