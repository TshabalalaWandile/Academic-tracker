using Academic_tracker.Models;
using Academic_tracker.Services;

namespace Academic_tracker.Pages;

public partial class ModuleDetailPage : ContentPage
{
    private readonly DBServices _db;
    private readonly Module _module;

    public ModuleDetailPage(DBServices db, Module module)
    {
        InitializeComponent();
        _db = db;
        _module = module;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ModuleNameLabel.Text = _module.ModuleName;
        ModuleCodeLabel.Text = _module.ModuleCode;
        TargetMarkLabel.Text = $"Target: {_module.TargetMark}%";

        await LoadAssessments();
    }

    private async Task LoadAssessments()
    {
        await _db.InitAsync();
        var assessments = await _db.GetAssessmentsByModuleAsync(_module.ModuleID);
        AssessmentsCollection.ItemsSource = assessments;
    }

    private async void OnAddAssessmentClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddAssessmentPage(_db, _module.ModuleID));
    }
}