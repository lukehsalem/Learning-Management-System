using MAUI.LMS.Models;
using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

public partial class TeacherMainPage : ContentPage
{
    public TeacherMainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshStudents();
        RefreshCourses();
    }

    private void RefreshStudents()
    {
        var students = StudentService.Current.GetAll();
        StudentsCollection.ItemsSource = null;
        StudentsCollection.ItemsSource = students;
        NoStudentsLabel.IsVisible = students.Count == 0;
    }

    private void RefreshCourses()
    {
        var courses = CourseService.Current.GetAll()
            .OrderBy(c => c.Semester ?? "")
            .ThenBy(c => c.Name)
            .ToList();
        CoursesCollection.ItemsSource = null;
        CoursesCollection.ItemsSource = courses;
        NoCoursesLabel.IsVisible = courses.Count == 0;
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Student", "Name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        var code = await DisplayPromptAsync("Add Student", "FSU ID (Code):");
        if (string.IsNullOrWhiteSpace(code)) return;
        var classification = await DisplayPromptAsync("Add Student", "Classification (e.g. Freshman, Sophomore):");
        if (classification == null) return;

        StudentService.Current.Add(name.Trim(), code.Trim(), classification.Trim());
        RefreshStudents();
    }

    private async void OnEditStudentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var student = StudentService.Current.GetById(id);
        if (student == null) return;

        var name = await DisplayPromptAsync("Edit Student", "Name:", initialValue: student.Name);
        if (name == null) return;
        var code = await DisplayPromptAsync("Edit Student", "FSU ID (Code):", initialValue: student.Code);
        if (code == null) return;
        var classification = await DisplayPromptAsync("Edit Student", "Classification:", initialValue: student.Classification);
        if (classification == null) return;

        StudentService.Current.Update(student,
            string.IsNullOrWhiteSpace(name) ? student.Name : name.Trim(),
            string.IsNullOrWhiteSpace(code) ? student.Code : code.Trim(),
            string.IsNullOrWhiteSpace(classification) ? student.Classification : classification.Trim());
        RefreshStudents();
    }

    private async void OnDeleteStudentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var student = StudentService.Current.GetById(id);
        if (student == null) return;

        bool confirm = await DisplayAlert("Delete Student",
            $"Remove {student.Name} from the system? This will unenroll them from all courses and delete their submissions.",
            "Delete", "Cancel");
        if (!confirm) return;

        StudentService.Current.Delete(student);
        RefreshStudents();
        RefreshCourses();
    }

    private async void OnAddCourseClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Course", "Course Name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        var code = await DisplayPromptAsync("Add Course", "Course Code (e.g. COP4870):");
        if (string.IsNullOrWhiteSpace(code)) return;
        var description = await DisplayPromptAsync("Add Course", "Description:");
        if (description == null) return;
        var semester = await DisplayPromptAsync("Add Course", "Semester (e.g. Fall 2026):");
        if (semester == null) return;
        var section = await DisplayPromptAsync("Add Course", "Section:");
        if (section == null) return;

        var course = CourseService.Current.Add(name.Trim(), code.Trim(), description.Trim());
        course.Semester = semester.Trim();
        course.Section = section.Trim();
        RefreshCourses();
    }

    private async void OnManageCourseClicked(object sender, EventArgs e)
    {
        var courseId = (int)((Button)sender).CommandParameter;
        await Shell.Current.GoToAsync($"{nameof(TeacherCourseDetailPage)}?courseId={courseId}");
    }

    private async void OnDeleteCourseClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var course = CourseService.Current.GetById(id);
        if (course == null) return;

        bool confirm = await DisplayAlert("Delete Course",
            $"Delete '{course.Name}'? This removes the course but not the students.",
            "Delete", "Cancel");
        if (!confirm) return;

        CourseService.Current.Delete(course);
        RefreshCourses();
    }
}
