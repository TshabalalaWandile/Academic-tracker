using System.Security.Cryptography;

namespace Academic_tracker.Services
{
    // Recovery codes let a user reset a forgotten password. The app is offline, so there is no email step -
    // the code (shown once at registration, stored only as a hash) proves the person resetting owns the account.
    public static class RecoveryCode
    {
        // Leaves out 0/O and 1/I/L so the code is easy to copy by hand
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        // Generates a random code like "7KQ4-M2XP-9RTD"
        public static string Generate()
        {
            var chars = new char[12];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }

            var code = new string(chars);
            return $"{code[..4]}-{code[4..8]}-{code[8..]}";
        }

        // Removes dashes/spaces and uppercases, so "7kq4 m2xp 9rtd" matches "7KQ4-M2XP-9RTD"
        public static string Normalize(string code)
        {
            return new string(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        }

        public static string Hash(string code)
        {
            return BCrypt.Net.BCrypt.HashPassword(Normalize(code));
        }

        public static bool Verify(string code, string? hash)
        {
            return !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(Normalize(code), hash);
        }

        // Shows the code to the user once, with an option to copy it to the clipboard
        public static async Task ShowAsync(Page page, string title, string code)
        {
            bool copy = await page.DisplayAlert(title,
                $"Your recovery code is:\n\n{code}\n\nSave it somewhere safe. You'll need it to reset your password, and it won't be shown again.",
                "Copy code", "I've saved it");

            if (copy)
            {
                await Clipboard.Default.SetTextAsync(code);
            }
        }
    }
}
