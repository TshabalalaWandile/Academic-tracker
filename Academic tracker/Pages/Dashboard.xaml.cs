using Academic_tracker.Models;
using Academic_tracker.Services;
using Academic_tracker.ViewModels;

namespace Academic_tracker.Pages;

public partial class Dashboard : ContentPage
{
    private readonly DBServices _db;
    private readonly int _userID;

    public Dashboard(DBServices db, int userID)
    {
        InitializeComponent();
        _db = db;
        _userID = userID;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadModules();
    }

    private async Task LoadModules()
    {
        await _db.InitAsync();
        var modules = await _db.GetModulesByUserAsync(_userID);

        var viewModels = new List<ModuleViewModel>();
        foreach (var module in modules)
        {
            var runningMark = await _db.GetRunningMarkAsync(module.ModuleID);
            viewModels.Add(new ModuleViewModel
            {
                Module = module,
                RunningMark = runningMark
            });
        }

        ModulesCollection.ItemsSource = viewModels;
    }

    private async void OnAddModuleClicked(object sender, EventArgs e)
    {
        string moduleName = await DisplayPromptAsync("New Module", "Enter module name:");
        if (string.IsNullOrWhiteSpace(moduleName)) return;

        string moduleCode = await DisplayPromptAsync("New Module", "Enter module code:");
        if (string.IsNullOrWhiteSpace(moduleCode)) return;

        string targetStr = await DisplayPromptAsync("New Module", "Enter target mark (%):", keyboard: Keyboard.Numeric);
        if (!double.TryParse(targetStr, out double targetMark)) return;

        var module = new Module
        {
            UserID = _userID,
            ModuleName = moduleName,
            ModuleCode = moduleCode,
            TargetMark = targetMark
        };

        await _db.AddModuleAsync(module);
        await LoadModules();
    }

    private async void OnModuleTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is ModuleViewModel vm)
        {
            await Navigation.PushAsync(new ModuleDetailPage(_db, vm.Module));
        }
    }
}