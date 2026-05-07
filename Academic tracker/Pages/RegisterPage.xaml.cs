using Academic_tracker.Models;
using Academic_tracker.Services;
using System.Net.Mail;
using System.Diagnostics;

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

    // Validates password strength
    private bool IsValidPassword(string password)
    {
        // Minimum 8 characters
        return !String.IsNullOrEmpty(password) && password.Length >= 8;
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            // Retrive and trim user input
            var username = UsernameEntry.Text?.Trim();
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            // Validate username
            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert("Error", "Please enter a username.", "OK");
                return;
            }

            if (username.Length > 50)
            {
                await DisplayAlert("Error", "Please enter a username.", "OK");
                return;
            }

            // Validate that the email
            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Error", "Please enter an email.", "OK");
                return;
            }
            
            if (!IsValidEmail(email))
            {
                await DisplayAlert("Error", "Please enter a valid email address.", "OK");
                return;
            }

            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter a password.", "OK");
                return;
            }

            if (!IsValidPassword(password))
            {
                await DisplayAlert("Error", "Password must be at least 8 characters.", "OK");
                return;
            }

            try
            {
                // Initialize database connection
                await _db.InitAsync();

                // Check if an account with this email already exists
                var existing = await _db.GetUserByEmailAsync(email);
                if (existing != null)
                {
                    await DisplayAlert("Error", "An account with this email already exists.", "OK");
                    return;
                }

                // Create new user with hashed password
                var user = new User
                {
                    Username = username,
                    Email = email,
                    // Use Bcrypt to securely hash the password - never store plain text passwords
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                };

                // Store the new user in the database
                await _db.AddUserAsync(user);

                // Display success message
                await DisplayAlert("Success", "Account created! Please log in.", "OK");

                // Return to previous page (LoginPage)
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected error: " + ex.Message);
                await DisplayAlert("Error", "Registration failed. Please try again.", "OK");
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine("Unexpected error: " + ex.Message);
            await DisplayAlert("Error", "An unexpected error occurred.", "OK");
        }
    }

    /// Handles the back button click event and returns to the previous page without creating an account.
    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            // Navigate back to the previous page
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Navigation error: " + ex.Message);
            await DisplayAlert("Erorr", "Failed to navigate back.", "OK");
        }

    }
}