using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Academic_tracker.Models
{
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int AssessmentID { get; set; }
        public int ModuleID { get; set; }
        public string AssessmentName { get; set; }
        public double Weighting { get; set; }
        public double MarkObtained { get; set; }
        public double TotalMark { get; set; }
    }
}
