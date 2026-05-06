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

    private async void OnEditModuleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ModuleViewModel vm)
        {
            string moduleName = await DisplayPromptAsync("Edit Module", "Module name:", initialValue: vm.ModuleName);
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            string moduleCode = await DisplayPromptAsync("Edit Module", "Module code:", initialValue: vm.ModuleCode);
            if (string.IsNullOrWhiteSpace(moduleCode)) return;

            string targetStr = await DisplayPromptAsync("Edit Module", "Target mark (%):", initialValue: vm.TargetMark.ToString(), keyboard: Keyboard.Numeric);
            if (!double.TryParse(targetStr, out double targetMark)) return;

            vm.Module.ModuleName = moduleName;
            vm.Module.ModuleCode = moduleCode;
            vm.Module.TargetMark = targetMark;

            await _db.UpdateModuleAsync(vm.Module);
            await LoadModules();
        }
    }

    private async void OnDeleteModuleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ModuleViewModel vm)
        {
            bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {vm.ModuleName}?", "Yes", "No");
            if (!confirm) return;

            await _db.DeleteModuleAsync(vm.Module);
            await LoadModules();
        }
    }
}