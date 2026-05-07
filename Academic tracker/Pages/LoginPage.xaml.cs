using Academic_tracker.Services;

namespace Academic_tracker.Pages;

public partial class LoginPage : ContentPage
{
    private readonly DBServices _db;

    public LoginPage(DBServices db)
    {
        InitializeComponent();
        _db = db;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        await _db.InitAsync();

        var user = await _db.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await DisplayAlert("Error", "Invalid email or password.", "OK");
            return;
        }

        // Persist the session so the user stays logged in
        Preferences.Set("loggedInUserID", user.UserID);

        await Navigation.PushAsync(new Dashboard(_db, user.UserID));
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_db));
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage(_db));
    }
}