using Academic_tracker.Models;
using Academic_tracker.Services;
using System.Net.Mail;

namespace Academic_tracker.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly DBServices _db;

    public RegisterPage(DBServices db)
    {
        InitializeComponent();
        _db = db;
    }

    // This method validates that the input is a proper internet email address
    private bool IsValidEmail(string email)
    {
        try
        {
            var emailAddress = new MailAddress(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        // Validate that the email entered is in the correct format
        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            return;
        }

        await _db.InitAsync();

        var existing = await _db.GetUserByEmailAsync(email);
        if (existing != null)
        {
            await DisplayAlert("Error", "An account with this email already exists.", "OK");
            return;
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await _db.AddUserAsync(user);
        await DisplayAlert("Success", "Account created! Please log in.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}