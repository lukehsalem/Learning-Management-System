using MAUI.LMS.Models;
using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class TeacherCourseDetailPage : ContentPage
{
    public string CourseId { get; set; }
    private Course _course;

    public TeacherCourseDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!int.TryParse(CourseId, out int id)) return;
        _course = CourseService.Current.GetById(id);
        if (_course == null) return;

        Title = _course.Name;
        CourseNameLabel.Text = $"{_course.Name} ({_course.Code})";
        CourseInfoLabel.Text = $"Section {_course.Section} — {_course.Semester}";
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshAnnouncements();
        RefreshRoster();
        RefreshAssignments();
        RefreshModules();
    }

    private void RefreshAnnouncements()
    {
        var list = _course.Announcements.OrderByDescending(a => a.PostedDate).ToList();
        AnnouncementsCollection.ItemsSource = null;
        AnnouncementsCollection.ItemsSource = list;
        NoAnnouncementsLabel.IsVisible = list.Count == 0;
    }

    private void RefreshRoster()
    {
        RosterCollection.ItemsSource = null;
        RosterCollection.ItemsSource = _course.Roster.ToList();
        NoRosterLabel.IsVisible = _course.Roster.Count == 0;
    }

    private void RefreshAssignments()
    {
        AssignmentsCollection.ItemsSource = null;
        AssignmentsCollection.ItemsSource = _course.Assignments.ToList();
        NoAssignmentsLabel.IsVisible = _course.Assignments.Count == 0;
    }

    private void RefreshModules()
    {
        var items = _course.Modules.Select(m => new { m.Id, m.Name, ItemCount = m.Content.Count }).ToList();
        ModulesCollection.ItemsSource = null;
        ModulesCollection.ItemsSource = items;
        NoModulesLabel.IsVisible = items.Count == 0;
    }

    // --- Settings (ISSUE-47) ---

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(CourseSettingsPage)}?courseId={CourseId}");
    }

    // --- Announcements ---

    private async void OnAddAnnouncementClicked(object sender, EventArgs e)
    {
        var title = await DisplayPromptAsync("New Announcement", "Title:");
        if (string.IsNullOrWhiteSpace(title)) return;
        var body = await DisplayPromptAsync("New Announcement", "Body:");
        if (body == null) return;

        _course.Announcements.Add(new Announcement
        {
            Id = _course.Announcements.Count > 0 ? _course.Announcements.Max(a => a.Id) + 1 : 1,
            Title = title.Trim(),
            Body = body.Trim(),
            PostedDate = DateTime.Now
        });
        RefreshAnnouncements();
    }

    private async void OnDeleteAnnouncementClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var announcement = _course.Announcements.FirstOrDefault(a => a.Id == id);
        if (announcement == null) return;
        bool confirm = await DisplayAlert("Delete Announcement", $"Delete \"{announcement.Title}\"?", "Delete", "Cancel");
        if (!confirm) return;
        _course.Announcements.Remove(announcement);
        RefreshAnnouncements();
    }

    // --- Roster ---

    private async void OnEnrollStudentClicked(object sender, EventArgs e)
    {
        var allStudents = StudentService.Current.GetAll()
            .Where(s => !_course.Roster.Any(r => r.Id == s.Id))
            .ToList();

        if (allStudents.Count == 0)
        {
            await DisplayAlert("Enroll Student", "All students are already enrolled, or no students exist in the system.", "OK");
            return;
        }

        var options = allStudents.Select(s => $"{s.Name} ({s.Code})").ToArray();
        var choice = await DisplayActionSheet("Select Student to Enroll", "Cancel", null, options);
        if (choice == null || choice == "Cancel") return;

        var idx = Array.IndexOf(options, choice);
        _course.Roster.Add(allStudents[idx]);
        RefreshRoster();
    }

    private async void OnUnenrollStudentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var student = _course.Roster.FirstOrDefault(s => s.Id == id);
        if (student == null) return;
        bool confirm = await DisplayAlert("Remove Student", $"Remove {student.Name} from this course?", "Remove", "Cancel");
        if (!confirm) return;
        _course.Roster.Remove(student);
        RefreshRoster();
    }

    private async void OnExportRosterClicked(object sender, EventArgs e)
    {
        if (_course.Roster.Count == 0)
        {
            await DisplayAlert("Export Roster", "No students to export.", "OK");
            return;
        }

        var lines = new List<string> { "Id,Name,Code,Classification" };
        lines.AddRange(_course.Roster.Select(s => $"{s.Id},{Escape(s.Name)},{Escape(s.Code)},{Escape(s.Classification)}"));
        var csv = string.Join("\n", lines);

        var path = Path.Combine(FileSystem.AppDataDirectory, $"roster_{_course.Code}_{_course.Semester?.Replace(" ", "_") ?? "nosem"}.csv");
        File.WriteAllText(path, csv);

        await DisplayAlert("Export Roster", $"Roster exported to:\n{path}", "OK");
    }

    private async void OnImportRosterClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Roster CSV",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text", "public.plain-text" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.Android, new[] { "text/csv", "text/plain" } },
                { DevicePlatform.WinUI, new[] { ".csv", ".txt" } }
            })
        });

        if (result == null) return;

        var lines = File.ReadAllLines(result.FullPath);
        int added = 0;

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;
            var code = parts[2].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(code)) continue;

            var existing = StudentService.Current.GetAll().FirstOrDefault(s => s.Code == code);
            if (existing == null)
            {
                var name = parts.Length > 1 ? parts[1].Trim().Trim('"') : code;
                var classification = parts.Length > 3 ? parts[3].Trim().Trim('"') : "";
                existing = StudentService.Current.Add(name, code, classification);
            }

            if (!_course.Roster.Any(s => s.Id == existing.Id))
            {
                _course.Roster.Add(existing);
                added++;
            }
        }

        RefreshRoster();
        await DisplayAlert("Import Roster", $"Imported {added} new student(s).", "OK");
    }

    private static string Escape(string value) =>
        value != null && value.Contains(',') ? $"\"{value}\"" : value ?? "";

    // --- Assignments ---

    private async void OnAddAssignmentClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Assignment", "Name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        var description = await DisplayPromptAsync("Add Assignment", "Description:");
        if (description == null) return;
        var ptsStr = await DisplayPromptAsync("Add Assignment", "Available Points:", keyboard: Keyboard.Numeric);
        if (!int.TryParse(ptsStr, out int points)) return;
        var dateStr = await DisplayPromptAsync("Add Assignment", "Due Date (MM/DD/YYYY):");
        if (!DateTime.TryParse(dateStr, out DateTime dueDate)) return;

        var quizChoice = await DisplayActionSheet("Assignment Type", "Cancel", null, "Regular Assignment", "Quiz");
        if (quizChoice == null || quizChoice == "Cancel") return;
        bool isQuiz = quizChoice == "Quiz";
        string quizQuestion = null;
        if (isQuiz)
        {
            quizQuestion = await DisplayPromptAsync("Quiz Question", "Enter the question students will answer:");
            if (string.IsNullOrWhiteSpace(quizQuestion)) return;
        }

        _course.Assignments.Add(new Assignment
        {
            Id = _course.Assignments.Count > 0 ? _course.Assignments.Max(a => a.Id) + 1 : 1,
            Name = name.Trim(),
            Description = description.Trim(),
            AvailablePoints = points,
            DueDate = dueDate,
            IsQuiz = isQuiz,
            QuizQuestion = quizQuestion?.Trim()
        });
        RefreshAssignments();
    }

    private async void OnEditAssignmentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var assignment = _course.Assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return;

        var name = await DisplayPromptAsync("Edit Assignment", "Name:", initialValue: assignment.Name);
        if (name == null) return;
        var description = await DisplayPromptAsync("Edit Assignment", "Description:", initialValue: assignment.Description);
        if (description == null) return;
        var ptsStr = await DisplayPromptAsync("Edit Assignment", "Available Points:", initialValue: assignment.AvailablePoints.ToString(), keyboard: Keyboard.Numeric);
        var dateStr = await DisplayPromptAsync("Edit Assignment", "Due Date (MM/DD/YYYY):", initialValue: assignment.DueDate.ToString("MM/dd/yyyy"));

        if (!string.IsNullOrWhiteSpace(name)) assignment.Name = name.Trim();
        if (!string.IsNullOrWhiteSpace(description)) assignment.Description = description.Trim();
        if (int.TryParse(ptsStr, out int pts)) assignment.AvailablePoints = pts;
        if (DateTime.TryParse(dateStr, out DateTime due)) assignment.DueDate = due;

        if (assignment.IsQuiz)
        {
            var question = await DisplayPromptAsync("Quiz Question", "Edit the question:", initialValue: assignment.QuizQuestion);
            if (!string.IsNullOrWhiteSpace(question)) assignment.QuizQuestion = question.Trim();
        }

        RefreshAssignments();
    }

    private async void OnDeleteAssignmentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var assignment = _course.Assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return;
        bool confirm = await DisplayAlert("Delete Assignment", $"Delete \"{assignment.Name}\" and all its submissions?", "Delete", "Cancel");
        if (!confirm) return;
        foreach (var group in _course.AssignmentGroups)
            group.Assignments.RemoveAll(a => a.Id == id);
        _course.Assignments.Remove(assignment);
        RefreshAssignments();
    }

    private async void OnGradeAssignmentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var assignment = _course.Assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return;

        if (assignment.Submissions.Count == 0)
        {
            await DisplayAlert("Grade", "No submissions for this assignment.", "OK");
            return;
        }

        var subOptions = assignment.Submissions.Select(s =>
        {
            var student = StudentService.Current.GetById(s.StudentId);
            var graded = s.PointsAwarded.HasValue ? $" [{s.PointsAwarded}/{assignment.AvailablePoints}]" : "";
            return $"{student?.Name ?? $"Student {s.StudentId}"}{graded}";
        }).ToArray();

        var chosen = await DisplayActionSheet("Select Submission to Grade", "Cancel", null, subOptions);
        if (chosen == null || chosen == "Cancel") return;

        var subIdx = Array.IndexOf(subOptions, chosen);
        var submission = assignment.Submissions[subIdx];

        var gradeStr = await DisplayPromptAsync("Grade Submission",
            $"Content: {submission.Content}\n\nGrade (points or %, e.g. '85' or '85%'):",
            keyboard: Keyboard.Numeric);
        if (gradeStr == null) return;

        int pts;
        gradeStr = gradeStr.Trim();
        if (gradeStr.EndsWith("%"))
        {
            if (!double.TryParse(gradeStr.TrimEnd('%'), out double pct)) { await DisplayAlert("Error", "Invalid grade.", "OK"); return; }
            pts = (int)Math.Round(pct / 100.0 * assignment.AvailablePoints);
        }
        else if (!int.TryParse(gradeStr, out pts)) { await DisplayAlert("Error", "Invalid grade.", "OK"); return; }

        submission.PointsAwarded = pts;

        var comment = await DisplayPromptAsync("Grade Submission", "Comment (optional):");
        if (!string.IsNullOrWhiteSpace(comment)) submission.Comment = comment.Trim();

        await DisplayAlert("Graded", $"Grade saved: {pts}/{assignment.AvailablePoints}", "OK");
    }

    // --- Gradebook Export (ISSUE-48) ---

    private async void OnExportGradebookClicked(object sender, EventArgs e)
    {
        if (_course.Roster.Count == 0 || _course.Assignments.Count == 0)
        {
            await DisplayAlert("Export Gradebook", "Course needs at least one student and one assignment.", "OK");
            return;
        }

        var header = "Student," + string.Join(",", _course.Assignments.Select(a => Escape(a.Name)));
        var rows = new List<string> { header };

        foreach (var student in _course.Roster)
        {
            var cells = _course.Assignments.Select(a =>
            {
                var sub = a.Submissions.FirstOrDefault(s => s.StudentId == student.Id);
                return sub?.PointsAwarded.HasValue == true ? sub.PointsAwarded.Value.ToString() : "";
            });
            rows.Add($"{Escape(student.Name)},{string.Join(",", cells)}");
        }

        var path = Path.Combine(FileSystem.AppDataDirectory,
            $"gradebook_{_course.Code}_{_course.Semester?.Replace(" ", "_") ?? "nosem"}.csv");
        File.WriteAllText(path, string.Join("\n", rows));
        await DisplayAlert("Export Gradebook", $"Gradebook exported to:\n{path}", "OK");
    }

    // --- Export / Import Assignments (ISSUE-46) ---

    private async void OnExportAssignmentsClicked(object sender, EventArgs e)
    {
        if (_course.Assignments.Count == 0)
        {
            await DisplayAlert("Export Assignments", "No assignments to export.", "OK");
            return;
        }

        var lines = new List<string> { "Name,Description,AvailablePoints,DueDate" };
        lines.AddRange(_course.Assignments.Select(a =>
            $"{Escape(a.Name)},{Escape(a.Description)},{a.AvailablePoints},{a.DueDate:MM/dd/yyyy}"));

        var path = Path.Combine(FileSystem.AppDataDirectory,
            $"assignments_{_course.Code}_{_course.Semester?.Replace(" ", "_") ?? "nosem"}.csv");
        File.WriteAllText(path, string.Join("\n", lines));
        await DisplayAlert("Export Assignments", $"Assignments exported to:\n{path}", "OK");
    }

    private async void OnImportAssignmentsClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Assignments CSV",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text", "public.plain-text" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.Android, new[] { "text/csv", "text/plain" } },
                { DevicePlatform.WinUI, new[] { ".csv", ".txt" } }
            })
        });

        if (result == null) return;

        var lines = File.ReadAllLines(result.FullPath);
        int added = 0;

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            var name = parts[0].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (_course.Assignments.Any(a => a.Name == name)) continue;

            var description = parts[1].Trim().Trim('"');
            int.TryParse(parts[2].Trim(), out int points);
            DateTime.TryParse(parts[3].Trim(), out DateTime dueDate);

            _course.Assignments.Add(new Assignment
            {
                Id = _course.Assignments.Count > 0 ? _course.Assignments.Max(a => a.Id) + 1 : 1,
                Name = name,
                Description = description,
                AvailablePoints = points,
                DueDate = dueDate
            });
            added++;
        }

        RefreshAssignments();
        await DisplayAlert("Import Assignments", $"Imported {added} new assignment(s).", "OK");
    }

    // --- Copy Assignments (ISSUE-43) ---

    private async void OnCopyAssignmentsClicked(object sender, EventArgs e)
    {
        var otherCourses = CourseService.Current.GetAll()
            .Where(c => c.Id != _course.Id && c.Assignments.Count > 0)
            .ToList();

        if (otherCourses.Count == 0)
        {
            await DisplayAlert("Copy Assignments", "No other courses with assignments available.", "OK");
            return;
        }

        var courseOptions = otherCourses.Select(c => $"{c.Name} ({c.Code})").ToArray();
        var chosenCourse = await DisplayActionSheet("Copy Assignments From:", "Cancel", null, courseOptions);
        if (chosenCourse == null || chosenCourse == "Cancel") return;

        var source = otherCourses[Array.IndexOf(courseOptions, chosenCourse)];
        var assignOptions = source.Assignments.Select(a => a.Name).ToArray();
        var chosenAssignment = await DisplayActionSheet("Select Assignment to Copy", "Cancel", null, assignOptions);
        if (chosenAssignment == null || chosenAssignment == "Cancel") return;

        var original = source.Assignments[Array.IndexOf(assignOptions, chosenAssignment)];
        _course.Assignments.Add(new Assignment
        {
            Id = _course.Assignments.Count > 0 ? _course.Assignments.Max(a => a.Id) + 1 : 1,
            Name = original.Name,
            Description = original.Description,
            AvailablePoints = original.AvailablePoints,
            DueDate = original.DueDate
        });

        RefreshAssignments();
        await DisplayAlert("Copied", $"\"{original.Name}\" copied (without submissions).", "OK");
    }

    // --- Modules ---

    private async void OnAddModuleClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Module", "Module Name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        _course.Modules.Add(new Module
        {
            Id = _course.Modules.Count > 0 ? _course.Modules.Max(m => m.Id) + 1 : 1,
            Name = name.Trim()
        });
        RefreshModules();
    }

    private async void OnDeleteModuleClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var module = _course.Modules.FirstOrDefault(m => m.Id == id);
        if (module == null) return;
        bool confirm = await DisplayAlert("Delete Module", $"Delete module \"{module.Name}\"?", "Delete", "Cancel");
        if (!confirm) return;
        _course.Modules.Remove(module);
        RefreshModules();
    }

    private async void OnManageModuleContentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        await Shell.Current.GoToAsync($"{nameof(TeacherModuleContentPage)}?courseId={CourseId}&moduleId={id}");
    }
}
