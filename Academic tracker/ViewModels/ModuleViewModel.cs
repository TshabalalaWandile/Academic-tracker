using Academic_tracker.Models;

namespace Academic_tracker.ViewModels
{
    public class ModuleViewModel
    {
        public Module Module { get; set; }
        public string ModuleName => Module.ModuleName;
        public string ModuleCode => Module.ModuleCode;
        public double TargetMark => Module.TargetMark;

        // Null when no assessments have been marked yet
        public double? RunningMark { get; set; }

        public string RunningMarkDisplay => RunningMark.HasValue
            ? $"Running Mark: {RunningMark.Value:F1}% / Target: {TargetMark}%"
            : $"Running Mark: – / Target: {TargetMark}%";

        public string StatusDisplay
        {
            get
            {
                if (!RunningMark.HasValue) return "No marks yet";
                if (RunningMark >= TargetMark) return "✅ On Track";
                if (RunningMark >= TargetMark * 0.8) return "⚠️ At Risk";
                return "❌ Off Track";
            }
        }

        public string StatusColor
        {
            get
            {
                if (!RunningMark.HasValue) return "Gray";
                if (RunningMark >= TargetMark) return "Green";
                if (RunningMark >= TargetMark * 0.8) return "Orange";
                return "Red";
            }
        }
    }
}