# CLAUDE.md — Learning Management System (COP4870, FSU)

## What This Is
A C# .NET Learning Management System for COP4870 at FSU. The project evolves across 7 sprints, starting as a CLI console app and refactoring into a MAUI GUI with WebAPI and Entity Framework. Target: complete Sprints 1-5.

## Grading
- Sprint 3 complete = C-
- Sprint 4 complete = B-  
- Sprint 5 complete = B
- Sprint 6 complete = A-
- Sprint 7 complete = A

Final submission = link to best branch + a demo video.

## Project Structure
LMS/CLI.LMS/ — Sprint 1-2 CLI app
LMS/CLI.LMS/Models/ — C# model classes
LMS/CLI.LMS/Helpers/ — Menu helper classes
LMS/CLI.LMS/Program.cs — Entry point
LMS/LMS.slnx — Solution file
backlog/ — Sprint YML files
scripts/ — Python GitHub issue scripts (do not modify)
.github/workflows/ — GitHub Actions (do not modify)

## C# Code Style
Match the professor's existing patterns exactly:

- Singleton pattern using a private constructor + static Current property with lock for thread safety
- private string _fieldName for private fields (underscore prefix, camelCase)
- Public properties use PascalCase with auto-properties
- Use List<T> for all collections
- Newtonsoft.Json for serialization (Sprint 6+)
- using blocks for all disposable resources
- Minimal comments — only when logic needs explanation
- No over-engineering — keep it simple and clean
- Namespaces match folder structure exactly

Model example pattern:
```csharp
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Student> Roster { get; set; } = new List<Student>();
}
```

Service/context example pattern:
```csharp
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
}
```

Menu helper pattern:
```csharp
public class TeacherMenuHelper
{
    public void EnterMainMenu()
    {
        Console.WriteLine("--=========================--");
        Console.WriteLine("Teacher Main Menu:");
        Console.WriteLine("--=========================--\n");
    }
}
```

## Selection Rule
Always select by stable integer Id — never by list index.

## Sprint 1 — CLI Foundation
Issues: ISSUE-3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23

Models needed:
- User (Id, Name, Code)
- Student extends User (Classification)
- Instructor extends User (YearsOfExperience)
- Course (Id, Code, Name, Description, Roster, Modules, Assignments)
- Module (Id, Content as List<string>)
- Assignment (Id, Name, Description, AvailablePoints, DueDate, Submissions)
- Submission (Id, StudentId, AssignmentId, Content, SubmissionDate)

One global student list, shallow copied into course rosters.
Deleting assignment must delete its submissions.
Deleting course removes content but NOT students.

Menus needed:
- Main menu (Student / Teacher / Quit) — already in Program.cs
- Teacher menu (add course, select course by Id)
- Student proxy menu (select existing student by Id)
- Student main menu (select enrolled course)
- Course main menu (modules, assignments, other students, schedule)

## Sprint 2 — Advanced CLI
Issues: ISSUE-24, 25, 26, 27, 28, 29, 30, 31, 32

- Module content uses inheritance: base ModuleContent, derived Assignment/File/Page types
- Assignment groups with weights for grade calculation
- Deep copy course (excludes roster and submissions)
- Add Semester and Section properties to Course
- Teachers view courses grouped by semester with filter/sort
- Students see individual grades + weighted course average
- Teacher grading: points or percentage + comment

## Sprint 3 — MAUI GUI
Issues: ISSUE-33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45

- Migrate to .NET MAUI project
- Main menu: Student button / Teacher button
- Student selects from list → sees enrolled courses → course detail
- Course detail shows letter grade prominently
- Teacher manages roster, assignments, modules, announcements
- Export/import rosters and assignments (idempotent, non-destructive)
- No user traps — always allow back navigation

## Sprint 4 — Advanced Features
Issues: ISSUE-46, 47, 48, 49, 50

- Import/export assignments between courses
- Teacher sets grade ranges for letter grades in course settings
- Gradebook CSV export (columns = assignments, rows = students)
- Quiz assignment type (teacher writes question, student responds in textbox)
- Students upload files as part of submissions

## Sprint 5 — WebAPI
Issues: ISSUE-51, 52, 53, 54

- ASP.NET WebAPI controller for student data
- ASP.NET WebAPI controller for course data
- Student search in course rosters
- Teachers set semester start/stop dates

## Git Workflow
- Commit format: "Issue-XX: brief description" — sentence case, only capitalize the first word unless it's a proper noun or acronym (API, MAUI, CLI, WebAPI)
- Never include co-author signatures or any "Co-authored-by" lines in commits
- Never add any AI attribution to commit messages
- Push to origin main
- Never modify scripts/ or .github/workflows/
- Final submission = link to best branch + demo video
