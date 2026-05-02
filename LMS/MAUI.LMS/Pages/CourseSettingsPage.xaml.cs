using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class CourseSettingsPage : ContentPage
{
    public string CourseId { get; set; }

    public CourseSettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!int.TryParse(CourseId, out int id)) return;
        var course = CourseService.Current.GetById(id);
        if (course == null) return;

        CourseNameLabel.Text = $"{course.Name} ({course.Code})";
        var r = course.GradeRanges;
        AMinEntry.Text = r.AMin.ToString("F0");
        BMinEntry.Text = r.BMin.ToString("F0");
        CMinEntry.Text = r.CMin.ToString("F0");
        DMinEntry.Text = r.DMin.ToString("F0");
        FLabel.Text = $"Below {r.DMin:F0}%";

        DMinEntry.TextChanged += (s, e) =>
            FLabel.Text = double.TryParse(DMinEntry.Text, out double d) ? $"Below {d:F0}%" : "Below ?%";

        if (course.StartDate.HasValue) StartDatePicker.Date = course.StartDate.Value;
        if (course.EndDate.HasValue) EndDatePicker.Date = course.EndDate.Value;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(CourseId, out int id)) return;
        var course = CourseService.Current.GetById(id);
        if (course == null) return;

        if (!double.TryParse(AMinEntry.Text, out double aMin) ||
            !double.TryParse(BMinEntry.Text, out double bMin) ||
            !double.TryParse(CMinEntry.Text, out double cMin) ||
            !double.TryParse(DMinEntry.Text, out double dMin))
        {
            await DisplayAlert("Error", "All grade ranges must be valid numbers.", "OK");
            return;
        }

        if (!(aMin > bMin && bMin > cMin && cMin > dMin))
        {
            await DisplayAlert("Error", "Grade cutoffs must be in descending order (A > B > C > D).", "OK");
            return;
        }

        course.GradeRanges.AMin = aMin;
        course.GradeRanges.BMin = bMin;
        course.GradeRanges.CMin = cMin;
        course.GradeRanges.DMin = dMin;
        course.StartDate = StartDatePicker.Date;
        course.EndDate = EndDatePicker.Date;

        await DisplayAlert("Saved", "Settings updated.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
