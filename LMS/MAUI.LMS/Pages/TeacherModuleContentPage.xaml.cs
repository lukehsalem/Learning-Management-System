using MAUI.LMS.Models;
using MAUI.LMS.Services;

namespace MAUI.LMS.Pages;

[QueryProperty(nameof(CourseId), "courseId")]
[QueryProperty(nameof(ModuleId), "moduleId")]
public partial class TeacherModuleContentPage : ContentPage
{
    public string CourseId { get; set; }
    public string ModuleId { get; set; }

    private Course _course;
    private Module _module;

    public TeacherModuleContentPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!int.TryParse(CourseId, out int courseId) || !int.TryParse(ModuleId, out int moduleId)) return;

        _course = CourseService.Current.GetById(courseId);
        _module = _course?.Modules.FirstOrDefault(m => m.Id == moduleId);
        if (_module == null) return;

        ModuleNameLabel.Text = _module.Name;
        CourseNameLabel.Text = _course.Name;
        RefreshContent();
    }

    private void RefreshContent()
    {
        var items = _module.Content.Select(c =>
        {
            string typeLabel, detail;
            if (c is AssignmentContent ac)
            {
                typeLabel = "Assignment";
                var a = _course.Assignments.FirstOrDefault(x => x.Id == ac.AssignmentId);
                detail = a?.Name ?? $"(Assignment Id {ac.AssignmentId})";
            }
            else if (c is FileContent fc)
            {
                typeLabel = "File";
                detail = fc.FileName;
            }
            else if (c is PageContent pc)
            {
                typeLabel = "Page";
                detail = pc.Text;
            }
            else
            {
                typeLabel = "Content";
                detail = "";
            }
            return new { c.Id, TypeLabel = typeLabel, Detail = detail };
        }).ToList();

        ContentCollection.ItemsSource = null;
        ContentCollection.ItemsSource = items;
        EmptyLabel.IsVisible = items.Count == 0;
    }

    private async void OnAddContentClicked(object sender, EventArgs e)
    {
        var type = await DisplayActionSheet("Content Type", "Cancel", null, "Assignment", "File", "Page");
        if (type == null || type == "Cancel") return;

        int newId = _module.Content.Count > 0 ? _module.Content.Max(c => c.Id) + 1 : 1;

        switch (type)
        {
            case "Assignment":
                if (_course.Assignments.Count == 0)
                {
                    await DisplayAlert("Add Content", "No assignments in this course.", "OK");
                    return;
                }
                var assignOptions = _course.Assignments.Select(a => a.Name).ToArray();
                var chosen = await DisplayActionSheet("Select Assignment", "Cancel", null, assignOptions);
                if (chosen == null || chosen == "Cancel") return;
                var assignment = _course.Assignments[Array.IndexOf(assignOptions, chosen)];
                _module.Content.Add(new AssignmentContent { Id = newId, AssignmentId = assignment.Id });
                break;

            case "File":
                var fileName = await DisplayPromptAsync("Add File Content", "File Name:");
                if (string.IsNullOrWhiteSpace(fileName)) return;
                _module.Content.Add(new FileContent { Id = newId, FileName = fileName.Trim() });
                break;

            case "Page":
                var text = await DisplayPromptAsync("Add Page Content", "Page Text:");
                if (string.IsNullOrWhiteSpace(text)) return;
                _module.Content.Add(new PageContent { Id = newId, Text = text.Trim() });
                break;
        }

        RefreshContent();
    }

    private async void OnRemoveContentClicked(object sender, EventArgs e)
    {
        var id = (int)((Button)sender).CommandParameter;
        var item = _module.Content.FirstOrDefault(c => c.Id == id);
        if (item == null) return;
        bool confirm = await DisplayAlert("Remove Content", "Remove this content item from the module?", "Remove", "Cancel");
        if (!confirm) return;
        _module.Content.Remove(item);
        RefreshContent();
    }
}
