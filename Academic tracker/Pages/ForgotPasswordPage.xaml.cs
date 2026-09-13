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
        var recoveryCode = RecoveryCodeEntry.Text?.Trim();
        var newPassword = NewPasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Validate all fields are filled
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(recoveryCode) ||
            string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
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

        // Validate password length (same rule as registration)
        if (newPassword.Length < 8)
        {
            await DisplayAlert("Error", "Password must be at least 8 characters.", "OK");
            return;
        }

        await _db.InitAsync();

        // Check the account exists and the recovery code matches. The same message is shown either way,
        // so this page can't be used to find out which emails have accounts.
        var user = await _db.GetUserByEmailAsync(email);
        if (user == null || !RecoveryCode.Verify(recoveryCode, user.RecoveryCodeHash))
        {
            await DisplayAlert("Error", "The email or recovery code is incorrect.", "OK");
            return;
        }

        // Hash the new password and save
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        // Recovery codes are single-use - replace the one that was just used
        var newRecoveryCode = RecoveryCode.Generate();
        user.RecoveryCodeHash = RecoveryCode.Hash(newRecoveryCode);

        await _db.UpdateUserAsync(user);

        await RecoveryCode.ShowAsync(this, "Password reset successfully. Please log in with your new password.", newRecoveryCode);
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
