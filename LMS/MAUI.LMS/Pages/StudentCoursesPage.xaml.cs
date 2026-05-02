using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

[QueryProperty(nameof(StudentId), "studentId")]
public partial class StudentCoursesPage : ContentPage
{
    public string StudentId { get; set; }

    public StudentCoursesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!int.TryParse(StudentId, out int id)) return;

        var student = StudentService.Current.GetById(id);
        if (student == null) return;

        Title = $"My Courses — {student.Name}";
        StudentNameLabel.Text = $"Courses for {student.Name} ({student.Code})";

        var enrolled = CourseService.Current.GetAll()
            .Where(c => c.Roster.Any(s => s.Id == student.Id))
            .ToList();

        CoursesCollection.ItemsSource = null;
        CoursesCollection.ItemsSource = enrolled;
        EmptyLabel.IsVisible = enrolled.Count == 0;
    }

    private async void OnViewCourseClicked(object sender, EventArgs e)
    {
        var courseId = (int)((Button)sender).CommandParameter;
        await Shell.Current.GoToAsync($"{nameof(StudentCourseDetailPage)}?studentId={StudentId}&courseId={courseId}");
    }
}
