namespace Academic_tracker.Services
{
    // Checks an assessment's numbers. Shared by AddAssessmentPage and the edit flow on ModuleDetailPage
    // so both apply the same rules.
    public static class AssessmentValidator
    {
        // Returns an error message to show the user, or null if the values are valid
        public static string? Validate(double weighting, double markObtained, double totalMark)
        {
            if (weighting <= 0)
            {
                return "Weighting must be greater than 0.";
            }

            // A single assessment cannot have weighting above 100%
            if (weighting > 100)
            {
                return "Weighting cannot exceed 100%.";
            }

            if (totalMark <= 0)
            {
                return "Total mark must be greater than 0.";
            }

            if (markObtained < 0)
            {
                return "Mark obtained cannot be negative.";
            }

            if (markObtained > totalMark)
            {
                return "Mark obtained cannot be greater than the total mark.";
            }

            return null;
        }
    }
}
