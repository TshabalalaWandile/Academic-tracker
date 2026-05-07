using Academic_tracker.Services;

namespace Academic_tracker.Pages;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly DBServices _db;

    public ForgotPasswordPage(DBServices db)
    {
        InitializeComponent();
        _db = db;
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var newPassword = NewPasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Validate all fields are filled
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        // Validate email format
        if (!email.Contains("@") || !email.Contains("."))
        {
            await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            return;
        }

        // Validate passwords match
        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        // Validate password length
        if (newPassword.Length < 6)
        {
            await DisplayAlert("Error", "Password must be at least 6 characters.", "OK");
            return;
        }

        await _db.InitAsync();

        // Check the email exists in the database
        var user = await _db.GetUserByEmailAsync(email);
        if (user == null)
        {
            await DisplayAlert("Error", "No account found with that email address.", "OK");
            return;
        }

        // Hash the new password and save
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.UpdateUserAsync(user);

        await DisplayAlert("Success", "Password reset successfully. Please log in with your new password.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}