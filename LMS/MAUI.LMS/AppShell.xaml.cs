using MAUI.LMS.Pages;

namespace MAUI.LMS;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(StudentSelectionPage), typeof(StudentSelectionPage));
        Routing.RegisterRoute(nameof(StudentCoursesPage), typeof(StudentCoursesPage));
        Routing.RegisterRoute(nameof(StudentCourseDetailPage), typeof(StudentCourseDetailPage));
        Routing.RegisterRoute(nameof(TeacherMainPage), typeof(TeacherMainPage));
        Routing.RegisterRoute(nameof(TeacherCourseDetailPage), typeof(TeacherCourseDetailPage));
        Routing.RegisterRoute(nameof(TeacherModuleContentPage), typeof(TeacherModuleContentPage));
    }
}
