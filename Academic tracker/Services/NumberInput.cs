using System.Globalization;

namespace Academic_tracker.Services
{
    // Parses numbers typed by the user. Accepts both "72.5" and "72,5": phones set to South African English (en-ZA)
    // use a comma as the decimal separator, but numeric keyboards often only offer a dot.
    public static class NumberInput
    {
        public static bool TryParse(string? text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && double.IsFinite(value);
        }
    }
}
