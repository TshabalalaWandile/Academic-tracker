using Academic_tracker.Models;
using Academic_tracker.Services;
using Academic_tracker.ViewModels;
using System.Diagnostics;

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
        // Initialize database and retrieve user's modules
        await _db.InitAsync();
        var modules = await _db.GetModulesByUserAsync(_userID);

        // Create view models with running mark calculations
        var viewModels = new List<ModuleViewModel>();
        foreach (var module in modules)
        {
            // Calculate weighted average of all assessments for this module
            var runningMark = await _db.GetRunningMarkAsync(module.ModuleID);
            viewModels.Add(new ModuleViewModel
            {
                Module = module,
                RunningMark = runningMark
            });
        }

        // Bind the modules to the collection view for display
        ModulesCollection.ItemsSource = viewModels;
    }

    // Validates module code format
    private bool IsValidModuleCode(string code)
    {
        return !string.IsNullOrWhiteSpace(code) &&
               code.Length >= 2 &&
               code.Length <= 10 &&
               code.All(char.IsLetterOrDigit);
    }

    private async void OnAddModuleClicked(object sender, EventArgs e)
    {
        // Prompt for module name
        string moduleName = await DisplayPromptAsync("New Module", "Enter module name:");
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            await DisplayAlert("Error", "Module name is required.", "OK");
            return;
        }

        if (moduleName.Length > 100)
        {
            await DisplayAlert("Error", "Module name must be 100 characters or less.", "OK");
            return;
        }

        // Prompt for module code
        string moduleCode = await DisplayPromptAsync("New Module", "Enter module code (e.g., COMP301):");
        if (!IsValidModuleCode(moduleCode))
        {
            await DisplayAlert("Error", "Module code must be 2-10 alphanumeric characters.", "OK");
            return;
        }


        try
        {
            // Check if this module code already exists for this user
            bool exists = await _db.ModuleCodeExistsAsync(moduleCode, _userID);
            if (exists)
            {
                await DisplayAlert("Error", "This module code already exists.", "OK");
                return;
            }

            // Prompt for target mark (numeric input)
            string targetStr = await DisplayPromptAsync("New Module", "Enter target mark (0-100%):", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(targetStr))
            {
                await DisplayAlert("Error", "Target mark is required.", "OK");
                return;
            }

            if (!double.TryParse(targetStr, out double targetMark))
            {
                await DisplayAlert("Error", "Please enter a valid number for the target mark.", "OK");
                return;
            }

            if (targetMark <= 0 || targetMark > 100)
            {
                await DisplayAlert("Error", "Target mark must be between 0 and 100%.", "OK");
                return;
            }

            // Create and save new module
            var module = new Module
            {
                UserID = _userID,
                ModuleName = moduleName,
                ModuleCode = moduleCode.ToUpper(),
                TargetMark = targetMark
            };

            await _db.AddModuleAsync(module);
            await LoadModules();
            await DisplayAlert("Success", "Module added successfully.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error adding module: " + ex.Message);
            await DisplayAlert("Error", "Failed to add module. Please try again.", "OK");
        }
    }

    // Handles when a module is tapped in the collection view. Navigates to ModuleDetailPage to view and manage assessments for that module.
    private async void OnModuleTapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is Border border && border.BindingContext is ModuleViewModel vm)
            {
                // Navigate to the detail page for this module
                await Navigation.PushAsync(new ModuleDetailPage(_db, vm.Module));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Navigation error: " + ex.Message);
            await DisplayAlert("Error", "Failed to navigate to module details.", "OK");
        }
    }

    // Handles the edit module button click. Allows user to update module name, code, and target mark and validates that the new module code doesn't conflict with other modules.
    private async void OnEditModuleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ModuleViewModel vm)
        {
            // Prompt for updated module name
            string moduleName = await DisplayPromptAsync("Edit Module", "Module name:", initialValue: vm.ModuleName);
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            // Prompt for updated module code
            string moduleCode = await DisplayPromptAsync("Edit Module", "Module code:", initialValue: vm.ModuleCode);
            if (string.IsNullOrWhiteSpace(moduleCode))
            {
                return;
            }

            // Re-prompting until the user either enters a unique code or cancels (clears the field and taps OK). 
            while (await _db.ModuleCodeExistsAsync(moduleCode, _userID, vm.Module.ModuleID))
            {
                await DisplayAlert("Error", "This module code already exists.", "OK");

                moduleCode = await DisplayPromptAsync("Edit Module", "Module code (must be unique):", initialValue: moduleCode);

                if (string.IsNullOrWhiteSpace(moduleCode))
                {
                    // user cancelled
                    return;
                }
            }

            // Prompt for updated target mark
            string targetStr = await DisplayPromptAsync("Edit Module", "Target mark (%):", initialValue: vm.TargetMark.ToString(), keyboard: Keyboard.Numeric);
            if (!double.TryParse(targetStr, out double targetMark)) return;

            // Update the module with new values
            vm.Module.ModuleName = moduleName;
            vm.Module.ModuleCode = moduleCode.ToUpper();
            vm.Module.TargetMark = targetMark;

            await _db.UpdateModuleAsync(vm.Module);

            // Refresh the display
            await LoadModules();
        }
    }

    // Handles the delete module button click. Prompts for confirmation before deleting the module and all associated assessments.
    private async void OnDeleteModuleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ModuleViewModel vm)
        {
            // Ask for confirmation before deleting
            bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete " + vm.ModuleName + "?", "Yes", "No");
            if (!confirm) return;

            // Delete all assessments under this module first (cascade delete)
            await _db.DeleteAssessmentsByModuleAsync(vm.Module.ModuleID);

            // Now safe to delete the module itself
            await _db.DeleteModuleAsync(vm.Module);

            // Refresh the display
            await LoadModules();
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");

        if (!confirm)
        {
            return;
        }

        // Clear the saved session
        Preferences.Remove("loggedInUserID");

        // Navigate back to login, clearing the navigation stack
        Application.Current!.MainPage = new NavigationPage(new LoginPage(_db));
    }
}