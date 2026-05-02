using MAUI.LMS.Pages;

namespace MAUI.LMS;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStudentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(StudentSelectionPage));
    }

    private async void OnTeacherClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TeacherMainPage));
    }
}
