using Academic_tracker.Models;
using Academic_tracker.Services;

namespace Academic_tracker.Pages;

public partial class AddAssessmentPage : ContentPage
{
    private readonly DBServices _db;
    private readonly int _moduleID;

    public AddAssessmentPage(DBServices db, int moduleID)
    {
        InitializeComponent();
        _db = db;
        _moduleID = moduleID;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = AssessmentNameEntry.Text?.Trim();
        var weightingStr = WeightingEntry.Text;
        var markStr = MarkObtainedEntry.Text;
        var totalStr = TotalMarkEntry.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(weightingStr) ||
            string.IsNullOrEmpty(markStr) || string.IsNullOrEmpty(totalStr))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (!double.TryParse(weightingStr, out double weighting) ||
            !double.TryParse(markStr, out double markObtained) ||
            !double.TryParse(totalStr, out double totalMark))
        {
            await DisplayAlert("Error", "Please enter valid numbers.", "OK");
            return;
        }

        var assessment = new Assessment
        {
            ModuleID = _moduleID,
            AssessmentName = name,
            Weighting = weighting,
            MarkObtained = markObtained,
            TotalMark = totalMark
        };

        await _db.AddAssessmentAsync(assessment);
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}