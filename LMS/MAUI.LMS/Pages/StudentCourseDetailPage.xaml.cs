using MAUI.LMS.Models;
using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

[QueryProperty(nameof(StudentId), "studentId")]
[QueryProperty(nameof(CourseId), "courseId")]
public partial class StudentCourseDetailPage : ContentPage
{
    public string StudentId { get; set; }
    public string CourseId { get; set; }

    private Student _student;
    private Course _course;

    public StudentCourseDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!int.TryParse(StudentId, out int studentId) || !int.TryParse(CourseId, out int courseId)) return;

        _student = StudentService.Current.GetById(studentId);
        _course = CourseService.Current.GetById(courseId);
        if (_student == null || _course == null) return;

        Title = _course.Name;
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshGrade();
        RefreshAnnouncements();
        RefreshModules();
        RefreshAssignments();
    }

    private void RefreshGrade()
    {
        double pct = CalculateCourseGrade();
        if (pct < 0)
        {
            LetterGradeLabel.Text = "N/A";
            PercentageLabel.Text = "No graded assignments yet";
        }
        else
        {
            LetterGradeLabel.Text = GetLetterGrade(pct, _course.GradeRanges);
            PercentageLabel.Text = $"{pct:F1}%";
        }
    }

    private void RefreshAnnouncements()
    {
        var announcements = _course.Announcements.OrderByDescending(a => a.PostedDate).ToList();
        AnnouncementsSection.IsVisible = announcements.Count > 0;
        AnnouncementsCollection.ItemsSource = null;
        AnnouncementsCollection.ItemsSource = announcements;
    }

    private void RefreshModules()
    {
        var items = _course.Modules.Select(m => new
        {
            m.Name,
            ContentSummary = m.Content.Count == 0
                ? "No content"
                : string.Join(", ", m.Content.Select(c =>
                {
                    if (c is AssignmentContent ac)
                    {
                        var a = _course.Assignments.FirstOrDefault(x => x.Id == ac.AssignmentId);
                        return $"Assignment: {a?.Name ?? "Unknown"}";
                    }
                    if (c is FileContent fc) return $"File: {fc.FileName}";
                    if (c is PageContent pc) return $"Page: {pc.Text?.Substring(0, Math.Min(30, pc.Text.Length))}...";
                    return "Content";
                }))
        }).ToList();

        ModulesCollection.ItemsSource = null;
        ModulesCollection.ItemsSource = items;
        NoModulesLabel.IsVisible = items.Count == 0;
    }

    private void RefreshAssignments()
    {
        var items = _course.Assignments.Select(a =>
        {
            var sub = a.Submissions.FirstOrDefault(s => s.StudentId == _student.Id);
            string gradeDisplay;
            if (sub == null) gradeDisplay = "Not submitted";
            else if (!sub.PointsAwarded.HasValue) gradeDisplay = "Submitted — awaiting grade";
            else gradeDisplay = $"{sub.PointsAwarded}/{a.AvailablePoints} pts";

            string fileDisplay = sub?.AttachedFileName != null ? $"File: {sub.AttachedFileName}" : null;
            return new { a.Id, a.Name, a.DueDate, GradeDisplay = gradeDisplay, a.IsQuiz, FileDisplay = fileDisplay, HasFile = fileDisplay != null };
        }).ToList();

        AssignmentsCollection.ItemsSource = null;
        AssignmentsCollection.ItemsSource = items;
        NoAssignmentsLabel.IsVisible = items.Count == 0;
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var assignmentId = (int)((Button)sender).CommandParameter;
        var assignment = _course.Assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) return;

        var existing = assignment.Submissions.FirstOrDefault(s => s.StudentId == _student.Id);
        string prompt = assignment.IsQuiz && !string.IsNullOrWhiteSpace(assignment.QuizQuestion)
            ? $"Question: {assignment.QuizQuestion}"
            : existing != null
                ? $"You already submitted. Enter new response to resubmit:\n\nPrevious: {existing.Content}"
                : "Enter your response:";

        var content = await DisplayPromptAsync(assignment.Name, prompt, "Submit", "Cancel", "Your response here...");
        if (content == null) return;

        string attachedFilePath = null;
        string attachedFileName = null;
        bool attachFile = await DisplayAlert("Attach File", "Would you like to attach a file to this submission?", "Yes", "No");
        if (attachFile)
        {
            var picked = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select File to Attach" });
            if (picked != null)
            {
                attachedFilePath = picked.FullPath;
                attachedFileName = picked.FileName;
            }
        }

        if (existing != null)
        {
            existing.Content = content;
            existing.SubmissionDate = DateTime.Now;
            existing.AttachedFilePath = attachedFilePath;
            existing.AttachedFileName = attachedFileName;
        }
        else
        {
            assignment.Submissions.Add(new Submission
            {
                Id = assignment.Submissions.Count > 0 ? assignment.Submissions.Max(s => s.Id) + 1 : 1,
                StudentId = _student.Id,
                AssignmentId = assignment.Id,
                Content = content,
                SubmissionDate = DateTime.Now,
                AttachedFilePath = attachedFilePath,
                AttachedFileName = attachedFileName
            });
        }

        RefreshAssignments();
        var msg = attachedFileName != null ? $"Submitted with file: {attachedFileName}" : "Your response has been submitted.";
        await DisplayAlert("Submitted", msg, "OK");
    }

    private double CalculateCourseGrade()
    {
        if (_course.AssignmentGroups.Count > 0)
        {
            double totalWeight = 0, weightedSum = 0;
            foreach (var group in _course.AssignmentGroups.Where(g => g.Weight > 0))
            {
                var graded = group.Assignments
                    .Select(a => new { a, sub = a.Submissions.FirstOrDefault(s => s.StudentId == _student.Id) })
                    .Where(x => x.sub?.PointsAwarded.HasValue == true)
                    .ToList();
                if (graded.Count == 0) continue;
                double avail = graded.Sum(x => x.a.AvailablePoints);
                double earned = graded.Sum(x => (double)x.sub.PointsAwarded.Value);
                if (avail > 0) { weightedSum += (earned / avail) * group.Weight; totalWeight += group.Weight; }
            }
            return totalWeight > 0 ? weightedSum / totalWeight * 100 : -1;
        }

        var all = _course.Assignments
            .Select(a => new { a, sub = a.Submissions.FirstOrDefault(s => s.StudentId == _student.Id) })
            .Where(x => x.sub?.PointsAwarded.HasValue == true)
            .ToList();
        if (all.Count == 0) return -1;
        double totalAvail = all.Sum(x => x.a.AvailablePoints);
        double totalEarned = all.Sum(x => (double)x.sub.PointsAwarded.Value);
        return totalAvail > 0 ? totalEarned / totalAvail * 100 : -1;
    }

    private static string GetLetterGrade(double pct, Models.GradeRange ranges)
    {
        if (pct >= ranges.AMin) return "A";
        if (pct >= ranges.BMin) return "B";
        if (pct >= ranges.CMin) return "C";
        if (pct >= ranges.DMin) return "D";
        return "F";
    }
}
