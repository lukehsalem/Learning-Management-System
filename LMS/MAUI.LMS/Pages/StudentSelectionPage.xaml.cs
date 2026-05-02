using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

public partial class StudentSelectionPage : ContentPage
{
    public StudentSelectionPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var students = StudentService.Current.GetAll();
        StudentsCollection.ItemsSource = null;
        StudentsCollection.ItemsSource = students;
        EmptyLabel.IsVisible = students.Count == 0;
    }

    private async void OnSelectStudentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        await Shell.Current.GoToAsync($"{nameof(StudentCoursesPage)}?studentId={id}");
    }
}
