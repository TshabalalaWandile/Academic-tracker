using Academic_tracker.Models;

namespace Academic_tracker.ViewModels
{
    public class ModuleViewModel
    {
        public Module Module { get; set; }
        public string ModuleName => Module.ModuleName;
        public string ModuleCode => Module.ModuleCode;
        public double TargetMark => Module.TargetMark;
        public double RunningMark { get; set; }

        public string RunningMarkDisplay => $"Running Mark: {RunningMark:F1}% / Target: {TargetMark}%";

        public string StatusDisplay
        {
            get
            {
                if (RunningMark >= TargetMark) return "✅ On Track";
                if (RunningMark >= TargetMark * 0.8) return "⚠️ At Risk";
                return "❌ Off Track";
            }
        }

        public string StatusColor
        {
            get
            {
                if (RunningMark >= TargetMark) return "Green";
                if (RunningMark >= TargetMark * 0.8) return "Orange";
                return "Red";
            }
        }
    }
}