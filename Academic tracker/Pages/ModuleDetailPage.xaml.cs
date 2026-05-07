using Academic_tracker.Models;
using Academic_tracker.Services;
using System.Reflection.Metadata;

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
        TargetMarkLabel.Text = "Target: " + _module.TargetMark + "%";

        // Calculate and display running mark + status on this screen
        var runningMark = await _db.GetRunningMarkAsync(_module.ModuleID);
        RunningMarkLabel.Text = "Running Mark: "  + runningMark.ToString("F1") + "%";

        // Running mark is at or above the target
        if (runningMark >= _module.TargetMark)
        {
            StatusLabel.Text = "✅ On Track";
            StatusLabel.TextColor = Colors.LightGreen;
        }
        // Running mark is below the target but still within 80 % of it — close but not there yet
        else if (runningMark >= _module.TargetMark * 0.8)
        {
            StatusLabel.Text = "⚠️ At Risk";
            StatusLabel.TextColor = Colors.Orange;
        }
        // running mark is below 80% of the target — far off
        else
        {
            StatusLabel.Text = "❌ Off Track";
            StatusLabel.TextColor = Colors.OrangeRed;
        }

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

    private async void OnEditAssessmentClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Assessment assessment)
        {
            string name = await DisplayPromptAsync("Edit Assessment", "Assessment name:", initialValue: assessment.AssessmentName);
            if (string.IsNullOrWhiteSpace(name)) return;

            string weightingStr = await DisplayPromptAsync("Edit Assessment", "Weighting (%):", initialValue: assessment.Weighting.ToString(), keyboard: Keyboard.Numeric);
            if (!double.TryParse(weightingStr, out double weighting)) return;

            string markStr = await DisplayPromptAsync("Edit Assessment", "Mark obtained:", initialValue: assessment.MarkObtained.ToString(), keyboard: Keyboard.Numeric);
            if (!double.TryParse(markStr, out double markObtained)) return;

            string totalStr = await DisplayPromptAsync("Edit Assessment", "Total mark:", initialValue: assessment.TotalMark.ToString(), keyboard: Keyboard.Numeric);
            if (!double.TryParse(totalStr, out double totalMark)) return;

            // Check weighting cap excluding current assessment's weighting
            double currentTotal = await _db.GetTotalWeightingAsync(assessment.ModuleID);
            double newTotal = currentTotal - assessment.Weighting + weighting;
            if (newTotal > 100)
            {
                await DisplayAlert("Error", $"Total weighting cannot exceed 100%. You have {100 - (currentTotal - assessment.Weighting)}% remaining.", "OK");
                return;
            }

            assessment.AssessmentName = name;
            assessment.Weighting = weighting;
            assessment.MarkObtained = markObtained;
            assessment.TotalMark = totalMark;

            await _db.UpdateAssessmentAsync(assessment);
            await LoadAssessments();
        }
    }

    private async void OnDeleteAssessmentClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Assessment assessment)
        {
            bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {assessment.AssessmentName}?", "Yes", "No");
            if (!confirm) return;

            await _db.DeleteAssessmentAsync(assessment);
            await LoadAssessments();
        }
    }
}