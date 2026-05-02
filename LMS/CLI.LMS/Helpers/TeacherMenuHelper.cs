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
                Console.WriteLine("3. Copy Course");
                Console.WriteLine("4. Back");
                choice = Console.ReadLine();

                if (choice == "1") AddCourse();
                else if (choice == "2") SelectCourse();
                else if (choice == "3") CopyCourse();
            } while (choice != "4");
        }

        private void AddCourse()
        {
            Console.Write("Course Name: ");
            var name = Console.ReadLine();
            Console.Write("Course Code: ");
            var code = Console.ReadLine();
            Console.Write("Course Description: ");
            var description = Console.ReadLine();
            Console.Write("Semester (e.g. Fall 2026): ");
            var semester = Console.ReadLine();
            Console.Write("Section: ");
            var section = Console.ReadLine();
            var course = CourseService.Current.Add(name, code, description);
            course.Semester = semester;
            course.Section = section;
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

            Console.Write("Filter by semester (or press Enter for all): ");
            var semesterFilter = Console.ReadLine();
            var filtered = string.IsNullOrWhiteSpace(semesterFilter)
                ? courses
                : courses.Where(c => string.Equals(c.Semester, semesterFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filtered.Count == 0) { Console.WriteLine("No courses found for that semester.\n"); return; }

            var sorted = filtered.OrderBy(c => c.Semester ?? "").ThenBy(c => c.Name).ToList();
            var grouped = sorted.GroupBy(c => string.IsNullOrWhiteSpace(c.Semester) ? "(No Semester)" : c.Semester);

            Console.WriteLine("Courses:");
            foreach (var group in grouped)
            {
                Console.WriteLine($"\n  {group.Key}:");
                foreach (var c in group)
                    Console.WriteLine($"    [{c.Id}] {c.Name} ({c.Code}) Section: {c.Section ?? "N/A"}");
            }

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

        private void CopyCourse()
        {
            var courses = CourseService.Current.GetAll();
            if (courses.Count == 0) { Console.WriteLine("No courses available.\n"); return; }
            foreach (var c in courses)
                Console.WriteLine($"  [{c.Id}] {c.Name} ({c.Code})");
            Console.Write("Enter Course Id to copy: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var source = CourseService.Current.GetById(id);
            if (source == null) { Console.WriteLine("Course not found.\n"); return; }

            Console.Write("New Course Name (Enter to keep same): ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = source.Name + " (Copy)";

            var copy = CourseService.Current.Add(name, source.Code, source.Description);
            copy.Semester = source.Semester;
            copy.Section = source.Section;

            var assignmentMap = new Dictionary<int, Assignment>();
            foreach (var a in source.Assignments)
            {
                var newAssignment = new Assignment
                {
                    Id = copy.Assignments.Count > 0 ? copy.Assignments.Max(x => x.Id) + 1 : 1,
                    Name = a.Name,
                    Description = a.Description,
                    AvailablePoints = a.AvailablePoints,
                    DueDate = a.DueDate
                };
                copy.Assignments.Add(newAssignment);
                assignmentMap[a.Id] = newAssignment;
            }

            foreach (var m in source.Modules)
            {
                var newModule = new Module
                {
                    Id = copy.Modules.Count > 0 ? copy.Modules.Max(x => x.Id) + 1 : 1
                };
                foreach (var item in m.Content)
                {
                    int newContentId = newModule.Content.Count > 0 ? newModule.Content.Max(x => x.Id) + 1 : 1;
                    if (item is AssignmentContent ac && assignmentMap.ContainsKey(ac.AssignmentId))
                        newModule.Content.Add(new AssignmentContent { Id = newContentId, AssignmentId = assignmentMap[ac.AssignmentId].Id });
                    else if (item is FileContent fc)
                        newModule.Content.Add(new FileContent { Id = newContentId, FileName = fc.FileName });
                    else if (item is PageContent pc)
                        newModule.Content.Add(new PageContent { Id = newContentId, Text = pc.Text });
                }
                copy.Modules.Add(newModule);
            }

            foreach (var g in source.AssignmentGroups)
            {
                var newGroup = new AssignmentGroup
                {
                    Id = copy.AssignmentGroups.Count > 0 ? copy.AssignmentGroups.Max(x => x.Id) + 1 : 1,
                    Name = g.Name,
                    Weight = g.Weight
                };
                foreach (var a in g.Assignments)
                    if (assignmentMap.ContainsKey(a.Id))
                        newGroup.Assignments.Add(assignmentMap[a.Id]);
                copy.AssignmentGroups.Add(newGroup);
            }

            Console.WriteLine($"Course '{source.Name}' copied as '{copy.Name}'.\n");
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
                Console.WriteLine("10. Manage Assignment Groups");
                Console.WriteLine("11. Delete Course");
                Console.WriteLine("12. Back");
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
                    case "10": ManageAssignmentGroups(course); break;
                    case "11":
                        DeleteCourse(course);
                        choice = "12";
                        break;
                }
            } while (choice != "12");
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
            foreach (var g in course.AssignmentGroups)
                g.Assignments.RemoveAll(a => a.Id == assignment.Id);
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
            Console.Write($"Grade (points or %, e.g. '85' or '85%'): ");
            var gradeInput = Console.ReadLine()?.Trim() ?? "";
            int pts;
            if (gradeInput.EndsWith("%"))
            {
                if (double.TryParse(gradeInput.TrimEnd('%'), out double pct))
                    pts = (int)Math.Round(pct / 100.0 * assignment.AvailablePoints);
                else { Console.WriteLine("Invalid input.\n"); return; }
            }
            else if (!int.TryParse(gradeInput, out pts))
            {
                Console.WriteLine("Invalid input.\n");
                return;
            }
            submission.PointsAwarded = pts;

            Console.Write("Comment (optional, Enter to skip): ");
            var comment = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(comment))
                submission.Comment = comment;

            Console.WriteLine("Submission graded.\n");
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
                foreach (var item in module.Content)
                {
                    if (item is AssignmentContent ac)
                    {
                        var a = course.Assignments.FirstOrDefault(x => x.Id == ac.AssignmentId);
                        Console.WriteLine($"  [{item.Id}] Assignment: {a?.Name ?? $"(Id {ac.AssignmentId})"}");
                    }
                    else if (item is FileContent fc)
                        Console.WriteLine($"  [{item.Id}] File: {fc.FileName}");
                    else if (item is PageContent pc)
                        Console.WriteLine($"  [{item.Id}] Page: {pc.Text}");
                }
                Console.WriteLine("1. Add Content");
                Console.WriteLine("2. Remove Content");
                Console.WriteLine("3. Back");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddModuleContent(course, module); break;
                    case "2": RemoveModuleContent(module); break;
                }
            } while (choice != "3");
        }

        private void AddModuleContent(Course course, Module module)
        {
            Console.WriteLine("Content Type:");
            Console.WriteLine("1. Assignment");
            Console.WriteLine("2. File");
            Console.WriteLine("3. Page");
            var typeChoice = Console.ReadLine();
            int newId = module.Content.Count > 0 ? module.Content.Max(x => x.Id) + 1 : 1;

            switch (typeChoice)
            {
                case "1":
                    if (course.Assignments.Count == 0) { Console.WriteLine("No assignments in this course.\n"); return; }
                    foreach (var a in course.Assignments)
                        Console.WriteLine($"  [{a.Id}] {a.Name}");
                    Console.Write("Enter Assignment Id: ");
                    if (!int.TryParse(Console.ReadLine(), out int aId)) return;
                    if (!course.Assignments.Any(a => a.Id == aId)) { Console.WriteLine("Assignment not found.\n"); return; }
                    module.Content.Add(new AssignmentContent { Id = newId, AssignmentId = aId });
                    Console.WriteLine("Assignment content added.\n");
                    break;
                case "2":
                    Console.Write("File Name: ");
                    var fileName = Console.ReadLine();
                    module.Content.Add(new FileContent { Id = newId, FileName = fileName });
                    Console.WriteLine("File content added.\n");
                    break;
                case "3":
                    Console.Write("Page Text: ");
                    var text = Console.ReadLine();
                    module.Content.Add(new PageContent { Id = newId, Text = text });
                    Console.WriteLine("Page content added.\n");
                    break;
                default:
                    Console.WriteLine("Invalid choice.\n");
                    break;
            }
        }

        private void RemoveModuleContent(Module module)
        {
            if (module.Content.Count == 0) { Console.WriteLine("No content.\n"); return; }
            Console.Write("Enter Content Id to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int removeId)) return;
            var item = module.Content.FirstOrDefault(c => c.Id == removeId);
            if (item == null) { Console.WriteLine("Content not found.\n"); return; }
            module.Content.Remove(item);
            Console.WriteLine("Content removed.\n");
        }

        private void ManageAssignmentGroups(Course course)
        {
            var choice = "";
            do
            {
                Console.WriteLine("\n--=========================--");
                Console.WriteLine("Assignment Groups:");
                Console.WriteLine("--=========================--");
                foreach (var g in course.AssignmentGroups)
                    Console.WriteLine($"  [{g.Id}] {g.Name} (Weight: {g.Weight}, {g.Assignments.Count} assignment(s))");
                Console.WriteLine("1. Add Group");
                Console.WriteLine("2. Edit Group Name");
                Console.WriteLine("3. Edit Group Weight");
                Console.WriteLine("4. List Groups");
                Console.WriteLine("5. Add Assignment to Group");
                Console.WriteLine("6. Delete Group");
                Console.WriteLine("7. Back");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddAssignmentGroup(course); break;
                    case "2": EditGroupName(course); break;
                    case "3": EditGroupWeight(course); break;
                    case "4": ListGroups(course); break;
                    case "5": AddAssignmentToGroup(course); break;
                    case "6": DeleteAssignmentGroup(course); break;
                }
            } while (choice != "7");
        }

        private void AddAssignmentGroup(Course course)
        {
            Console.Write("Group Name: ");
            var name = Console.ReadLine();
            Console.Write("Weight (e.g. 0.4 or 40): ");
            double.TryParse(Console.ReadLine(), out double weight);
            var group = new AssignmentGroup
            {
                Id = course.AssignmentGroups.Count > 0 ? course.AssignmentGroups.Max(g => g.Id) + 1 : 1,
                Name = name,
                Weight = weight
            };
            course.AssignmentGroups.Add(group);
            Console.WriteLine("Group added.\n");
        }

        private void EditGroupName(Course course)
        {
            if (course.AssignmentGroups.Count == 0) { Console.WriteLine("No groups.\n"); return; }
            foreach (var g in course.AssignmentGroups)
                Console.WriteLine($"  [{g.Id}] {g.Name}");
            Console.Write("Enter Group Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var group = course.AssignmentGroups.FirstOrDefault(g => g.Id == id);
            if (group == null) { Console.WriteLine("Group not found.\n"); return; }
            Console.Write($"New Name [{group.Name}]: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) group.Name = name;
            Console.WriteLine("Group updated.\n");
        }

        private void EditGroupWeight(Course course)
        {
            if (course.AssignmentGroups.Count == 0) { Console.WriteLine("No groups.\n"); return; }
            foreach (var g in course.AssignmentGroups)
                Console.WriteLine($"  [{g.Id}] {g.Name} (Weight: {g.Weight})");
            Console.Write("Enter Group Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var group = course.AssignmentGroups.FirstOrDefault(g => g.Id == id);
            if (group == null) { Console.WriteLine("Group not found.\n"); return; }
            Console.Write($"New Weight [{group.Weight}]: ");
            if (double.TryParse(Console.ReadLine(), out double weight)) group.Weight = weight;
            Console.WriteLine("Weight updated.\n");
        }

        private void ListGroups(Course course)
        {
            if (course.AssignmentGroups.Count == 0) { Console.WriteLine("No groups.\n"); return; }
            foreach (var g in course.AssignmentGroups)
            {
                Console.WriteLine($"\n[{g.Id}] {g.Name} (Weight: {g.Weight})");
                if (g.Assignments.Count == 0)
                    Console.WriteLine("  (no assignments)");
                else
                    foreach (var a in g.Assignments)
                        Console.WriteLine($"  - [{a.Id}] {a.Name}");
            }
            Console.WriteLine();
        }

        private void AddAssignmentToGroup(Course course)
        {
            if (course.AssignmentGroups.Count == 0) { Console.WriteLine("No groups.\n"); return; }
            if (course.Assignments.Count == 0) { Console.WriteLine("No assignments.\n"); return; }
            foreach (var g in course.AssignmentGroups)
                Console.WriteLine($"  [{g.Id}] {g.Name}");
            Console.Write("Enter Group Id: ");
            if (!int.TryParse(Console.ReadLine(), out int groupId)) return;
            var group = course.AssignmentGroups.FirstOrDefault(g => g.Id == groupId);
            if (group == null) { Console.WriteLine("Group not found.\n"); return; }
            foreach (var a in course.Assignments)
                Console.WriteLine($"  [{a.Id}] {a.Name}");
            Console.Write("Enter Assignment Id: ");
            if (!int.TryParse(Console.ReadLine(), out int aId)) return;
            var assignment = course.Assignments.FirstOrDefault(a => a.Id == aId);
            if (assignment == null) { Console.WriteLine("Assignment not found.\n"); return; }
            if (group.Assignments.Any(a => a.Id == aId)) { Console.WriteLine("Assignment already in group.\n"); return; }
            group.Assignments.Add(assignment);
            Console.WriteLine("Assignment added to group.\n");
        }

        private void DeleteAssignmentGroup(Course course)
        {
            if (course.AssignmentGroups.Count == 0) { Console.WriteLine("No groups.\n"); return; }
            foreach (var g in course.AssignmentGroups)
                Console.WriteLine($"  [{g.Id}] {g.Name}");
            Console.Write("Enter Group Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            var group = course.AssignmentGroups.FirstOrDefault(g => g.Id == id);
            if (group == null) { Console.WriteLine("Group not found.\n"); return; }
            course.AssignmentGroups.Remove(group);
            Console.WriteLine("Group deleted.\n");
        }

        private void DeleteCourse(Course course)
        {
            CourseService.Current.Delete(course);
            Console.WriteLine("Course deleted.\n");
        }
    }
}
